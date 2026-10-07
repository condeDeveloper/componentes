using Conde.Componentes;

var pedido = args.Length > 0 ? args[0] : "tudo";
var tudo = pedido == "tudo";

if (tudo || pedido == "definicao") Definicao();
if (tudo || pedido == "plausivel") Plausivel();
if (tudo || pedido == "ordem") Ordens();
if (tudo || pedido == "custo") Custos();
if (tudo || pedido == "tamanho") Tamanhos();

static void Definicao()
{
    Console.WriteLine();
    Console.WriteLine("  OS TRES ALGORITMOS CONTRA A DEFINICAO");
    Console.WriteLine();
    Console.WriteLine("  grafo                 vertices   arestas   pela definicao   Tarjan   Kosaraju   o plausivel   Tarjan bate   Kosaraju bate   o plausivel bate");

    foreach (var l in Medidas.ContraADefinicao(12))
        Console.WriteLine($"  {l.Grafo,-21} {l.Vertices,8} {l.Arestas,9} {l.PelaDefinicao,16} {l.Tarjan,8} {l.Kosaraju,10} {l.Plausivel,13} {Marca(l.TarjanBate),13} {Marca(l.KosarajuBate),15} {Marca(l.PlausivelBate),18}");

    Console.WriteLine();
    Console.WriteLine("  a definicao e uma busca saindo de cada vertice: dois estao juntos quando se");
    Console.WriteLine("  alcancam nos DOIS sentidos, e so isso");
    Console.WriteLine();
}

static void Plausivel()
{
    Console.WriteLine();
    Console.WriteLine("  A VERSAO PLAUSIVEL DO TARJAN, EM 500 GRAFOS SORTEADOS DE CADA TAMANHO");
    Console.WriteLine();
    Console.WriteLine("  vertices   arestas   grafos   discordou   componentes a menos   maior diferenca");

    foreach (var l in Medidas.OPlausivelErra())
        Console.WriteLine($"  {l.Vertices,8} {l.Arestas,9} {l.Grafos,8:N0} {l.Discordou,11:N0} {l.ComponentesAMenos,21:N0} {l.MaiorDiferenca,17:N0}");

    Console.WriteLine();
    Console.WriteLine("  a unica diferenca entre as duas versoes e uma conferencia de tres palavras:");
    Console.WriteLine("  se o vizinho ja descoberto AINDA ESTA NA PILHA");
    Console.WriteLine();
}

static void Ordens()
{
    Console.WriteLine();
    Console.WriteLine("  A ORDEM QUE VEM DE GRACA JUNTO DA RESPOSTA");
    Console.WriteLine();
    Console.WriteLine("  grafo                 componentes   condensacao sem ciclo   Tarjan em ordem inversa   Kosaraju em ordem direta");

    foreach (var l in Medidas.Ordens())
        Console.WriteLine($"  {l.Grafo,-21} {l.Componentes,11} {Marca(l.CondensacaoSemCiclo),23} {Marca(l.TarjanInversa),25} {Marca(l.KosarajuDireta),26}");

    Console.WriteLine();
}

static void Custos()
{
    Console.WriteLine();
    Console.WriteLine("  O CUSTO NOS GRAFOS GRANDES, EM ARESTAS OLHADAS");
    Console.WriteLine();
    Console.WriteLine("  grafo                        vertices   arestas   componentes   olhadas (Tarjan)   olhadas (Kosaraju)   maior profundidade");

    foreach (var l in Medidas.Custos())
        Console.WriteLine($"  {l.Grafo,-28} {l.Vertices,8:N0} {l.Arestas,9:N0} {l.Componentes,13:N0} {l.OlhadasTarjan,18:N0} {l.OlhadasKosaraju,20:N0} {l.MaiorProfundidade,20:N0}");

    Console.WriteLine();
    Console.WriteLine("  a ultima coluna e o numero de quadros que uma versao recursiva teria");
    Console.WriteLine("  empilhado, e e por isso que as duas buscas aqui tem pilha explicita");
    Console.WriteLine();
}

static void Tamanhos()
{
    Console.WriteLine();
    Console.WriteLine("  O TAMANHO DAS COMPONENTES, COM AS ARESTAS CRESCENDO");
    Console.WriteLine();
    Console.WriteLine("  grafo                              vertices   componentes   maior componente   vertices sozinhos");

    foreach (var l in Medidas.Tamanhos())
        Console.WriteLine($"  {l.Grafo,-34} {l.Vertices,8:N0} {l.Componentes,13:N0} {l.MaiorComponente,18:N0} {l.Sozinhos,19:N0}");

    Console.WriteLine();
    Console.WriteLine("  a transicao e brusca: a componente gigante aparece quase de uma vez");
    Console.WriteLine();
}

static string Marca(bool valor) => valor ? "sim" : "NAO";
