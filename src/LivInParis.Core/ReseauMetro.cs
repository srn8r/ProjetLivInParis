using System.Globalization;

namespace LivInParis.Core;

/// <summary>
/// Construit le graphe du métro parisien à partir de la liste des stations.
/// </summary>
public static class ReseauMetro
{
    /// <summary>
    /// Crée un nœud par station, relie chaque station à sa précédente et à sa suivante sur la ligne,
    /// puis (si demandé) relie entre elles les stations de même nom sur des lignes différentes,
    /// avec le temps de correspondance comme poids.
    /// Toutes les stations sont chargées avant la création des liens, donc l'ordre des lignes
    /// en base n'a pas d'importance.
    /// </summary>
    public static Graphe<Station> Construire(IEnumerable<Station> stations, bool avecCorrespondances = true)
    {
        var graphe = new Graphe<Station>();
        foreach (var station in stations)
        {
            if (graphe.TrouverNoeud(station.Identifiant) is null)
                graphe.AjouterNoeud(station.Identifiant, station);
        }

        // 1. Liens le long des lignes. Le temps d'une station est celui depuis sa précédente.
        foreach (var noeud in graphe.Noeuds)
        {
            var station = noeud.Valeur;
            if (Voisin(graphe, station.Precedent) is { } precedent)
                graphe.AjouterLien(precedent, noeud, Minutes(station.TempsEntre2Stations), bidirectionnel: true);
        }
        // Une station dont la suivante n'a pas de précédente renseignée reste reliée.
        foreach (var noeud in graphe.Noeuds)
        {
            if (Voisin(graphe, noeud.Valeur.Suivant) is { } suivant && !graphe.LienExiste(noeud, suivant))
                graphe.AjouterLien(noeud, suivant, Minutes(suivant.Valeur.TempsEntre2Stations), bidirectionnel: true);
        }

        // 2. Correspondances entre lignes.
        if (avecCorrespondances)
        {
            foreach (var groupe in graphe.Noeuds.GroupBy(n => n.Valeur.Nom.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var memeNom = groupe.Where(n => Minutes(n.Valeur.TempsDeChangement) > 0).ToList();
                for (int i = 0; i < memeNom.Count; i++)
                    for (int j = i + 1; j < memeNom.Count; j++)
                        graphe.AjouterLien(memeNom[i], memeNom[j],
                            Minutes(memeNom[i].Valeur.TempsDeChangement), bidirectionnel: true);
            }
        }

        return graphe;
    }

    /// <summary>Station la plus proche d'un point GPS (distance de Haversine).</summary>
    public static Noeud<Station>? PlusProche(Graphe<Station> graphe, double latitude, double longitude) =>
        graphe.Noeuds.MinBy(n => DistanceKm(latitude, longitude, n.Valeur.Latitude, n.Valeur.Longitude));

    /// <summary>Distance orthodromique en kilomètres entre deux points GPS.</summary>
    public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double rayonTerreKm = 6371.0;
        double dLat = Radians(lat2 - lat1), dLon = Radians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * rayonTerreKm * Math.Asin(Math.Sqrt(a));
    }

    /// <summary>Convertit un temps texte ("2", "1,5", "1.5", vide) en minutes.</summary>
    public static double Minutes(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur)) return 0;
        var normalise = valeur.Trim().Replace(',', '.');
        return double.TryParse(normalise, NumberStyles.Float, CultureInfo.InvariantCulture, out var m) ? m : 0;
    }

    private static Noeud<Station>? Voisin(Graphe<Station> graphe, string? id) =>
        int.TryParse(id, out var valeur) ? graphe.TrouverNoeud(valeur) : null;

    private static double Radians(double degres) => degres * Math.PI / 180.0;
}
