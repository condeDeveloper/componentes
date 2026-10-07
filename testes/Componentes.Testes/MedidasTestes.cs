using Conde.Componentes;
using Xunit;

namespace Conde.Componentes.Testes;

/// <summary>
/// Os testes das TABELAS que o medidor imprime.
///
/// Eles existem porque numero em README que ninguem confere envelhece errado em
/// silencio: alguem troca uma constante, a tabela muda e o texto continua dizendo
/// a frase antiga. Aqui o teste afirma sobre o mesmo objeto que vira linha
/// impressa.
/// </summary>
public class MedidasTestes
{
    /// <summary>
    /// A linha principal: os dois algoritmos certos batem com a definicao em todas
    /// as linhas, e o plausivel nao.
    /// </summary>
    [Fact]
    public void OsDoisCertosBatemEmTodaLinhaEOPlausivelNao()
    {
        var linhas = Medidas.ContraADefinicao(12);

        Assert.Equal(18, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.TarjanBate, l.Grafo));
        Assert.All(linhas, l => Assert.True(l.KosarajuBate, l.Grafo));
        Assert.All(linhas, l => Assert.Equal(l.PelaDefinicao, l.Tarjan));
        Assert.All(linhas, l => Assert.Equal(l.PelaDefinicao, l.Kosaraju));

        Assert.Contains(linhas, l => !l.PlausivelBate);

        var separa = linhas.Single(l => l.Grafo == "o que separa");

        Assert.Equal(3, separa.PelaDefinicao);
        Assert.Equal(2, separa.Plausivel);
        Assert.False(separa.PlausivelBate);
    }

    /// <summary>
    /// A frase do texto: a versao plausivel erra em 472 de 500 grafos de oito
    /// vertices, e em todos os 500 a partir de quarenta.
    /// </summary>
    [Fact]
    public void OPlausivelErraEmQuaseTodoGrafoSorteado()
    {
        var linhas = Medidas.OPlausivelErra();

        Assert.Equal(5, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.Discordou > 400, $"{l.Vertices} vertices: {l.Discordou}"));

        var menor = linhas.First();

        Assert.Equal(8, menor.Vertices);
        Assert.Equal(472, menor.Discordou);

        var maior = linhas.Last();

        Assert.Equal(80, maior.Vertices);
        Assert.Equal(500, maior.Discordou);

        // e sempre para menos: juntar demais so pode diminuir a contagem
        Assert.All(linhas, l => Assert.True(l.ComponentesAMenos > 0));
    }

    /// <summary>
    /// A frase do texto: a condensacao nunca tem ciclo, o Tarjan entrega em ordem
    /// inversa e o Kosaraju em ordem direta, em toda linha.
    /// </summary>
    [Fact]
    public void AsDuasOrdensVemDeGracaEmTodaLinha()
    {
        var linhas = Medidas.Ordens();

        Assert.Equal(26, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.CondensacaoSemCiclo, l.Grafo));
        Assert.All(linhas, l => Assert.True(l.TarjanInversa, l.Grafo));
        Assert.All(linhas, l => Assert.True(l.KosarajuDireta, l.Grafo));
    }

    /// <summary>
    /// A frase do texto: o Kosaraju olha o DOBRO, e a profundidade chega ao numero
    /// de vertices no caminho em fila.
    /// </summary>
    [Fact]
    public void OKosarajuOlhaODobroEAProfundidadeChegaAoTamanho()
    {
        var linhas = Medidas.Custos();

        Assert.Equal(7, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(l.Arestas, l.OlhadasTarjan));
        Assert.All(linhas, l => Assert.Equal(2L * l.Arestas, l.OlhadasKosaraju));

        var caminho = linhas.Single(l => l.Grafo == "caminho de 200 mil");

        Assert.Equal(200_000, caminho.Componentes);
        Assert.Equal(200_000, caminho.MaiorProfundidade);

        var ciclo = linhas.Single(l => l.Grafo == "ciclo de 200 mil");

        Assert.Equal(1, ciclo.Componentes);
        Assert.Equal(200_000, ciclo.MaiorProfundidade);
    }

    /// <summary>
    /// A frase do texto: a componente gigante aparece quase de uma vez, por volta
    /// de uma aresta por vertice.
    /// </summary>
    [Fact]
    public void AComponenteGiganteApareceQuaseDeUmaVez()
    {
        var linhas = Medidas.Tamanhos();

        Assert.Equal(7, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(5_000, l.Vertices));

        // com poucas arestas, todo mundo sozinho
        Assert.Equal(1, linhas[0].MaiorComponente);
        Assert.Equal(5_000, linhas[0].Sozinhos);

        // e com o dobro delas, a maior engole a maior parte do grafo
        Assert.True(linhas[^1].MaiorComponente > 4_000);
        Assert.True(linhas[^1].Sozinhos < 500);

        // a maior componente so cresce quando se acrescenta aresta
        for (var i = 1; i < linhas.Count; i++)
            Assert.True(linhas[i].MaiorComponente >= linhas[i - 1].MaiorComponente);
    }
}
