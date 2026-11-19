using LivInParis.Core;

namespace LivInParis.Core.Tests;

public class GrapheTests
{
    // A --1--> B --2--> C
    //  \               ^
    //   -------5-------
    private static (Graphe<string> g, Noeud<string> a, Noeud<string> b, Noeud<string> c, Noeud<string> d) Exemple()
    {
        var g = new Graphe<string>();
        var a = g.AjouterNoeud(1, "A");
        var b = g.AjouterNoeud(2, "B");
        var c = g.AjouterNoeud(3, "C");
        var d = g.AjouterNoeud(4, "D"); // isolé
        g.AjouterLien(a, b, 1);
        g.AjouterLien(b, c, 2);
        g.AjouterLien(a, c, 5);
        return (g, a, b, c, d);
    }

    [Fact]
    public void AjouterLien_NeDupliquePas()
    {
        var (g, a, b, _, _) = Exemple();
        g.AjouterLien(a, b, 1);
        Assert.Equal(3, g.Liens.Count);
    }

    [Fact]
    public void AjouterLien_Bidirectionnel_CreeDeuxArcs()
    {
        var g = new Graphe<string>();
        var x = g.AjouterNoeud(1, "X");
        var y = g.AjouterNoeud(2, "Y");
        g.AjouterLien(x, y, 3, bidirectionnel: true);
        Assert.True(g.LienExiste(x, y));
        Assert.True(g.LienExiste(y, x));
        Assert.Equal(2, g.Liens.Count);
    }

    [Fact]
    public void AjouterNoeud_IdentifiantEnDouble_Leve()
    {
        var g = new Graphe<string>();
        g.AjouterNoeud(1, "A");
        Assert.Throws<ArgumentException>(() => g.AjouterNoeud(1, "B"));
    }

    [Fact]
    public void Dijkstra_TrouveLeCheminLePlusCourt()
    {
        var (g, a, b, c, _) = Exemple();
        var chemin = g.CheminCourt(a, c);
        Assert.Equal(new[] { a, b, c }, chemin);
        Assert.Equal(3, Graphe<string>.DureeChemin(chemin));
    }

    [Fact]
    public void Dijkstra_SansChemin_RetourneListeVide()
    {
        var (g, a, _, _, d) = Exemple();
        Assert.Empty(g.CheminCourt(a, d));
        Assert.True(double.IsPositiveInfinity(g.Dijkstra(a).distances[d]));
    }

    [Fact]
    public void CheminCourt_SourceEgaleDestination()
    {
        var (g, a, _, _, _) = Exemple();
        Assert.Equal(new[] { a }, g.CheminCourt(a, a));
    }

    [Fact]
    public void BellmanFord_DonneLesMemesDistancesQueDijkstra()
    {
        var (g, a, _, _, _) = Exemple();
        var dijkstra = g.Dijkstra(a).distances;
        var bellman = g.BellmanFord(a).distances;
        foreach (var n in g.Noeuds)
            Assert.Equal(dijkstra[n], bellman[n]);
    }

    [Fact]
    public void BellmanFord_GereLesPoidsNegatifs()
    {
        var g = new Graphe<string>();
        var a = g.AjouterNoeud(1, "A");
        var b = g.AjouterNoeud(2, "B");
        var c = g.AjouterNoeud(3, "C");
        g.AjouterLien(a, b, 4);
        g.AjouterLien(a, c, 2);
        g.AjouterLien(b, c, -3);
        var (distances, previous) = g.BellmanFord(a);
        Assert.Equal(1, distances[c]);
        Assert.Equal(new[] { a, b, c }, Graphe<string>.Reconstruire(previous, a, c));
    }

    [Fact]
    public void BellmanFord_DetecteUnCycleNegatif()
    {
        var g = new Graphe<string>();
        var a = g.AjouterNoeud(1, "A");
        var b = g.AjouterNoeud(2, "B");
        g.AjouterLien(a, b, 1);
        g.AjouterLien(b, a, -2);
        Assert.Throws<InvalidOperationException>(() => g.BellmanFord(a));
    }

    [Fact]
    public void FloydWarshall_DonneLesMemesDistancesQueDijkstra()
    {
        var (g, _, _, _, _) = Exemple();
        var dist = g.FloydWarshall(out var next);
        for (int i = 0; i < g.Noeuds.Count; i++)
        {
            var dijkstra = g.Dijkstra(g.Noeuds[i]).distances;
            for (int j = 0; j < g.Noeuds.Count; j++)
                Assert.Equal(dijkstra[g.Noeuds[j]], dist[i, j]);
        }
        Assert.Same(g.Noeuds[1], next[0, 2]); // A -> C passe d'abord par B
    }
}
