namespace LivInParis.Core;

/// <summary>
/// Graphe orienté pondéré générique avec les algorithmes de plus court chemin
/// Dijkstra, Bellman-Ford et Floyd-Warshall.
/// </summary>
public class Graphe<T>
{
    private readonly Dictionary<int, Noeud<T>> _parId = new();

    public List<Noeud<T>> Noeuds { get; } = new();
    public List<Lien<T>> Liens { get; } = new();

    #region Construction

    /// <summary>Ajoute un nœud. Lève une exception si l'identifiant existe déjà.</summary>
    public Noeud<T> AjouterNoeud(int id, T valeur)
    {
        if (_parId.ContainsKey(id))
            throw new ArgumentException($"Un nœud d'identifiant {id} existe déjà.", nameof(id));

        var noeud = new Noeud<T>(id, valeur);
        Noeuds.Add(noeud);
        _parId[id] = noeud;
        return noeud;
    }

    /// <summary>Retourne le nœud d'identifiant <paramref name="id"/>, ou null.</summary>
    public Noeud<T>? TrouverNoeud(int id) => _parId.GetValueOrDefault(id);

    /// <summary>
    /// Ajoute un lien source → destination (et destination → source si bidirectionnel).
    /// Un lien déjà existant n'est pas dupliqué.
    /// </summary>
    public void AjouterLien(Noeud<T> source, Noeud<T> destination, double poids, bool bidirectionnel = false)
    {
        if (poids < 0 && bidirectionnel)
            throw new ArgumentException("Un lien bidirectionnel de poids négatif crée un cycle négatif.", nameof(poids));

        if (!LienExiste(source, destination))
            AjouterArc(source, destination, poids);

        if (bidirectionnel && !LienExiste(destination, source))
            AjouterArc(destination, source, poids);
    }

    public bool LienExiste(Noeud<T> source, Noeud<T> destination) =>
        source.LiensSortants.Any(l => l.Destination == destination);

    private void AjouterArc(Noeud<T> source, Noeud<T> destination, double poids)
    {
        var lien = new Lien<T>(source, destination, poids);
        source.LiensSortants.Add(lien);
        Liens.Add(lien);
    }

    #endregion

    #region Plus courts chemins

    /// <summary>
    /// Dijkstra (file de priorité) depuis <paramref name="source"/>. Poids positifs uniquement.
    /// </summary>
    public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>?> previous) Dijkstra(Noeud<T> source)
    {
        var (distances, previous) = Initialiser(source);
        var visites = new HashSet<Noeud<T>>();
        var file = new PriorityQueue<Noeud<T>, double>();
        file.Enqueue(source, 0);

        while (file.Count > 0)
        {
            var courant = file.Dequeue();
            if (!visites.Add(courant))
                continue;

            foreach (var lien in courant.LiensSortants)
            {
                var alt = distances[courant] + lien.Poids;
                if (alt < distances[lien.Destination])
                {
                    distances[lien.Destination] = alt;
                    previous[lien.Destination] = courant;
                    file.Enqueue(lien.Destination, alt);
                }
            }
        }

        return (distances, previous);
    }

    /// <summary>
    /// Bellman-Ford depuis <paramref name="source"/>. Accepte les poids négatifs et lève
    /// <see cref="InvalidOperationException"/> si un cycle de poids négatif est atteignable.
    /// </summary>
    public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>?> previous) BellmanFord(Noeud<T> source)
    {
        var (distances, previous) = Initialiser(source);

        for (int i = 0; i < Noeuds.Count - 1; i++)
        {
            bool modifie = false;
            foreach (var lien in Liens)
            {
                if (distances[lien.Source] + lien.Poids < distances[lien.Destination])
                {
                    distances[lien.Destination] = distances[lien.Source] + lien.Poids;
                    previous[lien.Destination] = lien.Source;
                    modifie = true;
                }
            }
            if (!modifie) break; // convergence anticipée
        }

        foreach (var lien in Liens)
        {
            if (distances[lien.Source] + lien.Poids < distances[lien.Destination])
                throw new InvalidOperationException("Le graphe contient un cycle de poids négatif.");
        }

        return (distances, previous);
    }

    /// <summary>
    /// Floyd-Warshall : distances entre toutes les paires, indexées comme <see cref="Noeuds"/>.
    /// <paramref name="next"/>[i, j] est le nœud suivant sur le chemin de i vers j.
    /// </summary>
    public double[,] FloydWarshall(out Noeud<T>?[,] next)
    {
        int n = Noeuds.Count;
        var dist = new double[n, n];
        next = new Noeud<T>?[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                dist[i, j] = i == j ? 0 : double.PositiveInfinity;

        var index = new Dictionary<Noeud<T>, int>(n);
        for (int i = 0; i < n; i++)
            index[Noeuds[i]] = i;

        foreach (var lien in Liens)
        {
            int u = index[lien.Source], v = index[lien.Destination];
            if (lien.Poids < dist[u, v])
            {
                dist[u, v] = lien.Poids;
                next[u, v] = lien.Destination;
            }
        }

        for (int k = 0; k < n; k++)
            for (int i = 0; i < n; i++)
            {
                if (double.IsPositiveInfinity(dist[i, k])) continue;
                for (int j = 0; j < n; j++)
                {
                    if (dist[i, k] + dist[k, j] < dist[i, j])
                    {
                        dist[i, j] = dist[i, k] + dist[k, j];
                        next[i, j] = next[i, k];
                    }
                }
            }

        return dist;
    }

    /// <summary>
    /// Plus court chemin (Dijkstra) de <paramref name="source"/> à <paramref name="destination"/>.
    /// Retourne une liste vide si aucun chemin n'existe.
    /// </summary>
    public List<Noeud<T>> CheminCourt(Noeud<T> source, Noeud<T> destination)
    {
        var (_, previous) = Dijkstra(source);
        return Reconstruire(previous, source, destination);
    }

    /// <summary>Reconstruit un chemin à partir du dictionnaire des prédécesseurs.</summary>
    public static List<Noeud<T>> Reconstruire(
        IReadOnlyDictionary<Noeud<T>, Noeud<T>?> previous, Noeud<T> source, Noeud<T> destination)
    {
        var chemin = new List<Noeud<T>>();
        Noeud<T>? courant = destination;
        while (courant != null)
        {
            chemin.Add(courant);
            if (courant == source) break;
            courant = previous.GetValueOrDefault(courant);
        }
        chemin.Reverse();
        return chemin.Count > 0 && chemin[0] == source ? chemin : new List<Noeud<T>>();
    }

    /// <summary>Durée totale d'un chemin (somme des poids des liens empruntés).</summary>
    public static double DureeChemin(IReadOnlyList<Noeud<T>> chemin)
    {
        double total = 0;
        for (int i = 0; i + 1 < chemin.Count; i++)
        {
            var lien = chemin[i].LiensSortants.FirstOrDefault(l => l.Destination == chemin[i + 1])
                ?? throw new ArgumentException("Le chemin emprunte un lien inexistant.", nameof(chemin));
            total += lien.Poids;
        }
        return total;
    }

    private (Dictionary<Noeud<T>, double>, Dictionary<Noeud<T>, Noeud<T>?>) Initialiser(Noeud<T> source)
    {
        var distances = new Dictionary<Noeud<T>, double>(Noeuds.Count);
        var previous = new Dictionary<Noeud<T>, Noeud<T>?>(Noeuds.Count);
        foreach (var noeud in Noeuds)
        {
            distances[noeud] = double.PositiveInfinity;
            previous[noeud] = null;
        }
        distances[source] = 0;
        return (distances, previous);
    }

    #endregion
}
