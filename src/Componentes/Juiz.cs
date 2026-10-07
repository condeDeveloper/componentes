namespace Conde.Componentes;

/// <summary>
/// O JUIZ, e ele nao e um terceiro algoritmo esperto.
///
/// Comparar Tarjan com Kosaraju diz quando eles discordam e nao diz qual esta
/// certo, e dois que concordam e erram juntos sao piores que um que erra sozinho.
/// Aqui o juiz e a DEFINICAO, escrita do jeito mais burro possivel: dois vertices
/// estao na mesma componente quando existe caminho de um ate o outro NOS DOIS
/// SENTIDOS, e descobrir isso e fazer uma busca saindo de cada vertice.
///
/// Custa o numero de vertices vezes o numero de arestas, e precisa custar. Um
/// juiz esperto seria um juiz com os mesmos pontos cegos do algoritmo julgado.
/// </summary>
public static class Juiz
{
    /// <summary>Quem alcanca quem, por uma busca saindo de cada vertice.</summary>
    public static bool[][] Alcanca(Digrafo grafo)
    {
        var tabela = new bool[grafo.Vertices][];

        for (var raiz = 0; raiz < grafo.Vertices; raiz++)
        {
            var visto = new bool[grafo.Vertices];
            var pilha = new Stack<int>();

            visto[raiz] = true;
            pilha.Push(raiz);

            while (pilha.Count > 0)
                foreach (var u in grafo.Saindo(pilha.Pop()))
                {
                    if (visto[u]) continue;

                    visto[u] = true;
                    pilha.Push(u);
                }

            tabela[raiz] = visto;
        }

        return tabela;
    }

    /// <summary>
    /// A particao PELA DEFINICAO: cada vertice recebe o menor numero de vertice
    /// com quem ele se alcanca nos dois sentidos.
    ///
    /// Numerar pelo menor membro, e nao pela ordem em que apareceu, e o que torna
    /// esta resposta comparavel com qualquer outra sem depender de ordem nenhuma.
    /// </summary>
    public static int[] PorDefinicao(Digrafo grafo)
    {
        if (grafo.Vertices > 600) throw new ArgumentException("a definicao so cabe ate 600 vertices", nameof(grafo));

        var alcanca = Alcanca(grafo);
        var de = new int[grafo.Vertices];

        for (var v = 0; v < grafo.Vertices; v++)
        {
            de[v] = v;

            for (var u = 0; u < v; u++)
                if (alcanca[u][v] && alcanca[v][u]) { de[v] = de[u]; break; }
        }

        return de;
    }

    /// <summary>
    /// Duas numeracoes descrevem a MESMA particao?
    ///
    /// A pergunta nao e se os numeros sao iguais, e sim se os grupos sao. Tarjan
    /// numera na ordem em que fecha, Kosaraju na ordem em que abre, e a definicao
    /// pelo menor membro: os tres dao numeros diferentes para a mesma resposta.
    /// </summary>
    public static bool MesmaParticao(int[] a, int[] b)
    {
        if (a.Length != b.Length) return false;

        var deAParaB = new Dictionary<int, int>();
        var deBParaA = new Dictionary<int, int>();

        for (var v = 0; v < a.Length; v++)
        {
            if (deAParaB.TryGetValue(a[v], out var esperadoB) && esperadoB != b[v]) return false;
            if (deBParaA.TryGetValue(b[v], out var esperadoA) && esperadoA != a[v]) return false;

            deAParaB[a[v]] = b[v];
            deBParaA[b[v]] = a[v];
        }

        return true;
    }

    public static int Quantas(int[] de) => de.Distinct().Count();

    /// <summary>
    /// A CONDENSACAO: cada componente vira um vertice so.
    ///
    /// Ela e o motivo pratico de todo este repositorio existir. Um grafo dirigido
    /// qualquer pode ser um emaranhado sem ordem nenhuma; a condensacao dele e
    /// SEMPRE um grafo sem ciclo, e com isso todo o ferramental de ordenacao
    /// topologica volta a valer.
    /// </summary>
    public static Digrafo Condensacao(Digrafo grafo, int[] de)
    {
        var quantas = de.Length == 0 ? 0 : de.Max() + 1;
        var condensado = new Digrafo(quantas);
        var jaTem = new HashSet<(int, int)>();

        for (var v = 0; v < grafo.Vertices; v++)
            foreach (var u in grafo.Saindo(v))
            {
                if (de[v] == de[u] || !jaTem.Add((de[v], de[u]))) continue;

                condensado.Ligar(de[v], de[u]);
            }

        return condensado;
    }

    /// <summary>Um grafo nao tem ciclo quando a ordenacao topologica cobre todos os vertices.</summary>
    public static bool NaoTemCiclo(Digrafo grafo)
    {
        var entrando = new int[grafo.Vertices];

        for (var v = 0; v < grafo.Vertices; v++)
            foreach (var u in grafo.Saindo(v)) entrando[u]++;

        var fila = new Queue<int>();

        for (var v = 0; v < grafo.Vertices; v++)
            if (entrando[v] == 0) fila.Enqueue(v);

        var saiu = 0;

        while (fila.Count > 0)
        {
            saiu++;

            foreach (var u in grafo.Saindo(fila.Dequeue()))
                if (--entrando[u] == 0) fila.Enqueue(u);
        }

        return saiu == grafo.Vertices;
    }

    /// <summary>
    /// As componentes do TARJAN saem em ordem topologica INVERSA da condensacao, e
    /// isso e de graca.
    ///
    /// Ou seja: para toda aresta que atravessa de uma componente para outra, o
    /// numero da de origem e MAIOR que o da de destino. Nenhuma passada a mais,
    /// nenhuma estrutura a mais: a ordem ja vem junto da resposta, e quase nenhum
    /// texto diz isso.
    /// </summary>
    public static bool EmOrdemInversa(Digrafo grafo, int[] de)
    {
        for (var v = 0; v < grafo.Vertices; v++)
            foreach (var u in grafo.Saindo(v))
                if (de[v] < de[u]) return false;

        return true;
    }

    /// <summary>E as do KOSARAJU saem na ordem topologica direta, pelo mesmo preco.</summary>
    public static bool EmOrdemDireta(Digrafo grafo, int[] de)
    {
        for (var v = 0; v < grafo.Vertices; v++)
            foreach (var u in grafo.Saindo(v))
                if (de[v] > de[u]) return false;

        return true;
    }

    /// <summary>
    /// Uma particao e valida quando todo par do mesmo grupo se alcanca nos dois
    /// sentidos E todo par de grupos diferentes nao.
    ///
    /// As duas metades importam. So a primeira aceitaria a particao que poe cada
    /// vertice sozinho; so a segunda aceitaria a que poe todo mundo junto.
    /// </summary>
    public static bool EstaCerta(Digrafo grafo, int[] de)
    {
        var alcanca = Alcanca(grafo);

        for (var v = 0; v < grafo.Vertices; v++)
            for (var u = 0; u < grafo.Vertices; u++)
            {
                var juntos = alcanca[v][u] && alcanca[u][v];

                if (juntos != (de[v] == de[u])) return false;
            }

        return true;
    }
}
