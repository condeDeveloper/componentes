namespace Conde.Componentes;

/// <summary>
/// As TABELAS que o medidor imprime e que os testes conferem.
///
/// Elas ficam aqui, e nao dentro do programa do medidor, por um motivo so: numero
/// em README que ninguem confere envelhece errado em silencio. Alguem troca uma
/// constante, a tabela muda, e o texto continua afirmando a frase antiga. Aqui o
/// teste afirma sobre o MESMO objeto que vira linha impressa.
/// </summary>
public static class Medidas
{
    public record LinhaDefinicao(
        string Grafo, int Vertices, int Arestas, int PelaDefinicao,
        int Tarjan, int Kosaraju, int Plausivel,
        bool TarjanBate, bool KosarajuBate, bool PlausivelBate);

    /// <summary>
    /// Os tres algoritmos contra a DEFINICAO, nos grafos em que ela cabe.
    /// </summary>
    public static List<LinhaDefinicao> ContraADefinicao(int quantosSorteados = 40)
    {
        var linhas = new List<LinhaDefinicao>();

        foreach (var (nome, grafo) in Digrafos.Pequenos(quantosSorteados))
        {
            var definicao = Juiz.PorDefinicao(grafo);
            var tarjan = Tarjan.Achar(grafo);
            var kosaraju = Kosaraju.Achar(grafo);
            var plausivel = Tarjan.AcharSemConferirAPilha(grafo);

            linhas.Add(new LinhaDefinicao(
                nome, grafo.Vertices, grafo.Arestas, Juiz.Quantas(definicao),
                tarjan.Quantas, kosaraju.Quantas, plausivel.Quantas,
                Juiz.MesmaParticao(definicao, tarjan.De),
                Juiz.MesmaParticao(definicao, kosaraju.De),
                Juiz.MesmaParticao(definicao, plausivel.De)));
        }

        return linhas;
    }

    public record LinhaPlausivel(
        int Vertices, int Arestas, int Grafos, int Discordou, int ComponentesAMenos, int MaiorDiferenca);

    /// <summary>
    /// A VERSAO PLAUSIVEL contra a certa, em muitos grafos sorteados.
    ///
    /// E a tabela que explica por que esse erro sobrevive: na faixa de poucas
    /// arestas ele quase nunca aparece, e e nessa faixa que estao quase todos os
    /// testes que a gente escreve.
    /// </summary>
    public static List<LinhaPlausivel> OPlausivelErra(int grafos = 500)
    {
        var linhas = new List<LinhaPlausivel>();

        foreach (var (vertices, arestas) in new[] { (8, 8), (12, 14), (20, 30), (40, 70), (80, 150) })
        {
            var discordou = 0;
            var aMenos = 0;
            var maior = 0;

            for (var k = 1UL; k <= (ulong)grafos; k++)
            {
                var grafo = Digrafos.ComTantasArestas(vertices, arestas, k);
                var certo = Tarjan.Achar(grafo).Quantas;
                var plausivel = Tarjan.AcharSemConferirAPilha(grafo).Quantas;

                if (certo == plausivel) continue;

                discordou++;
                aMenos += certo - plausivel;
                maior = Math.Max(maior, certo - plausivel);
            }

            linhas.Add(new LinhaPlausivel(vertices, arestas, grafos, discordou, aMenos, maior));
        }

        return linhas;
    }

    public record LinhaOrdem(
        string Grafo, int Componentes, bool CondensacaoSemCiclo, bool TarjanInversa, bool KosarajuDireta);

    /// <summary>
    /// A ORDEM que vem de graca junto da resposta.
    ///
    /// A condensacao nunca tem ciclo, Tarjan entrega as componentes em ordem
    /// topologica inversa dela, e Kosaraju na ordem direta. Nenhuma passada a mais,
    /// nenhuma estrutura a mais.
    /// </summary>
    public static List<LinhaOrdem> Ordens()
    {
        var linhas = new List<LinhaOrdem>();

        foreach (var (nome, grafo) in Digrafos.Pequenos(20))
        {
            var tarjan = Tarjan.Achar(grafo);
            var kosaraju = Kosaraju.Achar(grafo);

            linhas.Add(new LinhaOrdem(
                nome, tarjan.Quantas,
                Juiz.NaoTemCiclo(Juiz.Condensacao(grafo, tarjan.De)),
                Juiz.EmOrdemInversa(grafo, tarjan.De),
                Juiz.EmOrdemDireta(grafo, kosaraju.De)));
        }

        return linhas;
    }

    public record LinhaCusto(
        string Grafo, int Vertices, int Arestas, int Componentes,
        long OlhadasTarjan, long OlhadasKosaraju, int MaiorProfundidade);

    /// <summary>
    /// O custo nos grafos grandes, em ARESTAS OLHADAS, e a maior profundidade da
    /// busca.
    ///
    /// Kosaraju olha mais porque ele passa duas vezes: uma no grafo normal para
    /// pegar a ordem de termino, outra no invertido. Tarjan passa uma vez so, e e
    /// essa a diferenca inteira entre os dois.
    ///
    /// A ultima coluna nao e curiosidade: ela e o numero de quadros que uma versao
    /// recursiva teria empilhado.
    /// </summary>
    public static List<LinhaCusto> Custos()
    {
        var linhas = new List<LinhaCusto>();

        foreach (var (nome, grafo) in Digrafos.Grandes())
        {
            var tarjan = Tarjan.Achar(grafo);
            var kosaraju = Kosaraju.Achar(grafo);

            linhas.Add(new LinhaCusto(
                nome, grafo.Vertices, grafo.Arestas, tarjan.Quantas,
                tarjan.Olhadas, kosaraju.Olhadas, tarjan.MaiorProfundidade));
        }

        return linhas;
    }

    public record LinhaTamanho(string Grafo, int Vertices, int Componentes, int MaiorComponente, int Sozinhos);

    /// <summary>
    /// O TAMANHO das componentes, que e o que de fato se quer saber de um grafo
    /// dirigido de verdade.
    ///
    /// Em grafo sorteado, a transicao e brusca: com menos de uma aresta por
    /// vertice quase todo mundo fica sozinho, e com pouco mais que isso aparece uma
    /// componente gigante que engole metade do grafo.
    /// </summary>
    public static List<LinhaTamanho> Tamanhos()
    {
        var linhas = new List<LinhaTamanho>();

        foreach (var arestas in new[] { 2_000, 4_000, 5_000, 6_000, 8_000, 12_000, 20_000 })
        {
            var grafo = Digrafos.ComTantasArestas(5_000, arestas, 7);
            var de = Tarjan.Achar(grafo).De;
            var tamanhos = de.GroupBy(c => c).Select(g => g.Count()).ToList();

            linhas.Add(new LinhaTamanho(
                $"5 mil vertices, {arestas:N0} arestas", 5_000,
                tamanhos.Count, tamanhos.Max(), tamanhos.Count(t => t == 1)));
        }

        return linhas;
    }
}
