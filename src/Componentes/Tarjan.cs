namespace Conde.Componentes;

/// <summary>
/// O ALGORITMO DE TARJAN, de 1972: uma passada so.
///
/// Ele anda em profundidade e guarda duas coisas por vertice: o INDICE, que e a
/// ordem em que ele foi descoberto, e o BAIXO, que e o menor indice que da para
/// alcancar a partir dele sem sair da componente que esta sendo montada. Um
/// vertice cujo baixo ficou igual ao proprio indice e a RAIZ de uma componente, e
/// tudo o que esta acima dele na pilha sai junto.
///
/// A frase que faz o algoritmo funcionar e tambem a que quase todo mundo escreve
/// errado: ao olhar para um vizinho ja descoberto, o baixo so e atualizado se
/// esse vizinho AINDA ESTA NA PILHA. Se ele ja saiu, a componente dele ja
/// fechou, e chegar nele nao significa poder voltar de la.
///
/// O arquivo tem as duas versoes de proposito, e a tabela do medidor diz em
/// quantos grafos elas discordam.
/// </summary>
public static class Tarjan
{
    /// <param name="De">a componente de cada vertice, numerada na ordem em que fechou</param>
    /// <param name="Quantas">quantas componentes</param>
    /// <param name="Olhadas">quantas arestas foram olhadas</param>
    /// <param name="MaiorProfundidade">a maior profundidade que a busca chegou</param>
    public record Resultado(int[] De, int Quantas, long Olhadas, int MaiorProfundidade);

    public static Resultado Achar(Digrafo grafo) => Rodar(grafo, true);

    /// <summary>
    /// A VERSAO PLAUSIVEL, que e a mesma sem a conferencia da pilha.
    ///
    /// Ela existe aqui porque ela e o erro que a gente comete, nao um erro
    /// inventado para o teste ficar bonito: o enunciado "pegue o menor indice que
    /// da para alcancar" leva direto a ela, e o grafo que a separa da versao certa
    /// tem QUATRO vertices.
    ///
    /// E o que eu esperava era que ela concordasse com a versao certa quase sempre,
    /// porque e assim que erro sutil costuma se comportar. E o contrario: em
    /// quinhentos grafos sorteados de OITO vertices, ela discorda em 472. O erro e
    /// facil de cometer e facil de pegar, desde que exista um juiz para pegar.
    /// </summary>
    public static Resultado AcharSemConferirAPilha(Digrafo grafo) => Rodar(grafo, false);

    /// <summary>
    /// A busca em profundidade com PILHA EXPLICITA.
    ///
    /// Nao e preciosismo. Num caminho de duzentos mil vertices em fila, a recursao
    /// tem duzentos mil quadros, e um repositorio vizinho ja levou "Test host
    /// process crashed: Stack overflow" no macOS da integracao continua com uma
    /// profundidade menor que essa. O teste da profundidade maxima esta aqui para
    /// nao deixar ninguem achar que isso e teoria.
    /// </summary>
    private static Resultado Rodar(Digrafo grafo, bool conferirAPilha)
    {
        grafo.ZerarOlhadas();

        var n = grafo.Vertices;
        var indice = new int[n];
        var baixo = new int[n];
        var naPilha = new bool[n];
        var proxima = new int[n];
        var de = new int[n];

        Array.Fill(indice, -1);
        Array.Fill(de, -1);

        var pilha = new Stack<int>();
        var chamada = new Stack<int>();
        var contador = 0;
        var quantas = 0;
        var maiorProfundidade = 0;

        for (var raiz = 0; raiz < n; raiz++)
        {
            if (indice[raiz] >= 0) continue;

            chamada.Push(raiz);

            while (chamada.Count > 0)
            {
                maiorProfundidade = Math.Max(maiorProfundidade, chamada.Count);

                var v = chamada.Peek();

                if (indice[v] < 0)
                {
                    indice[v] = baixo[v] = contador++;
                    pilha.Push(v);
                    naPilha[v] = true;
                }

                var saindo = grafo.Saindo(v);

                if (proxima[v] < saindo.Count)
                {
                    var u = saindo[proxima[v]++];

                    grafo.Olhou();

                    if (indice[u] < 0) chamada.Push(u);
                    else if (naPilha[u] || !conferirAPilha) baixo[v] = Math.Min(baixo[v], indice[u]);

                    continue;
                }

                chamada.Pop();

                if (baixo[v] == indice[v])
                {
                    while (true)
                    {
                        var u = pilha.Pop();

                        naPilha[u] = false;
                        de[u] = quantas;

                        if (u == v) break;
                    }

                    quantas++;
                }

                if (chamada.Count > 0)
                {
                    var pai = chamada.Peek();

                    baixo[pai] = Math.Min(baixo[pai], baixo[v]);
                }
            }
        }

        return new Resultado(de, quantas, grafo.Olhadas, maiorProfundidade);
    }
}
