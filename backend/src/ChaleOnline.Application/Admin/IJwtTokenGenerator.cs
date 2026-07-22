namespace ChaleOnline.Application.Admin;

public record TokenGeradoDto(string Token, DateTime ExpiraEmUtc);

/// <summary>
/// AD-5: a validade fixa de 2h (sem sliding window) é decidida dentro da implementação
/// Infrastructure, não passada como parâmetro daqui.
/// </summary>
public interface IJwtTokenGenerator
{
    TokenGeradoDto Gerar(string adminId, string email);
}
