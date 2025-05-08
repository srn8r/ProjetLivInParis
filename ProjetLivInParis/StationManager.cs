using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Project_LivInParis
{
    public class StationManager
    {
        #region champs
        private MySqlConnection maConnexion;
        private Graphe<Station> graphe;
        private Dictionary<int, Noeud<Station>> stations;
        private Dictionary<string, List<Noeud<Station>>> correspondances;
        #endregion

        #region constructeur
        public StationManager(string connectionString)
        {
            graphe = new Graphe<Station>();
            stations = new Dictionary<int, Noeud<Station>>();
            correspondances = new Dictionary<string, List<Noeud<Station>>>();
            maConnexion = new MySqlConnection(connectionString);
        }
        #endregion

        #region méthodes chargement de données
        /// <summary>
        /// Charge les stations depuis la base de données et les ajoute au graphe.
        /// </summary>
        public void ChargerStationsDepuisBDD()
        {
            try
            {
                maConnexion.Open();
                string query = "SELECT * FROM metroParis";
                MySqlCommand command = new MySqlCommand(query, maConnexion);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int stationId = reader.GetInt32("StationId");

                    if (stationId == 0 || reader.IsDBNull(reader.GetOrdinal("Latitude")) || reader.IsDBNull(reader.GetOrdinal("Longitude")))
                        continue;

                    string nom = reader.GetString("Station");
                    string precedent = reader.IsDBNull(reader.GetOrdinal("Precedent")) ? null : reader.GetString("Precedent");
                    string suivant = reader.IsDBNull(reader.GetOrdinal("Suivant")) ? null : reader.GetString("Suivant");

                    string tempsEntreStr = reader.IsDBNull(reader.GetOrdinal("TempsEntre2Stations")) ? "0" : reader.GetString("TempsEntre2Stations").Trim();
                    string tempsChangementStr = reader.IsDBNull(reader.GetOrdinal("TempsChangement")) ? "0" : reader.GetString("TempsChangement").Trim();

                    double longitude = reader.GetDouble("Longitude");
                    double latitude = reader.GetDouble("Latitude");

                    string ligne = reader.GetString("Ligne");

                    Station station = new Station(stationId, nom, precedent, suivant, tempsEntreStr, tempsChangementStr, longitude, latitude, ligne);


                    Console.WriteLine($"{stationId}: {nom} - {latitude}, {longitude}");

                    if (!stations.ContainsKey(stationId))
                        stations[stationId] = graphe.AjouterNoeud(stationId, station);

                    // Lien avec station précédente
                    if (!string.IsNullOrEmpty(precedent) && int.TryParse(precedent, out int precedentId) && stations.ContainsKey(precedentId))
                    {
                        double tempsEntre = double.TryParse(tempsEntreStr, out double t1) ? t1 : 0;
                        graphe.AjouterLien(stations[precedentId], stations[stationId], tempsEntre, bidirectionnel: true);
                    }

                    if (!string.IsNullOrEmpty(suivant) && int.TryParse(suivant, out int suivantId) && stations.ContainsKey(suivantId))
                    {
                        double tempsEntre = double.TryParse(tempsEntreStr, out double t2) ? t2 : 0;
                        graphe.AjouterLien(stations[stationId], stations[suivantId], tempsEntre, bidirectionnel: true);
                    }

                    if (!string.IsNullOrEmpty(tempsChangementStr) && tempsChangementStr != "0")
                    {
                        if (!correspondances.ContainsKey(nom))
                            correspondances[nom] = new List<Noeud<Station>>();

                        correspondances[nom].Add(stations[stationId]);
                    }
                }

                reader.Close();
                command.Dispose();
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erreur de connexion : " + e.Message);
            }
            finally
            {
                maConnexion.Close();
            }
        }
        /// <summary>
        /// Lier les stations ayant le même nom pour les correspondances.
        /// </summary>
        public void LierCorrespondances()
        {
            foreach (var correspondance in correspondances)
            {
                var stationsMemeNom = correspondance.Value;
                for (int i = 0; i < stationsMemeNom.Count; i++)
                {
                    for (int j = i + 1; j < stationsMemeNom.Count; j++)
                    {
                        var station1 = stationsMemeNom[i];
                        var station2 = stationsMemeNom[j];
                        if (double.TryParse(station1.Valeur.TempsDeChangement, out double tempsChangement) && tempsChangement > 0)
                        {
                            graphe.AjouterLien(station1, station2, tempsChangement, bidirectionnel: true);
                        }
                    }
                }
            }
        }
        #endregion

        #region méthodes d'accès et d'affichage
        public Graphe<Station> ObtenirGraphe()
        {
            return graphe;
        }
        /// <summary>
        /// Affiche le graphe des stations et leurs connexions.
        /// </summary>
        public void AfficherGraphe()
        {
            foreach (var noeud in graphe.Noeuds)
            {
                Console.Write($"Station {noeud.Valeur.Nom} ({noeud.Id}) -> ");
                foreach (var lien in noeud.LiensSortants)
                {
                    Console.Write($"{lien.Destination.Valeur.Nom} ({lien.Destination.Id}) [{lien.Poids} min] | ");
                }
                Console.WriteLine();
            }
        }
        #endregion
    }
}
