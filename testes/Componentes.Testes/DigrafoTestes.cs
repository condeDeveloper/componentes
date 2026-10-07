using Conde.Componentes;
using Xunit;

namespace Conde.Componentes.Testes;

public class DigrafoTestes
{
    [Fact]
    public void OSorteioEhDeterminista()
    {
        var a = new Sorteio(7);
        var b = new Sorteio(7);

        for (var i = 0; i < 1000; i++) Assert.Equal(a.Proximo(), b.Proximo());
    }

    /// <summary>
    /// A fracao precisa cobrir os DOIS LADOS do meio: um deslocamento de 33 em vez
    /// de 32 deixaria o gerador com 31 bits, e toda decisao de "esta aresta
    /// existe?" com chance acima de meio sairia errada, com os numeros continuando
    /// plausiveis.
    /// </summary>
    [Fact]
    public void AFracaoCobreOsDoisLadosDoMeio()
    {
        var sorteio = new Sorteio(7);
        var acima = 0;
        var maior = 0.0;

        for (var i = 0; i < 10_000; i++)
        {
            var f = sorteio.Fracao();

            Assert.InRange(f, 0.0, 1.0);
            if (f > 0.5) acima++;
            if (f > maior) maior = f;
        }

        Assert.InRange(acima, 4_700, 5_300);
        Assert.True(maior > 0.999, $"o maior sorteio foi {maior}");
    }

    [Fact]
    public void OTranspostoInverteTodasAsSetas()
    {
        var grafo = Digrafos.ComTantasArestas(30, 60, 3);
        var invertido = grafo.Transposto();

        Assert.Equal(grafo.Arestas, invertido.Arestas);

        for (var v = 0; v < grafo.Vertices; v++)
        {
            Assert.Equal(
                grafo.Saindo(v).OrderBy(x => x),
                invertido.Chegando(v).OrderBy(x => x));

            Assert.Equal(
                grafo.Chegando(v).OrderBy(x => x),
                invertido.Saindo(v).OrderBy(x => x));
        }
    }

    [Fact]
    public void OTranspostoDoTranspostoEhOOriginal()
    {
        var grafo = Digrafos.ComTantasArestas(25, 50, 5);
        var voltou = grafo.Transposto().Transposto();

        for (var v = 0; v < grafo.Vertices; v++)
            Assert.Equal(grafo.Saindo(v).OrderBy(x => x), voltou.Saindo(v).OrderBy(x => x));
    }

    [Fact]
    public void OCaminhoNaoTemComponenteNenhumaComMaisDeUm()
    {
        var grafo = Digrafos.Caminho(50);

        Assert.Equal(49, grafo.Arestas);
        Assert.Equal(50, Tarjan.Achar(grafo).Quantas);
    }

    [Fact]
    public void OCicloEhUmaComponenteSo()
    {
        var grafo = Digrafos.Ciclo(50);

        Assert.Equal(50, grafo.Arestas);
        Assert.Equal(1, Tarjan.Achar(grafo).Quantas);
    }

    [Fact]
    public void OsCiclosEmFilaDaoUmaComponentePorCiclo()
    {
        foreach (var (quantos, tamanho) in new[] { (3, 3), (5, 4), (10, 7) })
        {
            var (grafo, esperado) = Digrafos.CiclosEmFila(quantos, tamanho);

            Assert.Equal(esperado, Tarjan.Achar(grafo).Quantas);
            Assert.Equal(quantos * tamanho, grafo.Vertices);
        }
    }

    [Fact]
    public void OGrafoSemCicloNaoTemComponenteNenhumaComMaisDeUm()
    {
        for (ulong semente = 1; semente <= 10; semente++)
        {
            var grafo = Digrafos.SemCiclo(30, 0.2, semente);

            Assert.True(Juiz.NaoTemCiclo(grafo));
            Assert.Equal(30, Tarjan.Achar(grafo).Quantas);
        }
    }

    [Fact]
    public void VerticesSoltosSaoComponentesDeUmSo()
    {
        var grafo = new Digrafo(9);

        Assert.Equal(0, grafo.Arestas);
        Assert.Equal(9, Tarjan.Achar(grafo).Quantas);
        Assert.Equal(9, Kosaraju.Achar(grafo).Quantas);
        Assert.Equal(9, Juiz.Quantas(Juiz.PorDefinicao(grafo)));
    }

    /// <summary>
    /// O grafo que separa as duas versoes do Tarjan tem QUATRO vertices e tres
    /// componentes: o zero sozinho, o ciclo do um com o dois, e o tres sozinho.
    /// </summary>
    [Fact]
    public void OGrafoQueSeparaTemTresComponentes()
    {
        var grafo = Digrafos.OQueSeparaOCertoDoPlausivel();
        var de = Juiz.PorDefinicao(grafo);

        Assert.Equal(4, grafo.Vertices);
        Assert.Equal(5, grafo.Arestas);
        Assert.Equal(3, Juiz.Quantas(de));
        Assert.Equal(de[1], de[2]);
        Assert.NotEqual(de[0], de[3]);
        Assert.NotEqual(de[0], de[1]);
    }
}
