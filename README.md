# componentes

Componentes fortemente conexas do zero em C# e .NET 8: **Tarjan**, de 1972, e
**Kosaraju**. O juiz é a **definição**, escrita do jeito mais burro possível:
dois vértices estão na mesma componente quando existe caminho de um até o outro
**nos dois sentidos**, e descobrir isso é fazer uma busca saindo de cada vértice.

```
$ dotnet medidor.dll definicao

  grafo                 vertices   arestas   pela definicao   Tarjan   Kosaraju   o plausivel   Tarjan bate   Kosaraju bate   o plausivel bate
  caminho de 6                 6         5                6        6          6             6           sim             sim                sim
  ciclo de 6                   6         6                1        1          1             1           sim             sim                sim
  tres ciclos em fila          9        11                3        3          3             3           sim             sim                sim
  o que separa                 4         5                3        3          3             2           sim             sim                NAO
  sorteado 5                  12        13               11       11         11             6           sim             sim                NAO
  sorteado 7                  12        12               11       11         11             3           sim             sim                NAO
```

A terceira coluna de "bate" é a interessante. Ela é a mesma implementação do
Tarjan com **uma conferência de três palavras a menos**.

## As três palavras

Tarjan guarda duas coisas por vértice: o **índice**, que é a ordem em que ele foi
descoberto, e o **baixo**, que é o menor índice alcançável a partir dele sem sair
da componente que está sendo montada. Um vértice cujo baixo ficou igual ao
próprio índice é a raiz de uma componente.

Ao olhar para um vizinho já descoberto, o baixo só pode ser atualizado se esse
vizinho **ainda está na pilha**. Se ele já saiu, a componente dele já fechou, e
chegar nele não significa poder voltar de lá.

```csharp
if (indice[u] < 0) chamada.Push(u);
else if (naPilha[u]) baixo[v] = Math.Min(baixo[v], indice[u]);
//       ^^^^^^^^^^ isto
```

Tirar o `naPilha[u]` dá um algoritmo que parece certo, roda sem erro e responde
quase a mesma coisa. O menor grafo que separa os dois tem **quatro vértices**:

```
  0 -> 1    1 -> 2    2 -> 1    0 -> 3    3 -> 1
```

A busca entra pelo 0, desce até o ciclo do 1 com o 2, **fecha** essa componente,
volta ao 0 e só então desce pelo 3. Quando o 3 olha para o 1, o 1 já está fora da
pilha. A versão plausível usa o índice dele mesmo assim, o 3 deixa de ser raiz da
própria componente e cai junto com o 0: duas componentes onde há três.

## Dois caminhos que não têm nada em comum

Kosaraju faz duas passadas. Na primeira, uma busca em profundidade no grafo
normal, só para anotar a ordem em que cada vértice **termina**. Na segunda, uma
busca no grafo com todas as setas **invertidas**, começando pelo que terminou por
último.

Tarjan nunca olha a lista de arestas invertidas. Kosaraju depende dela por
inteiro. Que os dois cheguem sempre à mesma partição é um fato sobre o problema,
não sobre eles.

E cada um dá de brinde uma coisa que quase nenhum texto menciona:

```
$ dotnet medidor.dll ordem

  grafo                 componentes   condensacao sem ciclo   Tarjan em ordem inversa   Kosaraju em ordem direta
  tres ciclos em fila             3                     sim                       sim                        sim
  o que separa                    3                     sim                       sim                        sim
  sorteado 1                     11                     sim                       sim                        sim
```

A condensação, que é o grafo com cada componente virando um vértice só, **nunca**
tem ciclo. Tarjan entrega as componentes já em ordem topológica inversa dela;
Kosaraju, na ordem direta. Nenhuma passada a mais, nenhuma estrutura a mais: a
ordem vem junto da resposta.

## O custo, e a pilha

