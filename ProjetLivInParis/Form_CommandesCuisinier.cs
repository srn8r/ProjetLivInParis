using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public partial class Form_CommandesCuisinier : Form
    {
        private int cuisinierId;
        private ListView lvCommandes;
        private Button btnValider;

        public Form_CommandesCuisinier(int id)
        {
            cuisinierId = id;
            this.Text = "Commandes reçues";
            this.Size = new Size(810, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;

            Label lblTitre = new Label
            {
                Text = "Commandes reçues pour vos plats",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitre);

            lvCommandes = new ListView
            {
                Location = new Point(20, 60),
                Width = 750,
                Height = 350,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            lvCommandes.Columns.Add("Commande", 100);
            lvCommandes.Columns.Add("Plat", 150);
            lvCommandes.Columns.Add("Date", 120);
            lvCommandes.Columns.Add("Adresse", 150);
            lvCommandes.Columns.Add("Prix (€)", 80);
            lvCommandes.Columns.Add("Statut", 130);

            this.Controls.Add(lvCommandes);

            btnValider = new Button
            {
                Text = "Changer Statut",
                Size = new Size(160, 40),
                Location = new Point(610, 420),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnValider.Click += BtnValider_Click;
            this.Controls.Add(btnValider);

            Button btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(100, 40),
                Location = new Point(480, 420),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnFermer.Click += (s, e) => this.Close();
            this.Controls.Add(btnFermer);

            ChargerCommandes();
        }
        /// <summary>
        /// Charge les commandes du cuisinier depuis la base de données et les affiche dans le ListView.
        /// </summary>
        private void ChargerCommandes()
        {
            lvCommandes.Items.Clear();
            string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT c.CommandeID, c.DateCommande, c.ClientID, c.AdresseLivraison, 
                           p.Nom AS PlatNom, p.Prix, c.Statut
                    FROM Commande c
                    JOIN Plat p ON p.CommandeID = c.CommandeID
                    WHERE p.CuisinierID = @CuisinierID";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CuisinierID", cuisinierId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["CommandeID"].ToString());
                            item.SubItems.Add(reader["PlatNom"].ToString());
                            item.SubItems.Add(Convert.ToDateTime(reader["DateCommande"]).ToString("dd/MM/yyyy"));
                            item.SubItems.Add(reader["AdresseLivraison"].ToString());
                            item.SubItems.Add(reader["Prix"].ToString());
                            item.SubItems.Add(reader["Statut"].ToString());

                            lvCommandes.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void BtnValider_Click(object sender, EventArgs e)
        {
            if (lvCommandes.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner une commande.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selected = lvCommandes.SelectedItems[0];
            int commandeId = int.Parse(selected.SubItems[0].Text);
            string statutActuel = selected.SubItems[5].Text;

            string nouveauStatut = Prompt.ShowDialog("Modifier le statut :", "Changer Statut", statutActuel);
            if (string.IsNullOrWhiteSpace(nouveauStatut)) return;

            string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string update = "UPDATE Commande SET Statut = @Statut WHERE CommandeID = @CommandeID";
                using (MySqlCommand cmd = new MySqlCommand(update, conn))
                {
                    cmd.Parameters.AddWithValue("@Statut", nouveauStatut);
                    cmd.Parameters.AddWithValue("@CommandeID", commandeId);
                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Statut mis à jour !");
            ChargerCommandes();
        }
    }
    /// <summary>
    /// Classe utilitaire pour afficher une boîte de dialogue de saisie de texte.
    /// </summary>
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form
            {
                Width = 400,
                Height = 180,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label lbl = new Label() { Left = 20, Top = 20, Text = text, Width = 340 };
            TextBox tb = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 260, Width = 100, Top = 90 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(lbl);
            prompt.Controls.Add(tb);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            prompt.ShowDialog();
            return tb.Text;
        }
    }
}
