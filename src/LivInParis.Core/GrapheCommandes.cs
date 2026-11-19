namespace LivInParis.Core;

/// <summary>Sommet du graphe des commandes : un client, un cuisinier ou les deux.</summary>
public class Sommet
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    /// <summary>"Client", "Cuisinier" ou "Client-Cuisinier".</summary>
    public string Type { get; set; } = "";
    /// <summary>Couleur attribuée par Welsh-Powell (1, 2, ...), -1 tant que non coloré.</summary>
    public int Couleur { get; set; } = -1;
}

/// <summary>Arête non orientée : au moins une commande entre deux personnes.</summary>
public class Arete
{
    public required Sommet Source { get; init; }
    public required Sommet Cible { get; init; }
}

/// <summary>
/// Graphe non orienté clients / cuisiniers, coloré par l'algorithme de Welsh-Powell.
/// </summary>
public class GrapheCommandes
{
    private readonly Dictionary<int, Sommet> _parId = new();

    public List<Sommet> Sommets { get; } = new();
    public List<Arete> Aretes { get; } = new();

    public Sommet AjouterSommet(int id, string nom, bool estClient, bool estCuisinier)
    {
        var sommet = new Sommet
        {
            Id = id,
            Nom = nom,
            Type = (estClient, estCuisinier) switch
            {
                (true, true) => "Client-Cuisinier",
                (true, false) => "Client",
                _ => "Cuisinier",
            },
        };
        Sommets.Add(sommet);
        _parId[id] = sommet;
        return sommet;
    }

    /// <summary>
    /// Relie un client et un cuisinier. Ignore les boucles, les doublons et les identifiants inconnus.
    /// </summary>
    public bool AjouterCommande(int clientId, int cuisinierId)
    {
        if (clientId == cuisinierId) return false;
        if (!_parId.TryGetValue(clientId, out var client) || !_parId.TryGetValue(cuisinierId, out var cuisinier))
            return false;
        if (Aretes.Any(a => (a.Source == client && a.Cible == cuisinier) || (a.Source == cuisinier && a.Cible == client)))
            return false;

        Aretes.Add(new Arete { Source = client, Cible = cuisinier });
        return true;
    }

    public List<Sommet> ObtenirVoisins(Sommet sommet) =>
        Aretes.Where(a => a.Source == sommet || a.Cible == sommet)
              .Select(a => a.Source == sommet ? a.Cible : a.Source)
              .ToList();

    /// <summary>
    /// Welsh-Powell : trie les sommets par degré décroissant, puis attribue à chaque couleur
    /// successive le plus de sommets possible sans conflit avec leurs voisins.
    /// Retourne le nombre de couleurs utilisées.
    /// </summary>
    public int AppliquerColorationWelshPowell()
    {
        foreach (var s in Sommets) s.Couleur = -1;

        var voisins = Sommets.ToDictionary(s => s, ObtenirVoisins);
        var tries = Sommets.OrderByDescending(s => voisins[s].Count).ToList();

        int couleur = 0;
        while (tries.Any(s => s.Couleur == -1))
        {
            couleur++;
            foreach (var sommet in tries.Where(s => s.Couleur == -1).ToList())
            {
                if (voisins[sommet].All(v => v.Couleur != couleur))
                    sommet.Couleur = couleur;
            }
        }
        return couleur;
    }

    /// <summary>Vrai si aucune arête ne relie deux sommets de même couleur.</summary>
    public bool ColorationValide() => Aretes.All(a => a.Source.Couleur != a.Cible.Couleur);

    /// <summary>
    /// Un graphe 2-coloré est biparti : les clients d'un côté, les cuisiniers de l'autre.
    /// </summary>
    public bool EstBiparti() => Sommets.Count == 0 || (ColorationValide() && Sommets.Max(s => s.Couleur) <= 2);
}
