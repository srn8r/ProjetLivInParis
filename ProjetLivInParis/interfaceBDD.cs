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

                    // Créer le bon type d'utilisateur selon le type récupéré
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

                    // Remplir les propriétés communes
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
                        ClientPrenom = reader.GetString("ClientPrenom")
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

        #region supprimer données BDD depuis visual studio

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
        #endregion

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

    }
}
