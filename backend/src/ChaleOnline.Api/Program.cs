using System.Text;
using ChaleOnline.Api.Admin;
using ChaleOnline.Api.Chales;
using ChaleOnline.Api.Reservas;
using ChaleOnline.Application.Admin;
using ChaleOnline.Application.Chales;
using ChaleOnline.Application.Email;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Infrastructure;
using ChaleOnline.Infrastructure.Admin;
using ChaleOnline.Infrastructure.Email;
using ChaleOnline.Infrastructure.Jobs;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ListarChalesUseCase>();
builder.Services.AddScoped<BuscarChalesDisponiveisUseCase>();
builder.Services.AddScoped<ObterChaleDetalheUseCase>();
builder.Services.AddScoped<CriarReservaUseCase>();
builder.Services.AddScoped<ConfirmarPagamentoUseCase>();
builder.Services.AddScoped<ConsultarReservaUseCase>();
builder.Services.AddScoped<CancelarReservasExpiradasUseCase>();
builder.Services.AddScoped<CancelarReservasExpiradasJob>();
builder.Services.AddScoped<IEmailSender, LogEmailSender>();
builder.Services.AddScoped<AutenticarAdminUseCase>();
builder.Services.AddScoped<ObterVisaoDiariaUseCase>();
builder.Services.AddScoped<ObterRelatorioMensalUseCase>();

// HorarioBrasil.FusoSaoPaulo resolve TimeZoneInfo.FindSystemTimeZoneById só na primeira chamada —
// força isso a acontecer aqui no startup (fail-fast, mesmo espírito da validação de Jwt:Key
// abaixo) em vez de deixar a falha aparecer só na primeira requisição real a
// /api/admin/visao-diaria caso o id IANA não resolva num deploy futuro (achado de code review, 2026-07-20).
_ = HorarioBrasil.DiaCorrente(DateTime.UtcNow);

// AD-5 — JWT bearer, validade fixa de 2h sem tolerância de expiração (ClockSkew=Zero, remove os
// 5 min de tolerância padrão do .NET, coerente com "sem sliding/idle-reset window").
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Configuração 'Jwt' não encontrada.");
// Falha rápido e claro no startup em vez de um erro opaco do Microsoft.IdentityModel no primeiro
// login/validação de token com uma chave vazia ou curta demais (achado de code review, 2026-07-20).
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("Configuração 'Jwt:Key' ausente ou curta demais (mínimo 32 caracteres, requisito do HMAC-SHA256).");
}

// AddIdentity() (chamado dentro de AddInfrastructure) já registrou o esquema de cookie
// "Identity.Application" como DefaultAuthenticateScheme/DefaultChallengeScheme — o overload
// AddAuthentication(string) só define DefaultScheme, então sem sobrescrever os três aqui
// explicitamente, uma requisição sem token cai no desafio de cookie (redirect 302 pra uma
// rota de login que não existe) em vez do 401 esperado do JWT bearer.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        };
    });
builder.Services.AddAuthorization();

// Desligado nos testes de integração (ChalesApiFactory) — evita que múltiplas instâncias paralelas
// de WebApplicationFactory<Program> disputem a instalação do schema do Hangfire.MySqlStorage
// ("Table 'job' already exists"). CancelarReservasExpiradasJob continua registrado e testável via
// DI normalmente — só o storage/server/dashboard/recurring job real do Hangfire são pulados.
var hangfireHabilitado = !builder.Configuration.GetValue<bool>("Hangfire:Desabilitado");

if (hangfireHabilitado)
{
    // AD-4 — job de cancelamento roda dentro do processo da Api/Infrastructure, nunca no Next.js.
    // "Allow User Variables=True" é exigido pelo Hangfire.MySqlStorage (usa variáveis de sessão MySQL
    // internamente); não é necessário na connection string usada pelo EF Core, então é acrescentado só
    // aqui, sem alterar ConnectionStrings:ChaleOnlineDb.
    var hangfireConnectionString = builder.Configuration.GetConnectionString("ChaleOnlineDb")
        + ";Allow User Variables=True";

    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseStorage(new MySqlStorage(hangfireConnectionString, new MySqlStorageOptions { PrepareSchemaIfNecessary = true })));
    builder.Services.AddHangfireServer();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    if (hangfireHabilitado)
    {
        // Dashboard só em dev, por conveniência de debug local — ARCHITECTURE-SPINE.md não menciona
        // dashboard em nenhum lugar; não é um requisito da história.
        app.UseHangfireDashboard();
    }
}
else
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await Results.Problem(statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(context);
}));

app.UseAuthentication();
app.UseAuthorization();

app.MapChalesEndpoints();
app.MapReservasEndpoints();
app.MapAdminEndpoints();
app.MapPainelEndpoints();

if (hangfireHabilitado)
{
    // Decisão confirmada com Mauricio (2026-07-20): varredura recorrente, não job agendado por
    // Reserva individual — mais simples, e a atualização condicional (WHERE Status=AguardandoPagamento)
    // já é o mecanismo real de "só um vence" (AC #3), então rodar a varredura repetidamente é inofensivo.
    RecurringJob.AddOrUpdate<CancelarReservasExpiradasJob>(
        "cancelar-reservas-expiradas",
        job => job.ExecutarAsync(CancellationToken.None),
        "*/5 * * * *");
}

app.Run();

public partial class Program;
