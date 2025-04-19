using System;
using System.Data;
using MySql.Data.MySqlClient;

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
        /// Insère un utilisateur dans la table identite et retourne l'ID inséré.
        /// </summary>
        public int InsererUtilisateur(string nom, string prenom, string adresse, string telephone, string email, string motDePasse)
        {
            string sql = @"INSERT INTO identite (Nom, Prenom, Adresse, Telephone, Email, MotDePasse)
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
            string sql = @"INSERT INTO Client (ID, TypeClient, NomEntreprise, Referent)
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
            string sql = @"INSERT INTO Cuisinier (ID) VALUES (@ID);";

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
                Console.WriteLine($"ID: {reader["ID"]}, Nom: {reader["Nom"]}, Prénom: {reader["Prenom"]}, Email: {reader["Email"]}");
            }
        }
        public class Utilisateur
        {
            public int ID { get; set; }
            public string Nom { get; set; }
            public string Prenom { get; set; }
            public string Email { get; set; }
        }
        public List<Utilisateur> RecupererUtilisateurs()
        {
            var liste = new List<Utilisateur>();

            string sql = "SELECT ID, Nom, Prenom, Email FROM identite ORDER BY ID";

            using var cmd = new MySqlCommand(sql, connexion);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var user = new Utilisateur
                {
                    ID = reader.GetInt32("ID"),
                    Nom = reader.GetString("Nom"),
                    Prenom = reader.GetString("Prenom"),
                    Email = reader.GetString("Email")
                };
                liste.Add(user);
            }

            return liste;
        }



    }
}
