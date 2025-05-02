using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using MySql.Data.MySqlClient;


namespace ProjetLivInParis
{
    public partial class Form_Connecter : Form
    {
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnConnexion;
        private Label lblTitre;

        public Form_Connecter()
        {
            InitializeComponent();
            this.Text = "Connexion";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;

            // Titre
            lblTitre = new Label
            {
                Text = "Connexion",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 200) / 2, 30)
            };
            this.Controls.Add(lblTitre);

            // Email
            txtEmail = new TextBox
            {
                PlaceholderText = "Email",
                Width = 250,
                Location = new Point((this.ClientSize.Width - 250) / 2, 100)
            };
            this.Controls.Add(txtEmail);

            // Mot de passe
            txtPassword = new TextBox
            {
                PlaceholderText = "Mot de passe",
                Width = 250,
                UseSystemPasswordChar = true,
                Location = new Point((this.ClientSize.Width - 250) / 2, 150)
            };
            this.Controls.Add(txtPassword);

            // Bouton connexion
            btnConnexion = new Button
            {
                Text = "Se connecter",
                Width = 150,
                Height = 40,
                Location = new Point((this.ClientSize.Width - 150) / 2, 230),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnConnexion.Click += BtnConnexion_Click;
            this.Controls.Add(btnConnexion);

            CheckBox cbAfficherMotDePasse = new CheckBox
            {
                Text = "Afficher le mot de passe",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(txtPassword.Left, txtPassword.Top + 35)
            };
            cbAfficherMotDePasse.CheckedChanged += CbAfficherMotDePasse_CheckedChanged;
            this.Controls.Add(cbAfficherMotDePasse);

        }

        private void BtnConnexion_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (email == "" || password == "")
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT ID, Prenom FROM identite WHERE Email = @Email AND MotDePasse = @MotDePasse";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@MotDePasse", password);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32("ID");
                                string prenom = reader.GetString("Prenom");

                                reader.Close(); // fermer avant les autres requêtes

                                // Vérifier s'il est client
                                bool estClient = false;
                                string queryClient = "SELECT COUNT(*) FROM client WHERE ID = @ID";
                                using (MySqlCommand cmdClient = new MySqlCommand(queryClient, conn))
                                {
                                    cmdClient.Parameters.AddWithValue("@ID", userId);
                                    estClient = Convert.ToInt32(cmdClient.ExecuteScalar()) > 0;
                                }

                                // Vérifier s'il est cuisinier
                                bool estCuisinier = false;
                                string queryCuisinier = "SELECT COUNT(*) FROM cuisinier WHERE ID = @ID";
                                using (MySqlCommand cmdCuisinier = new MySqlCommand(queryCuisinier, conn))
                                {
                                    cmdCuisinier.Parameters.AddWithValue("@ID", userId);
                                    estCuisinier = Convert.ToInt32(cmdCuisinier.ExecuteScalar()) > 0;
                                }

                                // Diriger vers la bonne Form
                                if (estClient && estCuisinier)
                                {
                                    MessageBox.Show($"Bienvenue {prenom} ! Vous êtes client et cuisinier.");
                                    new Form_Choix(userId).ShowDialog();
                                }
                                else if (estClient)
                                {
                                    MessageBox.Show($"Bienvenue {prenom} ! (Client)");
                                    new Form_Client(userId).ShowDialog();
                                }
                                else if (estCuisinier)
                                {
                                    MessageBox.Show($"Bienvenue {prenom} ! (Cuisinier)");
                                    new Form_Cuisinier(userId).ShowDialog();
                                }
                                else
                                {
                                    MessageBox.Show("Votre profil n'est pas encore défini comme client ou cuisinier.");
                                }

                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Email ou mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la connexion à la base de données : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CbAfficherMotDePasse_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                txtPassword.UseSystemPasswordChar = !checkBox.Checked;
            }
        }

    }
}
