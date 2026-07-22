using ChaleOnline.Domain;

namespace ChaleOnline.Infrastructure;

internal static class SeedData
{
    // Foto de capa do catálogo/card (Home) — mesma foto real pra todos os 12 Chalés (pousada real
    // única disponível pro portfólio; ver ConstruirMidias() pra galeria completa do Detalhe).
    private const string FotoCapa = "/media/pousada/deck-vista.jpg";

    public static readonly Chale[] Chales =
    [
        // Tipo A — 2 quartos / 1 banheiro (6 unidades)
        new(1, "Pinheiro Bravo", TipoChale.A, 2, 1, 420m, FotoCapa),
        new(2, "Trilha da Neblina", TipoChale.A, 2, 1, 435m, FotoCapa),
        new(3, "Cabana do Vale", TipoChale.A, 2, 1, 410m, FotoCapa),
        new(4, "Recanto da Araucária", TipoChale.A, 2, 1, 445m, FotoCapa),
        new(5, "Clareira Dourada", TipoChale.A, 2, 1, 425m, FotoCapa),
        new(6, "Refúgio do Riacho", TipoChale.A, 2, 1, 430m, FotoCapa),

        // Tipo B — 3 quartos / 1 banheiro (4 unidades)
        new(7, "Vista da Serra", TipoChale.B, 3, 1, 620m, FotoCapa),
        new(8, "Morada Alpina", TipoChale.B, 3, 1, 635m, FotoCapa),
        new(9, "Chalé do Bosque", TipoChale.B, 3, 1, 610m, FotoCapa),
        new(10, "Encosta Verde", TipoChale.B, 3, 1, 645m, FotoCapa),

        // Tipo C — 4 quartos / 2 banheiros (2 unidades)
        new(11, "Grande Refúgio", TipoChale.C, 4, 2, 890m, FotoCapa),
        new(12, "Casa da Montanha", TipoChale.C, 4, 2, 910m, FotoCapa),
    ];

    private static readonly string[] NomesComodidades =
    [
        "Lareira",
        "Deck com hidromassagem",
        "Vista para o bosque",
        "Wi-Fi",
        "Estacionamento privativo",
        "Churrasqueira",
    ];

    private static readonly (string Comentario, int Nota)[] ComentariosAvaliacao =
    [
        ("Chalé impecável, exatamente como nas fotos.", 5),
        ("Ótima localização e muito aconchegante.", 5),
        ("Bom custo-benefício, voltaríamos com certeza.", 4),
        ("Estrutura boa, mas o Wi-Fi falhou algumas vezes.", 3),
        ("Vista incrível, recomendo para casais.", 5),
        ("Limpeza impecável e anfitrião muito atencioso.", 4),
    ];

    // Fotos + vídeo reais de uma pousada (fornecidos por Mauricio em docs/, 2026-07-20) — mesmo
    // conjunto de 5 fotos + 1 vídeo pra todos os 12 Chalés (só há material fotográfico de um imóvel
    // real disponível pro portfólio). Ordem pensada pra galeria do Detalhe: vista do deck como hero
    // (paisagem, melhor enquadramento no recorte 16:9), depois fachada, sala, os dois quartos, vídeo.
    private static readonly (string Url, TipoMidia Tipo)[] GaleriaReal =
    [
        ("/media/pousada/deck-vista.jpg", TipoMidia.Foto),
        ("/media/pousada/fachada-entrada.jpg", TipoMidia.Foto),
        ("/media/pousada/sala-lareira.jpg", TipoMidia.Foto),
        ("/media/pousada/quarto-casal.jpg", TipoMidia.Foto),
        ("/media/pousada/quarto-duplo.jpg", TipoMidia.Foto),
        ("/media/pousada/tour-virtual.mp4", TipoMidia.Video),
    ];

    public static readonly ChaleMidia[] ChaleMidias = ConstruirMidias();
    public static readonly ChaleComodidade[] ChaleComodidades = ConstruirComodidades();
    public static readonly Avaliacao[] Avaliacoes = ConstruirAvaliacoes();

    private static ChaleMidia[] ConstruirMidias()
    {
        var midias = new List<ChaleMidia>();
        var id = 1;

        foreach (var chale in Chales)
        {
            for (var ordem = 0; ordem < GaleriaReal.Length; ordem++)
            {
                var (url, tipo) = GaleriaReal[ordem];
                midias.Add(new ChaleMidia(id++, chale.Id, url, tipo, ordem));
            }
        }

        return [.. midias];
    }

    // 3 comodidades por Chalé, rotacionando o mesmo pool de 6 nomes — exercita o mapeamento de ícone no frontend.
    private static ChaleComodidade[] ConstruirComodidades()
    {
        var comodidades = new List<ChaleComodidade>();
        var id = 1;

        for (var i = 0; i < Chales.Length; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                var nome = NomesComodidades[(i + j) % NomesComodidades.Length];
                comodidades.Add(new ChaleComodidade(id++, Chales[i].Id, nome));
            }
        }

        return [.. comodidades];
    }

    // 2 avaliações por Chalé, rotacionando o mesmo pool de comentários. Passo de 2 por Chalé
    // (em vez de 1) garante que Chalés adjacentes recebem pares de comentários disjuntos.
    private static Avaliacao[] ConstruirAvaliacoes()
    {
        var avaliacoes = new List<Avaliacao>();
        var id = 1;

        for (var i = 0; i < Chales.Length; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                var (comentario, nota) = ComentariosAvaliacao[(i * 2 + j) % ComentariosAvaliacao.Length];
                avaliacoes.Add(new Avaliacao(id++, Chales[i].Id, nota, comentario));
            }
        }

        return [.. avaliacoes];
    }
}
