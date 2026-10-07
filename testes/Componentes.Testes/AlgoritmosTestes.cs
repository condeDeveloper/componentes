using Conde.Componentes;
using Xunit;

namespace Conde.Componentes.Testes;

public class AlgoritmosTestes
{
    /// <summary>
    /// OS DOIS ALGORITMOS dao exatamente a mesma particao, e a particao esta certa
    /// pela definicao.
    ///
    /// As duas metades importam. Que Tarjan e Kosaraju concordem seria so uma
    /// coincidencia se nenhum dos dois fosse conferido contra a definicao, e e a
    /// definicao que decide.
    /// </summary>
    [Fact]
    public void OsDoisConcordamEntreSiEComADefinicao()
    {
        foreach (var (nome, grafo) in Digrafos.Pequenos(60))
        {
            var definicao = Juiz.PorDefinicao(grafo);
            var tarjan = Tarjan.Achar(grafo);
            var kosaraju = Kosaraju.Achar(grafo);

            Assert.True(Juiz.MesmaParticao(definicao, tarjan.De), $"{nome}: Tarjan");
            Assert.True(Juiz.MesmaParticao(definicao, kosaraju.De), $"{nome}: Kosaraju");
            Assert.True(Juiz.EstaCerta(grafo, tarjan.De), nome);
            Assert.Equal(tarjan.Quantas, kosaraju.Quantas);
        }
    }

    /// <summary>
    /// E em grafos sorteados de densidades bem diferentes tambem, que e onde o
    /// numero de componentes vai de "todos sozinhos" a "todos juntos".
    /// </summary>
    [Fact]
    public void OsDoisConcordamEmTodasAsDensidades()
    {
        foreach (var arestas in new[] { 5, 10, 20, 40, 80, 160 })
            for (ulong semente = 1; semente <= 25; semente++)
            {
                var grafo = Digrafos.ComTantasArestas(20, arestas, semente);
                var tarjan = Tarjan.Achar(grafo);

                Assert.True(Juiz.EstaCerta(grafo, tarjan.De), $"{arestas} arestas, semente {semente}");
                Assert.True(Juiz.MesmaParticao(tarjan.De, Kosaraju.Achar(grafo).De));
            }
    }

    /// <summary>
    /// O ACHADO: a versao plausivel do Tarjan erra, e o grafo que a pega tem
    /// QUATRO vertices.
    ///
    /// A unica diferenca entre as duas e uma conferencia de tres palavras, "ainda
    /// esta na pilha", e sem ela o algoritmo junta componentes que nao se tocam.
    /// </summary>
    [Fact]
    public void OPlausivelErraNumGrafoDeQuatroVertices()
    {
        var grafo = Digrafos.OQueSeparaOCertoDoPlausivel();

        Assert.Equal(3, Tarjan.Achar(grafo).Quantas);
        Assert.Equal(2, Tarjan.AcharSemConferirAPilha(grafo).Quantas);
        Assert.False(Juiz.EstaCerta(grafo, Tarjan.AcharSemConferirAPilha(grafo).De));
    }

    /// <summary>
    /// O ACHADO QUE ME CORRIGIU: eu esperava que a versao plausivel passasse quase
    /// sempre, e e o contrario.
    ///
    /// Erro sutil costuma ser raro de aparecer, e foi isso que eu escrevi primeiro
    /// no texto. Em quinhentos grafos sorteados de OITO vertices, ela discorda da
    /// versao certa em 472. Em grafos maiores, em todos os quinhentos.
    ///
    /// O erro e facil de cometer e facil de pegar. A unica coisa que ele precisa
    /// para sobreviver e nao existir juiz nenhum do outro lado.
    /// </summary>
    [Fact]
    public void OPlausivelErraNaEsmagadoraMaioriaDosGrafosSorteados()
    {
        var discordou = 0;

        for (ulong semente = 1; semente <= 500; semente++)
        {
            var grafo = Digrafos.ComTantasArestas(8, 8, semente);

            if (Tarjan.Achar(grafo).Quantas != Tarjan.AcharSemConferirAPilha(grafo).Quantas) discordou++;
        }

        Assert.Equal(472, discordou);
    }

