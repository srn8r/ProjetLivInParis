using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjetLivInParis
{
    public partial class Form_ChoixRole : Form
    {
        private int userId;
        private string prenom;
        private Button btnClient;
        private Button btnCuisinier;
        private Button btnLesDeux;
        private Button btnPlusTard;
        private Label lblMessage;

        public Form_ChoixRole(int userId, string prenom)
        {
            InitializeComponent();
            this.userId = userId;
            this.prenom = prenom;
            this.Text = "Choix de rôle";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblMessage = new Label
            {
                Text = $"Bienvenue {prenom} !\nVeuillez choisir votre rôle :",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 30)
            };
            this.Controls.Add(lblMessage);

            // Bouton Client
            btnClient = new Button
            {
                Text = "Client",
                Width = 200,
                Height = 50,
                Location = new Point((this.ClientSize.Width - 200) / 2, 100),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12)
            };
            btnClient.Click += BtnClient_Click;
            this.Controls.Add(btnClient);

            // Bouton Cuisinier
            btnCuisinier = new Button
            {
                Text = "Cuisinier",
                Width = 200,
                Height = 50,
                Location = new Point((this.ClientSize.Width - 200) / 2, 170),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12)
            };
            btnCuisinier.Click += BtnCuisinier_Click;
            this.Controls.Add(btnCuisinier);

            // Bouton Les Deux
            btnLesDeux = new Button
            {
                Text = "Client et Cuisinier",
                Width = 200,
                Height = 50,
                Location = new Point((this.ClientSize.Width - 200) / 2, 240),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12)
            };
            btnLesDeux.Click += BtnLesDeux_Click;
            this.Controls.Add(btnLesDeux);

            // Bouton Plus Tard
            btnPlusTard = new Button
            {
                Text = "Choisir plus tard",
                Width = 200,
                Height = 50,
                Location = new Point((this.ClientSize.Width - 200) / 2, 310),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12)
            };
            btnPlusTard.Click += BtnPlusTard_Click;
            this.Controls.Add(btnPlusTard);
        }

        private void BtnClient_Click(object sender, EventArgs e)
        {
            // Créer un enregistrement dans la table client
            try
            {
                string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string insertClientQuery = "INSERT INTO client (ID, TypeClient) VALUES (@ID, 'Particulier')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertClientQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@ID", userId);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Votre profil a été défini comme client. Vous pouvez maintenant accéder à l'application.",
                    "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
                new Form_Client(userId).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCuisinier_Click(object sender, EventArgs e)
        {
            // Créer un enregistrement dans la table cuisinier
            try
            {
                string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string insertCuisinierQuery = "INSERT INTO cuisinier (ID) VALUES (@ID)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertCuisinierQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@ID", userId);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Votre profil a été défini comme cuisinier. Vous pouvez maintenant accéder à l'application.",
                    "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
                new Form_Cuisinier(userId).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLesDeux_Click(object sender, EventArgs e)
        {
            // Créer des enregistrements dans les tables client et cuisinier
            try
            {
                string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Ajouter en tant que client
                    string insertClientQuery = "INSERT INTO client (ID, TypeClient) VALUES (@ID, 'Particulier')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertClientQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@ID", userId);
                        insertCmd.ExecuteNonQuery();
                    }

                    // Ajouter en tant que cuisinier
                    string insertCuisinierQuery = "INSERT INTO cuisinier (ID) VALUES (@ID)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertCuisinierQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@ID", userId);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Votre profil a été défini comme client et cuisinier. Vous pourrez choisir votre rôle à chaque connexion.",
                    "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
                new Form_Choix(userId).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPlusTard_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vous n'avez choisi aucun rôle. Vous devrez choisir un rôle lors de votre prochaine connexion.",
                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
