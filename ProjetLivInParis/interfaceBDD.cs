using System;
using MySql.Data.MySqlClient;

namespace Project_LivInParis
{
    /// <summary>
    /// Fournit une interface pour se connecter à la base de données, insérer et afficher des utilisateurs.
    /// </summary>
    public class InterfaceBDD
    {
        private MySqlConnection maConnexion;

        /// <summary>
        /// Initialise une nouvelle instance de la classe InterfaceBDD.
        /// </summary>
        public InterfaceBDD()
        {
            maConnexion = null;
        }

        /// <summary>
        /// Établit la connexion à la base de données en utilisant la chaîne de connexion spécifiée.
        /// Affiche un message de succès ou d'erreur en cas d'échec.
        /// </summary>
        public void Connexion()
        {
            try
            {
                string connectionString = "SERVER=127.0.0.1;PORT=3306;" +
                                          "DATABASE=livinparis;" +
                                          "UID=root;PASSWORD=root;";
                maConnexion = new MySqlConnection(connectionString);
                maConnexion.Open();
                Console.WriteLine("Connexion réussie !");
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erreur de connexion : " + e.ToString());
            }
        }

        /// <summary>
        /// Insère un nouvel utilisateur dans la table 'identite' de la base de données.
        /// </summary>
        /// <param name="nom">Nom de l'utilisateur.</param>
        /// <param name="prenom">Prénom de l'utilisateur.</param>
        /// <param name="adresse">Adresse de l'utilisateur.</param>
        /// <param name="telephone">Téléphone de l'utilisateur.</param>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="motDePasse">Mot de passe de l'utilisateur.</param>
        public void InsererUtilisateur(string nom, string prenom, string adresse, string telephone, string email, string motDePasse)
        {
            if (maConnexion == null || maConnexion.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("La connexion n'est pas ouverte.");
                return;
            }

            string query = "INSERT INTO identite (Nom, Prenom, Adresse, Telephone, Email, MotDePasse) " +
                           "VALUES (@nom, @prenom, @adresse, @telephone, @email, @motDePasse)";

            using (MySqlCommand command = new MySqlCommand(query, maConnexion))
            {
                command.Parameters.AddWithValue("@nom", nom);
                command.Parameters.AddWithValue("@prenom", prenom);
                command.Parameters.AddWithValue("@adresse", adresse);
                command.Parameters.AddWithValue("@telephone", telephone);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@motDePasse", motDePasse);

                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} utilisateur(s) inséré(s).");
            }
        }

        /// <summary>
        /// Affiche dans la console la liste de tous les utilisateurs présents dans la table 'identite'.
        /// </summary>
        public void AfficherUtilisateurs()
        {
            if (maConnexion == null || maConnexion.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("La connexion n'est pas ouverte.");
                return;
            }

            string query = "SELECT * FROM identite";

            using (MySqlCommand command = new MySqlCommand(query, maConnexion))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("Liste des utilisateurs :");

                while (reader.Read())
                {
                    string id = reader["ID"].ToString();
                    string prenom = reader["Prenom"].ToString();
                    string nom = reader["Nom"].ToString();
                    string adresse = reader["Adresse"].ToString();
                    string telephone = reader["Telephone"].ToString();
                    string email = reader["Email"].ToString();

                    Console.WriteLine($"{id} {prenom} {nom} - {adresse}, {telephone}, {email}");
                }
            }
        }

        /// <summary>
        /// Ferme la connexion à la base de données si elle est ouverte.
        /// </summary>
        public void FermerConnexion()
        {
            if (maConnexion != null && maConnexion.State == System.Data.ConnectionState.Open)
            {
                maConnexion.Close();
                Console.WriteLine("Connexion fermée.");
            }
        }

        /// <summary>
        /// Exécute l'interface de gestion de la base de données.
        /// Cette méthode se connecte à la BDD, affiche la liste des utilisateurs,
        /// demande à l'utilisateur de saisir de nouvelles informations, insère ces données,
        /// réaffiche la liste actualisée et ferme la connexion.
        /// </summary>
        public void Execution()
        {
            Connexion();
            AfficherUtilisateurs();

            // Saisie utilisateur
            Console.Write("Nom : ");
            string nom = Console.ReadLine();
            Console.Write("Prénom : ");
            string prenom = Console.ReadLine();
            Console.Write("Adresse : ");
            string adresse = Console.ReadLine();
            Console.Write("Téléphone : ");
            string telephone = Console.ReadLine();
            Console.Write("Email : ");
            string email = Console.ReadLine();
            Console.Write("Mot de passe : ");
            string motDePasse = Console.ReadLine();

            // Insertion dans la base de données
            InsererUtilisateur(nom, prenom, adresse, telephone, email, motDePasse);

            // Affichage des utilisateurs mis à jour
            AfficherUtilisateurs();

            FermerConnexion();
        }
    }
}
