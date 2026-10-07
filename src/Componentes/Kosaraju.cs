namespace Conde.Componentes;

/// <summary>
/// O ALGORITMO DE KOSARAJU, que resolve o mesmo problema por um caminho que nao
/// tem nada a ver com o do Tarjan.
///
/// Duas passadas. Na primeira, uma busca em profundidade no grafo normal, so para
/// anotar a ordem em que cada vertice TERMINA. Na segunda, uma busca no grafo com
/// todas as setas INVERTIDAS, comecando pelo que terminou por ultimo: tudo o que
/// ela alcanca de uma vez e uma componente.
///
/// A razao e bonita e cabe em duas frases. Ir de u ate v no grafo invertido e o
/// mesmo que ir de v ate u no normal, entao alcancar nos dois sentidos e
/// exatamente a definicao. E a ordem de termino garante que a busca invertida
/// comece sempre numa componente SEM ENTRADA no que sobrou, o que a impede de
/// vazar para a componente de cima.
///
/// Tarjan nunca olha a lista de arestas invertidas. Kosaraju depende dela por
/// inteiro. Que os dois cheguem sempre a mesma particao e um fato sobre o
/// problema, e e isso que faz um servir de conferencia para o outro.
/// </summary>
public static class Kosaraju
{
    /// <param name="De">a componente de cada vertice</param>
    /// <param name="Quantas">quantas componentes</param>
    /// <param name="Olhadas">quantas arestas foram olhadas, somando as duas passadas</param>
    public record Resultado(int[] De, int Quantas, long Olhadas);

    public static Resultado Achar(Digrafo grafo)
    {
        grafo.ZerarOlhadas();

        var ordem = PorTermino(grafo);
        var invertido = grafo.Transposto();
        var de = new int[grafo.Vertices];
        var quantas = 0;

        Array.Fill(de, -1);

        var pilha = new Stack<int>();

        for (var i = ordem.Count - 1; i >= 0; i--)
        {
            var raiz = ordem[i];

            if (de[raiz] >= 0) continue;

            de[raiz] = quantas;
            pilha.Push(raiz);

            while (pilha.Count > 0)
                foreach (var u in invertido.Saindo(pilha.Pop()))
                {
                    grafo.Olhou();

                    if (de[u] >= 0) continue;

                    de[u] = quantas;
                    pilha.Push(u);
                }

            quantas++;
        }

        return new Resultado(de, quantas, grafo.Olhadas);
    }

    /// <summary>
    /// A ordem em que cada vertice TERMINA, por busca em profundidade com pilha
    /// explicita.
    ///
    /// A pilha e explicita pelo mesmo motivo do outro arquivo: num caminho de
    /// duzentos mil vertices em fila, a recursao tem duzentos mil quadros.
    /// </summary>
    public static List<int> PorTermino(Digrafo grafo)
    {
        var visto = new bool[grafo.Vertices];
        var proxima = new int[grafo.Vertices];
        var ordem = new List<int>(grafo.Vertices);
        var pilha = new Stack<int>();

        for (var raiz = 0; raiz < grafo.Vertices; raiz++)
        {
            if (visto[raiz]) continue;

            visto[raiz] = true;
            pilha.Push(raiz);

            while (pilha.Count > 0)
            {
                var v = pilha.Peek();
                var saindo = grafo.Saindo(v);

                if (proxima[v] < saindo.Count)
                {
                    var u = saindo[proxima[v]++];

                    grafo.Olhou();

                    if (visto[u]) continue;

                    visto[u] = true;
                    pilha.Push(u);

                    continue;
                }

                pilha.Pop();
                ordem.Add(v);
            }
        }

        return ordem;
    }
}
