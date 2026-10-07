using Conde.Componentes;
using Xunit;

namespace Conde.Componentes.Testes;

/// <summary>
/// O juiz tambem precisa de juiz.
///
/// Todo veredicto deste repositorio e "a particao bate com a definicao". Se a
/// forca bruta tivesse um erro, os dois algoritmos seriam julgados contra uma
/// referencia errada, e o erro mais provavel dela, que e juntar demais ou separar
/// demais, nao daria sinal nenhum.
/// </summary>
public class JuizTestes
{
    [Fact]
    public void AAlcancabilidadeAcertaOsCasosObvios()
    {
        var grafo = new Digrafo(4);

        grafo.Ligar(0, 1);
        grafo.Ligar(1, 2);

        var alcanca = Juiz.Alcanca(grafo);

        Assert.True(alcanca[0][0]);
        Assert.True(alcanca[0][1]);
        Assert.True(alcanca[0][2]);
        Assert.False(alcanca[0][3]);
        Assert.False(alcanca[2][0]);
        Assert.False(alcanca[1][0]);
    }

    /// <summary>
    /// ALCANCAR NOS DOIS SENTIDOS nao e o mesmo que alcancar: o caminho de ida nao
    /// promete o de volta, e e so disso que o problema inteiro trata.
    /// </summary>
    [Fact]
    public void AlcancarNaoEhOMesmoQueAlcancarNosDoisSentidos()
    {
        var grafo = Digrafos.Caminho(5);
        var alcanca = Juiz.Alcanca(grafo);

        Assert.True(alcanca[0][4]);
        Assert.False(alcanca[4][0]);
        Assert.Equal(5, Juiz.Quantas(Juiz.PorDefinicao(grafo)));
    }

    /// <summary>
    /// A conferencia de "esta certa" pega as DUAS maneiras de errar: juntar quem
    /// nao se alcanca e separar quem se alcanca.
    ///
    /// So a primeira metade aceitaria a particao que poe cada vertice sozinho; so a
    /// segunda aceitaria a que poe todo mundo junto.
    /// </summary>
    [Fact]
    public void AConferenciaPegaAsDuasManeirasDeErrar()
    {
        var grafo = Digrafos.CiclosEmFila(3, 3).Grafo;
        var certa = Tarjan.Achar(grafo).De;

        Assert.True(Juiz.EstaCerta(grafo, certa));

        // todo mundo junto
        Assert.False(Juiz.EstaCerta(grafo, new int[grafo.Vertices]));

        // cada um sozinho
        Assert.False(Juiz.EstaCerta(grafo, [.. Enumerable.Range(0, grafo.Vertices)]));
    }

    /// <summary>
    /// A comparacao de particoes olha os GRUPOS e nao os numeros: os tres caminhos
    /// daqui numeram de tres maneiras diferentes a mesma resposta.
    /// </summary>
    [Fact]
    public void AComparacaoDeParticoesIgnoraOsNomesDosGrupos()
    {
        Assert.True(Juiz.MesmaParticao([0, 0, 1, 1], [5, 5, 9, 9]));
        Assert.True(Juiz.MesmaParticao([2, 1, 0], [0, 1, 2]));
        Assert.False(Juiz.MesmaParticao([0, 0, 1], [0, 1, 1]));
        Assert.False(Juiz.MesmaParticao([0, 0], [0, 0, 0]));
    }

    [Fact]
    public void ACondensacaoJuntaOsVerticesDeCadaComponente()
    {
        var (grafo, quantas) = Digrafos.CiclosEmFila(4, 5);
        var de = Tarjan.Achar(grafo).De;
        var condensado = Juiz.Condensacao(grafo, de);

        Assert.Equal(quantas, condensado.Vertices);
        Assert.Equal(quantas - 1, condensado.Arestas);
        Assert.True(Juiz.NaoTemCiclo(condensado));
    }

    [Fact]
    public void ACondensacaoDeUmCicloEhUmVerticeSo()
    {
        var grafo = Digrafos.Ciclo(50);
        var condensado = Juiz.Condensacao(grafo, Tarjan.Achar(grafo).De);

        Assert.Equal(1, condensado.Vertices);
        Assert.Equal(0, condensado.Arestas);
    }

    [Fact]
    public void ADeteccaoDeCicloAcertaOsCasosObvios()
    {
        Assert.True(Juiz.NaoTemCiclo(Digrafos.Caminho(10)));
        Assert.False(Juiz.NaoTemCiclo(Digrafos.Ciclo(10)));
        Assert.True(Juiz.NaoTemCiclo(new Digrafo(5)));

        for (ulong semente = 1; semente <= 10; semente++)
            Assert.True(Juiz.NaoTemCiclo(Digrafos.SemCiclo(20, 0.3, semente)));
    }

    /// <summary>
    /// A DEFINICAO numera pelo MENOR MEMBRO, e isso e o que torna a resposta dela
    /// comparavel com qualquer outra sem depender de ordem nenhuma.
    /// </summary>
    [Fact]
    public void ADefinicaoNumeraPeloMenorMembro()
    {
        var grafo = Digrafos.CiclosEmFila(3, 3).Grafo;
        var de = Juiz.PorDefinicao(grafo);

        Assert.Equal([0, 0, 0, 3, 3, 3, 6, 6, 6], de);
    }

    [Fact]
    public void ADefinicaoRecusaGrafoGrandeDemaisEmVezDeDemorarPraSempre() =>
        Assert.Throws<ArgumentException>(() => Juiz.PorDefinicao(new Digrafo(1_000)));
}
