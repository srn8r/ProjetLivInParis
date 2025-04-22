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

        /// <summary>
        /// Insère un utilisateur dans la table utilisateurs et retourne l'ID inséré.
        /// </summary>
        public int InsererUtilisateur(string nom, string prenom, string adresse, string telephone, string email, string motDePasse)
        {
            string sql = @"INSERT INTO identite (nom, prenom, adresse, telephone, email, mot_de_passe)
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
                        Email = reader.GetString("email")
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
    }
}
