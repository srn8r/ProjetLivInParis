using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public partial class Form_PlatsCuisinier : Form
    {
        private int cuisinierId;
        private ListView lvPlats;

        public Form_PlatsCuisinier(int id)
        {
            cuisinierId = id;
            this.Text = "Mes Plats";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;

            Label lblTitre = new Label
            {
                Text = "Mes plats existants",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitre);

            lvPlats = new ListView
            {
                Location = new Point(20, 60),
                Width = 740,
                Height = 360,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            lvPlats.Columns.Add("Nom", 150);
            lvPlats.Columns.Add("Quantité", 80);
            lvPlats.Columns.Add("Prix (€)", 80);
            lvPlats.Columns.Add("Régime", 100);
            lvPlats.Columns.Add("Péremption", 120);

            this.Controls.Add(lvPlats);

            Button btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(100, 40),
                Location = new Point(660, 430),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnFermer.Click += (s, e) => this.Close();
            this.Controls.Add(btnFermer);

            ChargerPlats();
        }
        /// <summary>
        /// Charge les plats du cuisinier depuis la base de données et les affiche dans la ListView.
        /// </summary>
        private void ChargerPlats()
        {
            lvPlats.Items.Clear();
            string connectionString = AppConfig.ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT PlatID, Nom, Quantite, Prix, Regime, Date_de_peremption 
                               FROM Plat 
                               WHERE CuisinierID = @CuisinierID 
                               AND CommandeID IS NULL 
                               AND Quantite > 0 
                               AND Date_de_peremption >= CURDATE()";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CuisinierID", cuisinierId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["Nom"].ToString());
                            item.SubItems.Add(reader["Quantite"].ToString());
                            item.SubItems.Add(reader["Prix"].ToString());
                            item.SubItems.Add(reader["Regime"].ToString());
                            item.SubItems.Add(Convert.ToDateTime(reader["Date_de_peremption"]).ToShortDateString());

                            lvPlats.Items.Add(item);
                        }
                    }
                }
            }
        }
    }
}
