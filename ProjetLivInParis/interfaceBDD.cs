using System;
using System.Data;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using ProjetLivInParis;
using Client = ProjetLivInParis.Client;

namespace Project_LivInParis
{
    public class InterfaceBDD
    {
        #region connexion

        private MySqlConnection connexion;

        private readonly string connectionString =
            "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";

        public void Connexion()
        {
            connexion = new MySqlConnection(connectionString);
            connexion.Open();
        }

        public void FermerConnexion()
        {
            connexion?.Close();
        }
        #endregion

        #region récupérer les valeurs BDD
        public List<Utilisateur> RecupererUtilisateurs()
        {
            List<Utilisateur> utilisateurs = new List<Utilisateur>();

            try
            {
                string query = @"
                                    SELECT u.*, 
                                        CASE 
                                            WHEN c.ID IS NOT NULL THEN 'Client' 
                                            WHEN cui.ID IS NOT NULL THEN 'Cuisinier'
                                            ELSE 'Inconnu'
                                        END AS type
                                    FROM identite u
                                    LEFT JOIN client c ON u.ID = c.ID
                                    LEFT JOIN cuisinier cui ON u.ID = cui.ID
                                    ORDER BY u.Nom, u.Prenom";

                MySqlCommand cmd = new MySqlCommand(query, connexion);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Utilisateur utilisateur;
                    string type = reader.GetString("type");

                    if (type == "Client")
                    {
                        utilisateur = new Client();
                    }
                    else if (type == "Cuisinier")
                    {
                        utilisateur = new Cuisinier();
                    }
                    else
                    {
                        utilisateur = new Utilisateur();
                    }

                    utilisateur.ID = reader.GetInt32("id");
                    utilisateur.Nom = reader.GetString("nom");
                    utilisateur.Prenom = reader.GetString("prenom");
                    utilisateur.Adresse = reader.GetString("adresse");
                    utilisateur.Telephone = reader.GetString("telephone");
                    utilisateur.Email = reader.GetString("email");
                    utilisateur.MotDePasse = reader.GetString("motdepasse");

                    utilisateurs.Add(utilisateur);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la récupération des utilisateurs: " + ex.Message);
            }

            return utilisateurs;
        }
        /// <summary>
        /// Récupère tous les clients de la base de données.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<Client> RecupererClients()
        {
            List<Client> clients = new List<Client>();

            try
            {
                string query = @"
                                    SELECT u.*, c.TypeClient, c.NomEntreprise, c.Referent 
                                    FROM identite u
                                    INNER JOIN client c ON u.ID = c.ID
                                    ORDER BY u.Nom, u.Prenom";

                MySqlCommand cmd = new MySqlCommand(query, connexion);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Client client = new Client
                    {
                        ID = reader.GetInt32("ID"),
                        Nom = reader.GetString("Nom"),
                        Prenom = reader.GetString("Prenom"),
                        Adresse = reader.GetString("Adresse"),
                        Telephone = reader.GetString("Telephone"),
                        Email = reader.GetString("Email"),
                        TypeClient = reader.GetString("TypeClient"),
                        MotDePasse = reader.GetString("motdepasse"),
                        NomEntreprise = reader.IsDBNull(reader.GetOrdinal("NomEntreprise")) ? string.Empty : reader.GetString("NomEntreprise"),
                        Referent = reader.IsDBNull(reader.GetOrdinal("Referent")) ? string.Empty : reader.GetString("Referent")
                    };

                    clients.Add(client);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la récupération des clients: " + ex.Message);
            }

            return clients;
        }
        public List<Cuisinier> RecupererCuisiniers()
        {
            List<Cuisinier> cuisiniers = new List<Cuisinier>();

            try
            {
                string query = @"
                                    SELECT u.* 
                                    FROM identite u
                                    INNER JOIN cuisinier c ON u.ID = c.ID
                                    ORDER BY u.Nom, u.Prenom";

                MySqlCommand cmd = new MySqlCommand(query, connexion);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Cuisinier cuisinier = new Cuisinier
                    {
                        ID = reader.GetInt32("id"),
                        Nom = reader.GetString("nom"),
                        Prenom = reader.GetString("prenom"),
                        Adresse = reader.GetString("adresse"),
                        Telephone = reader.GetString("telephone"),
                        Email = reader.GetString("email"),
                        MotDePasse = reader.GetString("motdepasse")
                    };

                    cuisiniers.Add(cuisinier);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la récupération des cuisiniers: " + ex.Message);
            }

            return cuisiniers;
        }
        /// <summary>
        /// Récupère toutes les commandes de la base de données.
        /// </summary>
        /// <returns></returns>
        public List<Commande> RecupererToutesLesCommandes()
        {
            List<Commande> commandes = new List<Commande>();

            string query = @"
        SELECT c.CommandeID, c.DateCommande, c.AdresseLivraison, c.PrixTotal, c.Statut, 
               c.ModePaiement, i.ID AS ClientID, i.Nom AS ClientNom, i.Prenom AS ClientPrenom, p.Nom AS NomPlat
        FROM Commande c
        JOIN Identite i ON c.ClientID = i.ID
        JOIN Plat p ON p.CommandeID = c.CommandeID";

            using (MySqlCommand cmd = new MySqlCommand(query, connexion))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Commande commande = new Commande
                    {
                        CommandeID = reader.GetInt32("CommandeID"),
                        DateCommande = reader.GetDateTime("DateCommande"),
                        AdresseLivraison = reader.GetString("AdresseLivraison"),
                        PrixTotal = reader.GetDecimal("PrixTotal"),
                        Statut = reader.GetString("Statut"),
                        ModePaiement = reader.GetString("ModePaiement"),
                        ClientID = reader.GetInt32("ClientID"),
                        ClientNom = reader.GetString("ClientNom"),
                        ClientPrenom = reader.GetString("ClientPrenom"),
                        NomPlat = reader.GetString("NomPlat")
                    };

                    commandes.Add(commande);
                }
            }

