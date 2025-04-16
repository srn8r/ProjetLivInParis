using System;
using MySql.Data.MySqlClient;

namespace Project_LivInParis
{
    /// <summary>
    /// Interface console interactive pour la gestion de la base de données.
    /// Permet d'afficher les utilisateurs, d'ajouter un nouvel utilisateur et de quitter l'application.
    /// </summary>
    public class MenuConsoleBDD
    {
        public static void LancerMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== Menu de Gestion de la Base de Données ===");
                Console.WriteLine("1. Afficher les utilisateurs");
                Console.WriteLine("2. Ajouter un utilisateur");
                Console.WriteLine("3. Quitter");
                Console.Write("Votre choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        AfficherUtilisateursMenu();
                        break;
                    case "2":
                        AjouterUtilisateurMenu();
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour réessayer...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Affiche la liste des utilisateurs en se connectant à la base de données.
        /// </summary>
        private static void AfficherUtilisateursMenu()
        {
            InterfaceBDD interfaceBDD = new InterfaceBDD();
            interfaceBDD.Connexion();
            interfaceBDD.AfficherUtilisateurs();
            interfaceBDD.FermerConnexion();

            Console.WriteLine("Appuyez sur une touche pour revenir au menu...");
            Console.ReadKey();
        }

        /// <summary>
        /// Permet la saisie d'un nouvel utilisateur, son insertion dans la base et l'affichage de la liste actualisée.
        /// </summary>
        private static void AjouterUtilisateurMenu()
        {
            InterfaceBDD interfaceBDD = new InterfaceBDD();
            interfaceBDD.Connexion();

            Console.WriteLine("=== Ajout d'un nouvel utilisateur ===");
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

            interfaceBDD.InsererUtilisateur(nom, prenom, adresse, telephone, email, motDePasse);

            Console.WriteLine("\nUtilisateur ajouté !");
            Console.WriteLine("Liste actualisée des utilisateurs :");
            interfaceBDD.AfficherUtilisateurs();
            interfaceBDD.FermerConnexion();

            Console.WriteLine("Appuyez sur une touche pour revenir au menu...");
            Console.ReadKey();
        }
    }
}
