using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public class GrapheCommandes
    {
        #region champs et propriétés
        public List<Sommet> Sommets { get; set; } = new();
        public List<Arete> Aretes { get; set; } = new();

        private string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
        #endregion

        #region accès données
        public void CreerDepuisBDD()
        {
            Dictionary<int, Sommet> sommetsDict = new();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string queryIdentites = @"
                    SELECT i.ID, i.Nom, 
                           CASE WHEN cl.ID IS NOT NULL THEN 1 ELSE 0 END AS EstClient,
                           CASE WHEN cu.ID IS NOT NULL THEN 1 ELSE 0 END AS EstCuisinier
                    FROM Identite i
                    LEFT JOIN Client cl ON i.ID = cl.ID
                    LEFT JOIN Cuisinier cu ON i.ID = cu.ID
                    WHERE cl.ID IS NOT NULL OR cu.ID IS NOT NULL";

                using (var cmd = new MySqlCommand(queryIdentites, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("ID");
                        string nom = reader.GetString("Nom");
                        bool estClient = reader.GetBoolean("EstClient");
                        bool estCuisinier = reader.GetBoolean("EstCuisinier");

                        string type;
                        if (estClient && estCuisinier)
                            type = "Client-Cuisinier";
                        else if (estClient)
                            type = "Client";
                        else
                            type = "Cuisinier";

                        var sommet = new Sommet
                        {
                            Id = id,
                            Nom = nom,
                            Type = type
                        };

                        Sommets.Add(sommet);
                        sommetsDict[id] = sommet;
                    }
                }

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
                            bool areteExiste = Aretes.Any(a =>
                                (a.Source.Id == clientId && a.Cible.Id == cuisinierId) ||
                                (a.Source.Id == cuisinierId && a.Cible.Id == clientId));

                            if (!areteExiste && clientId != cuisinierId) 
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
        }
        #endregion

        #region algo de graphe
        public void AppliquerColorationWelshPowell()
        {
            var degres = new Dictionary<Sommet, int>();
            foreach (var sommet in Sommets)
            {
                degres[sommet] = Aretes.Count(a => a.Source == sommet || a.Cible == sommet);
            }

            var sommetsTries = Sommets.OrderByDescending(s => degres[s]).ToList();

            int couleurActuelle = 1;
            while (sommetsTries.Any(s => s.Couleur == -1))
            {
                foreach (var sommet in sommetsTries.Where(s => s.Couleur == -1))
                {
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
        #endregion
    }

    #region classes  
    public class Sommet
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Type { get; set; }
        public int Couleur { get; set; } = -1;
    }
    public class Arete
    {
        public Sommet Source { get; set; }
        public Sommet Cible { get; set; }
    }
    #endregion
}