namespace Conde.Componentes;

/// <summary>
/// O SORTEIO, escrito aqui em vez de vir da biblioteca.
///
/// A documentacao do sorteio de biblioteca nao promete a mesma sequencia entre
/// versoes nem entre sistemas, e a integracao continua roda em tres. "A versao
/// errada do Tarjan errou em 41 de 500 grafos" com grafos diferentes em cada
/// sistema nao e uma medida: sao tres medidas que ninguem pode comparar.
/// </summary>
public sealed class Sorteio
{
    private ulong estado;

    public Sorteio(ulong semente) => estado = semente == 0 ? 1 : semente;

    public uint Proximo()
    {
        estado = estado * 6364136223846793005UL + 1442695040888963407UL;
        return (uint)(estado >> 32);
    }

    public int Ate(int limite) => (int)(Proximo() % (uint)limite);

    public double Fracao() => Proximo() / 4294967296.0;
}

/// <summary>
/// Um GRAFO DIRIGIDO, guardado como lista de vizinhos de ida e de volta.
///
/// As duas listas existem porque um dos dois algoritmos daqui precisa das duas:
/// Kosaraju percorre o grafo uma vez de ida e outra de volta, e e so disso que
/// sai a resposta dele. Tarjan nunca olha a lista de volta.
///
/// Que dois algoritmos tao diferentes cheguem sempre a mesma particao e um fato
/// sobre o PROBLEMA, nao sobre eles, e e esse fato que faz um servir de
/// conferencia para o outro.
/// </summary>
public sealed class Digrafo
{
    private readonly List<int>[] saindo;
    private readonly List<int>[] chegando;

    public Digrafo(int vertices)
    {
        Vertices = vertices;
        saindo = new List<int>[vertices];
        chegando = new List<int>[vertices];

        for (var v = 0; v < vertices; v++) { saindo[v] = []; chegando[v] = []; }
    }

    public int Vertices { get; }

    public int Arestas { get; private set; }

    /// <summary>
    /// Quantas vezes uma aresta foi OLHADA.
    ///
    /// E esta a unidade do repositorio, e nao o relogio. Segundo depende de
    /// maquina, de compilador e de quem mais esta rodando no momento; aresta
    /// olhada e o mesmo numero nos tres sistemas da integracao continua.
    /// </summary>
    public long Olhadas { get; private set; }

    public void Olhou() => Olhadas++;

    public void ZerarOlhadas() => Olhadas = 0;

    public void Ligar(int de, int para)
    {
        saindo[de].Add(para);
        chegando[para].Add(de);
        Arestas++;
    }

    public IReadOnlyList<int> Saindo(int vertice) => saindo[vertice];

    public IReadOnlyList<int> Chegando(int vertice) => chegando[vertice];

    /// <summary>O mesmo grafo com todas as setas invertidas.</summary>
    public Digrafo Transposto()
    {
        var outro = new Digrafo(Vertices);

        for (var v = 0; v < Vertices; v++)
            foreach (var u in saindo[v]) outro.Ligar(u, v);

        return outro;
    }
}

/// <summary>
/// Os GRAFOS que separam os algoritmos, e por que eles sao assim.
///
/// Grafo sorteado com muita aresta nao separa nada: quase tudo vira uma
/// componente so. Grafo sorteado com pouca aresta tambem nao: quase tudo vira
/// vertice sozinho. O interessante fica numa faixa estreita no meio, e os casos
/// que realmente separam sao os ARMADOS.
/// </summary>
public static class Digrafos
{
    /// <summary>Um grafo sorteado, cada aresta com a mesma chance.</summary>
    public static Digrafo Sorteado(int vertices, double chance, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Digrafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = 0; b < vertices; b++)
                if (a != b && sorteio.Fracao() < chance) grafo.Ligar(a, b);

