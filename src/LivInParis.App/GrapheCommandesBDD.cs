using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    /// <summary>
    /// Construit le <see cref="GrapheCommandes"/> (clients / cuisiniers) à partir de la base.
    /// </summary>
    public static class GrapheCommandesBDD
    {
        public static GrapheCommandes Charger(string connectionString)
        {
            var graphe = new GrapheCommandes();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            const string queryIdentites = @"
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
                    graphe.AjouterSommet(
                        reader.GetInt32("ID"),
                        reader.GetString("Nom"),
                        reader.GetBoolean("EstClient"),
                        reader.GetBoolean("EstCuisinier"));
                }
            }

            const string queryCommandes =
                "SELECT ClientID, CuisinierID FROM Commande JOIN Plat ON Commande.CommandeID = Plat.CommandeID";

            using (var cmd = new MySqlCommand(queryCommandes, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    graphe.AjouterCommande(reader.GetInt32("ClientID"), reader.GetInt32("CuisinierID"));
            }

            return graphe;
        }
    }
}
