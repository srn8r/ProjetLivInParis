using LivInParis.Core;

namespace LivInParis.Core.Tests;

public class ReseauMetroTests
{
    private static Station S(int id, string nom, int? prec, int? suiv, string temps, string changement, string ligne,
        double lat = 48.86, double lon = 2.35) =>
        new(id, nom, prec?.ToString(), suiv?.ToString(), temps, changement, lon, lat, ligne);

    // Ligne 1 : 1 - 2 (Châtelet) - 3     Ligne 4 : 10 - 11 (Châtelet) - 12
    private static List<Station> Exemple() => new()
    {
        // Volontairement dans le désordre : la construction ne doit pas dépendre de l'ordre.
        S(3, "Louvre", 2, null, "2", "0", "1"),
        S(11, "Châtelet", 10, 12, "1,5", "4", "4"),
        S(1, "Concorde", null, 2, "0", "0", "1"),
        S(12, "Cité", 11, null, "2", "0", "4"),
        S(2, "Châtelet", 1, 3, "3", "4", "1"),
        S(10, "Les Halles", null, 11, "0", "0", "4"),
    };

    [Fact]
    public void Construire_RelieLesStationsDuneLigneDansLesDeuxSens()
    {
        var g = ReseauMetro.Construire(Exemple(), avecCorrespondances: false);
        var s1 = g.TrouverNoeud(1)!;
        var s2 = g.TrouverNoeud(2)!;
        Assert.True(g.LienExiste(s1, s2));
        Assert.True(g.LienExiste(s2, s1));
        Assert.Equal(3, s1.LiensSortants.Single(l => l.Destination == s2).Poids);
    }

    [Fact]
    public void Construire_SansCorrespondances_LesLignesSontSeparees()
    {
        var g = ReseauMetro.Construire(Exemple(), avecCorrespondances: false);
        Assert.Empty(g.CheminCourt(g.TrouverNoeud(1)!, g.TrouverNoeud(12)!));
    }

    [Fact]
    public void Construire_AvecCorrespondances_PermetDeChangerDeLigne()
    {
        var g = ReseauMetro.Construire(Exemple());
        var chemin = g.CheminCourt(g.TrouverNoeud(1)!, g.TrouverNoeud(12)!);
        Assert.Equal(new[] { 1, 2, 11, 12 }, chemin.Select(n => n.Id));
        // 3 min (Concorde -> Châtelet) + 4 min de correspondance + 2 min (Châtelet -> Cité)
        Assert.Equal(9, Graphe<Station>.DureeChemin(chemin));
    }

    [Theory]
    [InlineData("2", 2)]
    [InlineData("1,5", 1.5)]
    [InlineData("1.5", 1.5)]
    [InlineData(" 3 ", 3)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    [InlineData("n/a", 0)]
    public void Minutes_ParseLesFormatsDeLaBase(string? valeur, double attendu)
    {
        Assert.Equal(attendu, ReseauMetro.Minutes(valeur));
    }

    [Fact]
    public void DistanceKm_ConcordeChatelet_Environ2Km()
    {
        double d = ReseauMetro.DistanceKm(48.8656, 2.3212, 48.8584, 2.3470);
        Assert.InRange(d, 1.8, 2.3);
    }

    [Fact]
    public void PlusProche_RetourneLaStationLaPlusProche()
    {
        var stations = new List<Station>
        {
            S(1, "Nord", null, null, "0", "0", "1", lat: 48.88, lon: 2.35),
            S(2, "Sud", null, null, "0", "0", "1", lat: 48.83, lon: 2.35),
        };
        var g = ReseauMetro.Construire(stations);
        Assert.Equal(2, ReseauMetro.PlusProche(g, 48.835, 2.351)!.Id);
    }
}