    /// <summary>
    /// E ele sempre erra para MENOS: juntar componentes que nao se tocam so pode
    /// diminuir a contagem.
    /// </summary>
    [Fact]
    public void OPlausivelSoErraParaMenos()
    {
        foreach (var (nome, grafo) in Digrafos.Pequenos(60))
            Assert.True(
                Tarjan.AcharSemConferirAPilha(grafo).Quantas <= Tarjan.Achar(grafo).Quantas, nome);
    }

    /// <summary>
    /// O TARJAN entrega as componentes em ordem topologica INVERSA da condensacao,
    /// e isso vem de graca junto da resposta.
    /// </summary>
    [Fact]
    public void OTarjanEntregaAsComponentesEmOrdemInversa()
    {
        foreach (var (nome, grafo) in Digrafos.Pequenos(40))
            Assert.True(Juiz.EmOrdemInversa(grafo, Tarjan.Achar(grafo).De), nome);
    }

    /// <summary>E o KOSARAJU na ordem direta, pelo mesmo preco.</summary>
    [Fact]
    public void OKosarajuEntregaAsComponentesEmOrdemDireta()
    {
        foreach (var (nome, grafo) in Digrafos.Pequenos(40))
            Assert.True(Juiz.EmOrdemDireta(grafo, Kosaraju.Achar(grafo).De), nome);
    }

    /// <summary>
    /// A CONDENSACAO nunca tem ciclo, e e esse o motivo pratico de tudo isto
    /// existir: um emaranhado dirigido vira um grafo com ordem.
    /// </summary>
    [Fact]
    public void ACondensacaoNuncaTemCiclo()
    {
        foreach (var (nome, grafo) in Digrafos.Pequenos(40))
        {
            var de = Tarjan.Achar(grafo).De;

            Assert.True(Juiz.NaoTemCiclo(Juiz.Condensacao(grafo, de)), nome);
        }
    }

    /// <summary>
    /// O KOSARAJU olha o DOBRO de arestas: ele passa duas vezes, Tarjan uma.
    /// </summary>
    [Fact]
    public void OKosarajuOlhaODobroDeArestas()
    {
        foreach (var (nome, grafo) in Digrafos.Grandes())
        {
            var tarjan = Tarjan.Achar(grafo);
            var kosaraju = Kosaraju.Achar(grafo);

            Assert.Equal(grafo.Arestas, tarjan.Olhadas);
            Assert.Equal(2 * grafo.Arestas, kosaraju.Olhadas);
        }
    }

    /// <summary>
    /// A PROFUNDIDADE da busca chega a duzentos mil num caminho de duzentos mil
    /// vertices, e esse e o numero de quadros que uma versao recursiva teria
    /// empilhado.
    ///
    /// Que este teste rode nos tres sistemas da integracao continua e o ponto: um
    /// repositorio vizinho levou "Test host process crashed: Stack overflow" no
    /// macOS com uma profundidade menor que essa.
    /// </summary>
    [Fact]
    public void ABuscaChegaADuzentosMilDeProfundidadeSemEstourarNada()
    {
        var grafo = Digrafos.Caminho(200_000);
        var tarjan = Tarjan.Achar(grafo);

        Assert.Equal(200_000, tarjan.MaiorProfundidade);
        Assert.Equal(200_000, tarjan.Quantas);
        Assert.Equal(200_000, Kosaraju.Achar(grafo).Quantas);
    }

    [Fact]
    public void NoCicloGiganteADivisaoEhUmaSo()
    {
        var grafo = Digrafos.Ciclo(200_000);

        Assert.Equal(1, Tarjan.Achar(grafo).Quantas);
        Assert.Equal(1, Kosaraju.Achar(grafo).Quantas);
    }

    [Fact]
    public void AOrdemDeTerminoCobreTodoMundoUmaVezSo()
    {
        foreach (var (nome, grafo) in Digrafos.Pequenos(20))
        {
            var ordem = Kosaraju.PorTermino(grafo);

            Assert.Equal(grafo.Vertices, ordem.Count);
            Assert.Equal(grafo.Vertices, ordem.Distinct().Count());
        }
    }
}