            return commandes;
        }
        #endregion

        #region ajouter des utilisateurs BDD depuis visual studio

        /// <summary>
        /// Insère un utilisateur dans la table utilisateurs et retourne l'ID inséré.
        /// </summary>
        public int InsererUtilisateur(string nom, string prenom, string adresse, string telephone, string email, string motDePasse)
        {
            string sql = @"INSERT INTO identite (nom, prenom, adresse, telephone, email, motDePasse)
                   VALUES (@Nom, @Prenom, @Adresse, @Telephone, @Email, @MotDePasse);
                   SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, connexion);
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@Prenom", prenom);
            cmd.Parameters.AddWithValue("@Adresse", adresse);
            cmd.Parameters.AddWithValue("@Telephone", telephone);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@MotDePasse", motDePasse);

            int insertedId = Convert.ToInt32(cmd.ExecuteScalar());
            return insertedId;
        }

        /// <summary>
        /// Insère un client lié à l'utilisateur (via ID).
        /// </summary>
        public void InsererClient(int id, string typeClient, string nomEntreprise, string referent)
        {
            string sql = @"INSERT INTO client (ID, TypeClient, NomEntreprise, Referent)
              VALUES (@ID, @TypeClient, @NomEntreprise, @Referent);";

            using var cmd = new MySqlCommand(sql, connexion);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@TypeClient", typeClient);
            cmd.Parameters.AddWithValue("@NomEntreprise", string.IsNullOrWhiteSpace(nomEntreprise) ? (object)DBNull.Value : nomEntreprise);
            cmd.Parameters.AddWithValue("@Referent", string.IsNullOrWhiteSpace(referent) ? (object)DBNull.Value : referent);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Insère un cuisinier lié à l'utilisateur (via ID).
        /// </summary>
        public void InsererCuisinier(int id)
        {
            string sql = @"INSERT INTO cuisinier (ID) VALUES (@ID);";

            using var cmd = new MySqlCommand(sql, connexion);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region supprimer des utilisateur BDD depuis visual studio
        public bool SupprimerUtilisateur(int idUtilisateur)
        {
            try
            {
                MySqlTransaction transaction = connexion.BeginTransaction();
                try
                {
                    using (MySqlCommand cmdClient = new MySqlCommand("DELETE FROM client WHERE ID = @id", connexion, transaction))
                    {
                        cmdClient.Parameters.AddWithValue("@id", idUtilisateur);
                        cmdClient.ExecuteNonQuery();
                    }
                    using (MySqlCommand cmdCuisinier = new MySqlCommand("DELETE FROM cuisinier WHERE ID = @id", connexion, transaction))
                    {
                        cmdCuisinier.Parameters.AddWithValue("@id", idUtilisateur);
                        cmdCuisinier.ExecuteNonQuery();
                    }
                    using (MySqlCommand cmdIdentite = new MySqlCommand("DELETE FROM identite WHERE ID = @id", connexion, transaction))
                    {
                        cmdIdentite.Parameters.AddWithValue("@id", idUtilisateur);
                        int lignesAffectees = cmdIdentite.ExecuteNonQuery();
                        transaction.Commit();
                        return lignesAffectees > 0;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Erreur lors de la suppression de l'utilisateur: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la suppression de l'utilisateur: " + ex.Message);
            }
        }
        #endregion

        #region supprimer des commandes BDD depuis visual studio
        public bool SupprimerCommande(int commandeId)
        {
            try
            {
                string deletePlatsQuery = "DELETE FROM Plat WHERE CommandeID = @CommandeID";
                using (MySqlCommand cmdPlats = new MySqlCommand(deletePlatsQuery, connexion))
                {
                    cmdPlats.Parameters.AddWithValue("@CommandeID", commandeId);
                    cmdPlats.ExecuteNonQuery();
                }
                string deleteCommandeQuery = "DELETE FROM Commande WHERE CommandeID = @CommandeID";
                using (MySqlCommand cmdCmd = new MySqlCommand(deleteCommandeQuery, connexion))
                {
                    cmdCmd.Parameters.AddWithValue("@CommandeID", commandeId);
                    int rows = cmdCmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool SupprimerToutesLesCommandes()
        {
            try
            {
                string deletePlatsQuery = "DELETE FROM Plat";
                using (MySqlCommand cmdPlats = new MySqlCommand(deletePlatsQuery, connexion))
                {
                    cmdPlats.ExecuteNonQuery();
                }

                string deleteCommandeQuery = "DELETE FROM Commande";
                using (MySqlCommand cmdCmd = new MySqlCommand(deleteCommandeQuery, connexion))
                {
                    cmdCmd.ExecuteNonQuery();
                }

                string resetCommandeAutoIncrement = "ALTER TABLE Commande AUTO_INCREMENT = 1";
                using (MySqlCommand cmdResetCmd = new MySqlCommand(resetCommandeAutoIncrement, connexion))
                {
                    cmdResetCmd.ExecuteNonQuery();
                }

                string resetPlatAutoIncrement = "ALTER TABLE Plat AUTO_INCREMENT = 1";
                using (MySqlCommand cmdResetPlat = new MySqlCommand(resetPlatAutoIncrement, connexion))
                {
                    cmdResetPlat.ExecuteNonQuery();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region afficher et modifier informations utilisateurs

        public void AfficherUtilisateurs()
        {
            string sql = "SELECT * FROM identite";

            using var cmd = new MySqlCommand(sql, connexion);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id"]}, Nom: {reader["nom"]}, Prénom: {reader["prenom"]}, Email: {reader["email"]}");
            }
        }
        public bool MettreAJourUtilisateur(Utilisateur user)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = connexion;
                    cmd.CommandText = @"
                    UPDATE Identite
                    SET 
                        Nom = @Nom,
                        Prenom = @Prenom,
                        Email = @Email,
                        Telephone = @Telephone,
                        Adresse = @Adresse,
                        MotDePasse = @MotDePasse
                    WHERE ID = @ID";

                    cmd.Parameters.AddWithValue("@Nom", user.Nom);
                    cmd.Parameters.AddWithValue("@Prenom", user.Prenom);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Telephone", user.Telephone);
                    cmd.Parameters.AddWithValue("@Adresse", user.Adresse);
                    cmd.Parameters.AddWithValue("@MotDePasse", user.MotDePasse);
                    cmd.Parameters.AddWithValue("@ID", user.ID);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la mise à jour: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Statistiques avancées

        /// <summary>
        /// Récupère les statistiques globales : total utilisateurs, clients, cuisiniers, commandes, plat le plus commandé.
        /// </summary>
        public (int TotalUtilisateurs, int TotalClients, int TotalCuisiniers, int TotalCommandes, string PlatPopulaire) GetStatistiquesGlobales()
        {
            int totalUtilisateurs = 0, totalClients = 0, totalCuisiniers = 0, totalCommandes = 0;
            string platPopulaire = "";
            Connexion();

            try
            {
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM identite", connexion))
                    totalUtilisateurs = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM client", connexion))
                    totalClients = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM cuisinier", connexion))
                    totalCuisiniers = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM commande", connexion))
                    totalCommandes = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new MySqlCommand(@"
            SELECT p.Nom 
            FROM Plat p
            WHERE p.CommandeID IS NOT NULL
            GROUP BY p.Nom
            ORDER BY COUNT(*) DESC
            LIMIT 1;", connexion))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        platPopulaire = reader.GetString("Nom");
                }
            }
            finally
            {
                FermerConnexion();
            }

            return (totalUtilisateurs, totalClients, totalCuisiniers, totalCommandes, platPopulaire);
        }

        /// <summary>
        /// Clients qui ont plus de 2 commandes (GROUP BY + HAVING)
        /// </summary>
        public List<(int ClientID, string Nom, string Prenom, int NbCommandes)> GetClientsAvecPlusDeDeuxCommandes()
        {
            var result = new List<(int, string, string, int)>();
            Connexion();
            string query = @"
        SELECT c.ClientID, i.Nom, i.Prenom, COUNT(*) AS NbCommandes
        FROM Commande c
        JOIN Identite i ON c.ClientID = i.ID
        GROUP BY c.ClientID
        HAVING COUNT(*) > 2;";
            using (var cmd = new MySqlCommand(query, connexion))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add((
                        reader.GetInt32("ClientID"),
                        reader.GetString("Nom"),
                        reader.GetString("Prenom"),
                        reader.GetInt32("NbCommandes")
                    ));
                }
            }
            FermerConnexion();
            return result;
        }

        /// <summary>
        /// Les plats qui n'ont jamais été commandés (LEFT JOIN + IS NULL)
        /// </summary>
        public List<string> GetPlatsJamaisCommandes()
        {
            var result = new List<string>();
            Connexion();
            string query = @"
        SELECT p.Nom
        FROM Plat p
        LEFT JOIN Commande c ON p.CommandeID = c.CommandeID
        WHERE c.CommandeID IS NULL;";
            using (var cmd = new MySqlCommand(query, connexion))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString("Nom"));
                }
            }
            FermerConnexion();
            return result;
        }

        /// <summary>
        /// Clients dont le nombre de commandes est supérieur à la moyenne (ALL + sous-requête)
        /// </summary>
        public List<(int ClientID, string Nom, string Prenom)> GetClientsSuperieursMoyenne()
        {
            var result = new List<(int, string, string)>();
            Connexion();
            string query = @"
        SELECT i.ID, i.Nom, i.Prenom
        FROM Identite i
        WHERE i.ID IN (
            SELECT c.ClientID
            FROM Commande c
            GROUP BY c.ClientID
            HAVING COUNT(*) > ALL (
                SELECT AVG(nb)
                FROM (
                    SELECT COUNT(*) AS nb
                    FROM Commande
                    GROUP BY ClientID
                ) AS moyennes
            )
        );";
            using (var cmd = new MySqlCommand(query, connexion))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add((
                        reader.GetInt32("ID"),
                        reader.GetString("Nom"),
                        reader.GetString("Prenom")
                    ));
                }
            }
            FermerConnexion();
            return result;
        }

        /// <summary>
        /// Vérifie si un client donné existe (EXISTS)
        /// </summary>
        public bool ExisteClient(int clientId)
        {
            Connexion();
            string query = "SELECT EXISTS (SELECT 1 FROM client WHERE ID = @ID)";
            using (var cmd = new MySqlCommand(query, connexion))
            {
                cmd.Parameters.AddWithValue("@ID", clientId);
                bool exists = Convert.ToBoolean(cmd.ExecuteScalar());
                FermerConnexion();
                return exists;
            }
        }

        #endregion


    }
}
