namespace LivInParis.Core;

/// <summary>
/// Station de métro, telle que stockée dans la table <c>metroParis</c>.
/// Une station desservie par plusieurs lignes apparaît une fois par ligne.
/// </summary>
public class Station
{
    public Station(int identifiant, string nom, string? precedent, string? suivant,
        string tempsEntre, string tempsChangement, double longitude, double latitude, string ligne)
    {
        Identifiant = identifiant;
        Nom = nom;
        Precedent = precedent;
        Suivant = suivant;
        TempsEntre2Stations = tempsEntre;
        TempsDeChangement = tempsChangement;
        Longitude = longitude;
        Latitude = latitude;
        Ligne = ligne;
    }

    public int Identifiant { get; set; }
    public string Nom { get; set; }
    /// <summary>Identifiant de la station précédente sur la ligne.</summary>
    public string? Precedent { get; set; }
    /// <summary>Identifiant de la station suivante sur la ligne.</summary>
    public string? Suivant { get; set; }
    /// <summary>Temps (min) depuis la station précédente.</summary>
    public string TempsEntre2Stations { get; set; }
    /// <summary>Temps (min) de correspondance vers les autres lignes de la même station.</summary>
    public string TempsDeChangement { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string Ligne { get; set; }

    public override string ToString() => $"{Nom} (ligne {Ligne})";
}