```
$ dotnet medidor.dll custo

  grafo                        vertices   arestas   componentes   olhadas (Tarjan)   olhadas (Kosaraju)   maior profundidade
  caminho de 200 mil            200,000   199,999       200,000            199,999              399,998              200,000
  ciclo de 200 mil              200,000   200,000             1            200,000              400,000              200,000
  mil ciclos de 100             100,000   100,999         1,000            100,999              201,998                1,099
  sorteado 5 mil (1 aresta)       5,000     4,999         4,987              4,999                9,998                   32
  sorteado 5 mil (2 arestas)      5,000    10,000         1,782             10,000               20,000                1,271
  sem ciclo 2 mil                 2,000     8,046         2,000              8,046               16,092                   14
```

Kosaraju olha exatamente o dobro, porque passa duas vezes. Essa é a diferença
inteira entre os dois.

A última coluna não é curiosidade: ela é o número de quadros que uma versão
recursiva teria empilhado. As duas buscas aqui andam com pilha **explícita**, e a
integração contínua roda o caminho de duzentos mil vértices nos três sistemas.

## A componente gigante

```
$ dotnet medidor.dll tamanho

  grafo                              vertices   componentes   maior componente   vertices sozinhos
  5 mil vertices, 2,000 arestas         5,000         5,000                  1               5,000
  5 mil vertices, 4,000 arestas         5,000         4,996                  4               4,994
  5 mil vertices, 5,000 arestas         5,000         4,987                  5               4,982
  5 mil vertices, 6,000 arestas         5,000         4,572                425               4,569
  5 mil vertices, 8,000 arestas         5,000         2,956              2,045               2,955
  5 mil vertices, 12,000 arestas        5,000         1,188              3,813               1,187
  5 mil vertices, 20,000 arestas        5,000           208              4,793                 207
```

Com uma aresta por vértice, a maior componente tem **cinco** vértices. Com uma
aresta e meia, ela tem **dois mil**. A transição não é suave, e é por isso que
medir num só ponto de densidade não diz nada sobre o comportamento do algoritmo.

## Como rodar

```
dotnet test testes/Componentes.Testes/Componentes.Testes.csproj
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

Os comandos do medidor são `definicao`, `plausivel`, `ordem`, `custo`, `tamanho`
e `tudo`.

Nada aqui é medido em segundos: a unidade é **quantas arestas foram olhadas**,
porque segundo depende de máquina e de quem mais está rodando, e aresta olhada é
o mesmo número no Linux, no Windows e no macOS.

## O que as medidas me corrigiram

**Eu esperava que a versão plausível errasse raramente, e é o contrário.** Erro
sutil costuma ser raro de aparecer, e foi isso que eu escrevi primeiro no
comentário do código: "ela concorda com a versão certa na esmagadora maioria dos
grafos sorteados, que é o que torna o erro perigoso".

```
$ dotnet medidor.dll plausivel

  vertices   arestas   grafos   discordou   componentes a menos   maior diferenca
         8         8      500         472                 1,291                 6
        12        14      500         499                 2,179                 9
        20        30      500         498                 3,765                17
        40        70      500         500                 6,360                31
        80       150      500         500                10,221                58
```

Quatrocentos e setenta e dois de quinhentos, com **oito** vértices. A partir de
quarenta vértices, todos os quinhentos. O erro é fácil de cometer e fácil de
pegar: a única coisa de que ele precisa para sobreviver é não existir juiz
nenhum do outro lado.

E ele erra sempre **para menos**. Juntar componentes que não se tocam só pode
diminuir a contagem, nunca aumentar, e isso é uma afirmação que vale em todo
grafo e que virou teste.

**O juiz precisou das duas metades.** A primeira versão de "esta partição está
certa" conferia só que todo par do mesmo grupo se alcança nos dois sentidos. Essa
conferência aceita de olhos fechados a partição que põe **cada vértice sozinho**.
A conferência simétrica, sozinha, aceita a que põe **todo mundo junto**. São as
duas juntas que caracterizam a resposta, e nenhuma delas sozinha basta.

**A comparação de partições não podia comparar números.** Tarjan numera as
componentes na ordem em que elas fecham, Kosaraju na ordem em que elas abrem, e a
definição pelo menor membro de cada uma: os três dão nomes diferentes para a
mesma resposta. Comparar os vetores diretamente dava diferença em quase todo
grafo, e a diferença não queria dizer nada.

## Licença

MIT.