        return grafo;
    }

    /// <summary>Um grafo sorteado com numero fixo de arestas, que e o jeito de ficar na faixa interessante.</summary>
    public static Digrafo ComTantasArestas(int vertices, int arestas, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Digrafo(vertices);

        for (var k = 0; k < arestas; k++)
        {
            var a = sorteio.Ate(vertices);
            var b = sorteio.Ate(vertices);

            if (a != b) grafo.Ligar(a, b);
        }

        return grafo;
    }

    /// <summary>Um CAMINHO: n vertices em fila, n componentes de um vertice cada.</summary>
    public static Digrafo Caminho(int vertices)
    {
        var grafo = new Digrafo(vertices);

        for (var v = 0; v + 1 < vertices; v++) grafo.Ligar(v, v + 1);

        return grafo;
    }

    /// <summary>Um CICLO: n vertices, UMA componente com todos eles.</summary>
    public static Digrafo Ciclo(int vertices)
    {
        var grafo = Caminho(vertices);

        grafo.Ligar(vertices - 1, 0);

        return grafo;
    }

    /// <summary>
    /// VARIOS CICLOS em fila, ligados num sentido so.
    ///
    /// E o caso em que a resposta e obvia de escrever e nada obvia de calcular:
    /// cada ciclo e uma componente, e a ordem em que elas aparecem e a ordem da
    /// fila. Serve para conferir a afirmacao mais bonita do Tarjan, que e a de que
    /// ele entrega as componentes ja em ordem.
    /// </summary>
    public static (Digrafo Grafo, int Quantas) CiclosEmFila(int quantos, int tamanho)
    {
        var grafo = new Digrafo(quantos * tamanho);

        for (var c = 0; c < quantos; c++)
        {
            var inicio = c * tamanho;

            for (var i = 0; i < tamanho; i++) grafo.Ligar(inicio + i, inicio + (i + 1) % tamanho);

            if (c + 1 < quantos) grafo.Ligar(inicio, inicio + tamanho);
        }

        return (grafo, quantos);
    }

    /// <summary>Um grafo sem ciclo nenhum: toda componente tem um vertice so.</summary>
    public static Digrafo SemCiclo(int vertices, double chance, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Digrafo(vertices);

        // so para a frente, entao nao ha como voltar
        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++)
                if (sorteio.Fracao() < chance) grafo.Ligar(a, b);

        return grafo;
    }

    /// <summary>
    /// O MENOR GRAFO que separa o Tarjan certo do Tarjan plausivel.
    ///
    /// Quatro vertices e cinco arestas. O 0 aponta para o 1 e para o 3; o 1 e o 2
    /// formam um ciclo entre si; e o 3 aponta de volta para o 1.
    ///
    /// A busca entra pelo 0, desce ate o ciclo do 1 e do 2, FECHA essa componente,
    /// volta ao 0 e so entao desce pelo 3. Quando o 3 olha para o 1, o 1 ja esta
    /// fechado e fora da pilha.
    ///
    /// E ai esta o detalhe. A versao plausivel do algoritmo usa o indice de
    /// descoberta do 1 para baixar o do 3, e como o 1 foi descoberto antes, o 3
    /// deixa de ser raiz da propria componente e cai junto com o 0. A resposta
    /// vira duas componentes onde ha tres.
    ///
    /// Ir ate la nao e o mesmo que voltar de la, e a pilha e exatamente a
    /// estrutura que guarda essa diferenca.
    /// </summary>
    public static Digrafo OQueSeparaOCertoDoPlausivel()
    {
        var grafo = new Digrafo(4);

        grafo.Ligar(0, 1);
        grafo.Ligar(1, 2);
        grafo.Ligar(2, 1);
        grafo.Ligar(0, 3);
        grafo.Ligar(3, 1);

        return grafo;
    }

    /// <summary>
    /// Os grafos PEQUENOS, onde a definicao cabe em forca bruta.
    ///
    /// O juiz deste repositorio responde "existe caminho de u ate v e de v ate u?"
    /// com uma busca para cada par, e isso e caro de proposito: ele nao precisa ser
    /// bom, precisa ser a definicao.
    /// </summary>
    public static IEnumerable<(string Nome, Digrafo Grafo)> Pequenos(int quantos = 40)
    {
        yield return ("caminho de 6", Caminho(6));
        yield return ("ciclo de 6", Ciclo(6));
        yield return ("tres ciclos em fila", CiclosEmFila(3, 3).Grafo);
        yield return ("o que separa", OQueSeparaOCertoDoPlausivel());
        yield return ("sem ciclo", SemCiclo(10, 0.3, 5));
        yield return ("vertices soltos", new Digrafo(7));

        for (ulong semente = 1; semente <= (ulong)quantos; semente++)
            yield return ($"sorteado {semente}", ComTantasArestas(12, 14, semente));
    }

    public static IEnumerable<(string Nome, Digrafo Grafo)> Grandes()
    {
        yield return ("caminho de 200 mil", Caminho(200_000));
        yield return ("ciclo de 200 mil", Ciclo(200_000));
        yield return ("mil ciclos de 100", CiclosEmFila(1_000, 100).Grafo);
        yield return ("sorteado 5 mil (1 aresta)", ComTantasArestas(5_000, 5_000, 7));
        yield return ("sorteado 5 mil (2 arestas)", ComTantasArestas(5_000, 10_000, 11));
        yield return ("sorteado 2 mil denso", Sorteado(2_000, 0.002, 13));
        yield return ("sem ciclo 2 mil", SemCiclo(2_000, 0.004, 17));
    }
}
