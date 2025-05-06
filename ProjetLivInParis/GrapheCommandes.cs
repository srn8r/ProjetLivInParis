using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public class GrapheCommandes
    {
        public List<Sommet> Sommets { get; set; } = new();
        public List<Arete> Aretes { get; set; } = new();

        private string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";

        public void CreerDepuisBDD()
        {
            Dictionary<int, Sommet> sommetsDict = new();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // 1️⃣ Charger Clients
                string queryClients = "SELECT ID, Nom FROM Identite WHERE ID IN (SELECT ID FROM Client)";
                using (var cmd = new MySqlCommand(queryClients, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sommet = new Sommet
                        {
                            Id = reader.GetInt32("ID"),
                            Nom = reader.GetString("Nom"),
                            Type = "Client"
                        };
                        Sommets.Add(sommet);
                        sommetsDict[sommet.Id] = sommet;
                    }
                }

                // 2️⃣ Charger Cuisiniers
                string queryCuisiniers = "SELECT ID, Nom FROM Identite WHERE ID IN (SELECT ID FROM Cuisinier)";
                using (var cmd = new MySqlCommand(queryCuisiniers, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sommet = new Sommet
                        {
                            Id = reader.GetInt32("ID"),
                            Nom = reader.GetString("Nom"),
                            Type = "Cuisinier"
                        };
                        Sommets.Add(sommet);
                        sommetsDict[sommet.Id] = sommet;
                    }
                }

                // 3️⃣ Charger les commandes (création des arêtes)
                string queryCommandes = "SELECT ClientID, CuisinierID FROM Commande JOIN Plat ON Commande.CommandeID = Plat.CommandeID";
                using (var cmd = new MySqlCommand(queryCommandes, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int clientId = reader.GetInt32("ClientID");
                        int cuisinierId = reader.GetInt32("CuisinierID");

                        if (sommetsDict.ContainsKey(clientId) && sommetsDict.ContainsKey(cuisinierId))
                        {
                            var arete = new Arete
                            {
                                Source = sommetsDict[clientId],
                                Cible = sommetsDict[cuisinierId]
                            };
                            Aretes.Add(arete);
                        }
                    }
                }
            }
        }

        public void AppliquerColorationWelshPowell()
        {
            // Calculer le degré de chaque sommet
            var degres = new Dictionary<Sommet, int>();
            foreach (var sommet in Sommets)
            {
                degres[sommet] = Aretes.Count(a => a.Source == sommet || a.Cible == sommet);
            }

            // Trier les sommets par degré décroissant
            var sommetsTries = Sommets.OrderByDescending(s => degres[s]).ToList();

            int couleurActuelle = 1;
            while (sommetsTries.Any(s => s.Couleur == -1))
            {
                foreach (var sommet in sommetsTries.Where(s => s.Couleur == -1))
                {
                    // Vérifier les couleurs des voisins
                    var voisins = ObtenirVoisins(sommet);
                    bool conflit = voisins.Any(v => v.Couleur == couleurActuelle);

                    if (!conflit)
                    {
                        sommet.Couleur = couleurActuelle;
                    }
                }
                couleurActuelle++;
            }
        }

        private List<Sommet> ObtenirVoisins(Sommet sommet)
        {
            var voisins = new List<Sommet>();
            foreach (var arete in Aretes)
            {
                if (arete.Source == sommet)
                    voisins.Add(arete.Cible);
                else if (arete.Cible == sommet)
                    voisins.Add(arete.Source);
            }
            return voisins;
        }
    }

    public class Sommet
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Type { get; set; } // "Client" ou "Cuisinier"
        public int Couleur { get; set; } = -1; // Non colorié par défaut
    }

    public class Arete
    {
        public Sommet Source { get; set; }
        public Sommet Cible { get; set; }
    }
}