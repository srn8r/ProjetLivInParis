using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_LivInParis
{
    public class Graphe<T>
    {
        #region Champs et propriétés
        public List<Noeud<T>> Noeuds { get; set; }
        public List<Lien<T>> Liens { get; set; }
        #endregion

        #region Constructeur
        public Graphe()
        {
            Noeuds = new List<Noeud<T>>();
            Liens = new List<Lien<T>>();
        }
        #endregion

        #region Ajout de nœud & lien
        public Noeud<T> AjouterNoeud(int id, T valeur)
        {
            var noeud = new Noeud<T>(id, valeur);
            Noeuds.Add(noeud);
            return noeud;
        }

        public void AjouterLien(Noeud<T> source, Noeud<T> destination, double poids, bool bidirectionnel = false)
        {
            if (!LienExiste(source, destination))
            {
                var lien = new Lien<T>(source, destination, poids);
                source.LiensSortants.Add(lien);
                Liens.Add(lien);
            }

            if (bidirectionnel && !LienExiste(destination, source))
            {
                var lienInverse = new Lien<T>(destination, source, poids);
                destination.LiensSortants.Add(lienInverse);
                Liens.Add(lienInverse);
            }
        }

        private bool LienExiste(Noeud<T> source, Noeud<T> destination)
        {
            foreach (var lien in source.LiensSortants)
            {
                if (lien.Destination == destination)
                    return true;
            }
            return false;
        }

        #endregion

        #region Plus court chemin

        /// <summary>
        /// Implémente l'algorithme de Dijkstra pour calculer les distances minimales à partir d'un nœud source.
        /// </summary>
        /// <param name="source">Le nœud de départ.</param>
        /// <returns>Tuple (distances, previous).</returns>
        public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>> previous) Dijkstra(Noeud<T> source)
        {
            var distances = new Dictionary<Noeud<T>, double>();
            var previous = new Dictionary<Noeud<T>, Noeud<T>>();
            var visited = new HashSet<Noeud<T>>();
            var queue = new PriorityQueue<Noeud<T>, double>();

            foreach (var noeud in Noeuds)
            {
                distances[noeud] = double.PositiveInfinity;
                previous[noeud] = null;
            }

            distances[source] = 0;
            queue.Enqueue(source, 0);

            while (queue.Count > 0)
            {
                var courant = queue.Dequeue();
                if (!visited.Add(courant))
                    continue;

                foreach (var lien in courant.LiensSortants)
                {
                    var voisin = lien.Destination;
                    var alt = distances[courant] + lien.Poids;
                    if (alt < distances[voisin])
                    {
                        distances[voisin] = alt;
                        previous[voisin] = courant;
                        queue.Enqueue(voisin, alt);
                    }
                }
            }

            return (distances, previous);
        }

        /// <summary>
        /// Reconstruit le chemin le plus court de la source à la destination en remontant le dictionnaire des prédécesseurs.
        /// </summary>
        /// <param name="source">Le nœud de départ.</param>
        /// <param name="destination">Le nœud d'arrivée.</param>
        /// <returns>Liste de nœuds représentant le chemin ou une liste vide si aucun chemin n'existe.</returns>
        public List<Noeud<T>> CheminCourt(Noeud<T> source, Noeud<T> destination)
        {
            var (distances, previous) = Dijkstra(source);
            var path = new List<Noeud<T>>();
            Noeud<T> current = destination;

            while (current != null)
            {
                path.Insert(0, current);
                current = previous[current];
            }
            if (path.Count == 0 || path.First() != source)
                return new List<Noeud<T>>();

            return path;
        }

        /// <summary>
        /// Affiche le chemin le plus court entre la source et la destination avec :
        /// - l'identifiant et le nom de chaque station,
        /// - le temps de chaque segment,
        /// - ainsi que le temps total du trajet.
        /// </summary>
        /// <param name="source">Le nœud de départ.</param>
        /// <param name="destination">Le nœud d'arrivée.</param>
        public void AfficherCheminDijkstra(Noeud<T> source, Noeud<T> destination)
        {
            // Obtention du chemin le plus court grâce à GetShortestPath.
            List<Noeud<T>> chemin = CheminCourt(source, destination);
            if (chemin == null || chemin.Count == 0)
            {
                Console.WriteLine($"Aucun chemin trouvé entre {source.Valeur} et {destination.Valeur}.");
                return;
            }

            double tempsTotal = 0;
            for (int i = 0; i < chemin.Count; i++)
            {
                var courant = chemin[i];
                Console.Write($"{courant.Id}: {((Station)(object)courant.Valeur).Nom}");

                if (i < chemin.Count - 1)
                {
                    var suivant = chemin[i + 1];
                    var lien = courant.LiensSortants.FirstOrDefault(l => l.Destination == suivant);
                    if (lien != null)
                    {
                        Console.Write($" -> [{lien.Poids} min] -> ");
                        tempsTotal += lien.Poids;
                    }
                    else
                    {
                        Console.Write(" -> ");
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine($"Temps total: {tempsTotal} min");
        }

        /// <summary>
        /// Implémente l'algorithme de Bellman-Ford pour calculer les distances minimales à partir d'un nœud source.
        /// </summary>
        /// <param name="source">Le nœud de départ.</param>
        /// <returns>Tuple (distances, previous).</returns>
        public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>> previous) BellmanFord(Noeud<T> source)
        {
            var distances = new Dictionary<Noeud<T>, double>();
            var previous = new Dictionary<Noeud<T>, Noeud<T>>();

            foreach (var noeud in Noeuds)
            {
                distances[noeud] = double.PositiveInfinity;
                previous[noeud] = null;
            }
            distances[source] = 0;

            int n = Noeuds.Count;
            for (int i = 0; i < n - 1; i++)
            {
                foreach (var lien in Liens)
                {
                    if (distances[lien.Source] + lien.Poids < distances[lien.Destination])
                    {
                        distances[lien.Destination] = distances[lien.Source] + lien.Poids;
                        previous[lien.Destination] = lien.Source;
                    }
                }
            }

            foreach (var lien in Liens)
            {
                if (distances[lien.Source] + lien.Poids < distances[lien.Destination])
                {
                    throw new Exception("Le graphe contient un cycle de poids négatif");
                }
            }

            return (distances, previous);
        }

        /// <summary>
        /// Implémente l'algorithme de Floyd-Warshall pour calculer les plus courts chemins entre toutes les paires de nœuds.
        /// </summary>
        /// <param name="next">
        /// Matrice de taille n x n (où n est le nombre de nœuds) qui permet de retrouver le prochain nœud sur le chemin de i à j.
        /// </param>
        /// <returns>Une matrice de distances minimales entre toutes les paires de nœuds.</returns>
        public double[,] FloydWarshall(out Noeud<T>[,] next)
        {
            int n = Noeuds.Count;
            double[,] dist = new double[n, n];
            next = new Noeud<T>[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = (i == j) ? 0 : double.PositiveInfinity;
                    next[i, j] = null;
                }
            }

            var indexNoeud = new Dictionary<Noeud<T>, int>();
            for (int i = 0; i < n; i++)
            {
                indexNoeud[Noeuds[i]] = i;
            }

            foreach (var lien in Liens)
            {
                int u = indexNoeud[lien.Source];
                int v = indexNoeud[lien.Destination];
                dist[u, v] = lien.Poids;
                next[u, v] = lien.Destination;
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            next[i, j] = next[i, k];
                        }
                    }
                }
            }
            return dist;
        }

        #endregion
    }
}
