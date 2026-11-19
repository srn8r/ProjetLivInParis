using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    /// <summary>
    /// Charge le réseau du métro depuis la table <c>metroParis</c> et construit le graphe
    /// (la logique de construction est dans <see cref="ReseauMetro"/>).
    /// </summary>
    public class StationManager
    {
        private readonly string connectionString;
        private readonly List<Station> stations = new();
        private Graphe<Station> graphe = new();

        public StationManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Lit les stations en base et construit le graphe des lignes (sans correspondances).
        /// </summary>
        public void ChargerStationsDepuisBDD()
        {
            stations.Clear();
            try
            {
                using var connexion = new MySqlConnection(connectionString);
                connexion.Open();
                using var command = new MySqlCommand("SELECT * FROM metroParis", connexion);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int stationId = reader.GetInt32("StationId");
                    if (stationId == 0 || reader.IsDBNull(reader.GetOrdinal("Latitude")) || reader.IsDBNull(reader.GetOrdinal("Longitude")))
                        continue;

                    stations.Add(new Station(
                        stationId,
                        reader.GetString("Station"),
                        Texte(reader, "Precedent"),
                        Texte(reader, "Suivant"),
                        Texte(reader, "TempsEntre2Stations")?.Trim() ?? "0",
                        Texte(reader, "TempsChangement")?.Trim() ?? "0",
                        reader.GetDouble("Longitude"),
                        reader.GetDouble("Latitude"),
                        reader.GetString("Ligne")));
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erreur de connexion : " + e.Message);
            }

            graphe = ReseauMetro.Construire(stations, avecCorrespondances: false);
        }

        /// <summary>
        /// Ajoute les liens de correspondance entre les stations de même nom.
        /// </summary>
        public void LierCorrespondances()
        {
            graphe = ReseauMetro.Construire(stations, avecCorrespondances: true);
        }

        public Graphe<Station> ObtenirGraphe() => graphe;

        private static string? Texte(MySqlDataReader reader, string colonne)
        {
            int i = reader.GetOrdinal(colonne);
            return reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
        }
    }
}
