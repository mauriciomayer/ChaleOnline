namespace ChaleOnline.Application.Admin;

public enum StatusAutenticacao
{
    Sucesso,
    CredenciaisInvalidas,
    ContaBloqueada,
}

public record ResultadoAutenticacao(StatusAutenticacao Status, string? AdminId, string? Email);
