using LivInParis.Core;

namespace LivInParis.Core.Tests;

public class GrapheCommandesTests
{
    [Fact]
    public void AjouterSommet_DeduitLeType()
    {
        var g = new GrapheCommandes();
        Assert.Equal("Client", g.AjouterSommet(1, "A", true, false).Type);
        Assert.Equal("Cuisinier", g.AjouterSommet(2, "B", false, true).Type);
        Assert.Equal("Client-Cuisinier", g.AjouterSommet(3, "C", true, true).Type);
    }

    [Fact]
    public void AjouterCommande_IgnoreDoublonsBouclesEtInconnus()
    {
        var g = new GrapheCommandes();
        g.AjouterSommet(1, "Client", true, false);
        g.AjouterSommet(2, "Cuisinier", false, true);
        Assert.True(g.AjouterCommande(1, 2));
        Assert.False(g.AjouterCommande(1, 2));
        Assert.False(g.AjouterCommande(2, 1));
        Assert.False(g.AjouterCommande(1, 1));
        Assert.False(g.AjouterCommande(1, 99));
        Assert.Single(g.Aretes);
    }

    [Fact]
    public void WelshPowell_GrapheBiparti_DeuxCouleurs()
    {
        var g = new GrapheCommandes();
        for (int i = 1; i <= 3; i++) g.AjouterSommet(i, $"Client{i}", true, false);
        for (int i = 10; i <= 11; i++) g.AjouterSommet(i, $"Cuisinier{i}", false, true);
        g.AjouterCommande(1, 10);
        g.AjouterCommande(2, 10);
        g.AjouterCommande(2, 11);
        g.AjouterCommande(3, 11);

        Assert.Equal(2, g.AppliquerColorationWelshPowell());
        Assert.True(g.ColorationValide());
        Assert.True(g.EstBiparti());
    }

    [Fact]
    public void WelshPowell_Triangle_TroisCouleurs()
    {
        // Possible quand des personnes sont à la fois client et cuisinier.
        var g = new GrapheCommandes();
        for (int i = 1; i <= 3; i++) g.AjouterSommet(i, $"P{i}", true, true);
        g.AjouterCommande(1, 2);
        g.AjouterCommande(2, 3);
        g.AjouterCommande(3, 1);

        Assert.Equal(3, g.AppliquerColorationWelshPowell());
        Assert.True(g.ColorationValide());
        Assert.False(g.EstBiparti());
    }

    [Fact]
    public void WelshPowell_SommetsIsoles_UneCouleur()
    {
        var g = new GrapheCommandes();
        g.AjouterSommet(1, "A", true, false);
        g.AjouterSommet(2, "B", false, true);
        Assert.Equal(1, g.AppliquerColorationWelshPowell());
    }
}
