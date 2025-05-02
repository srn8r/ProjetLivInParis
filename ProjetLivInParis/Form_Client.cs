using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public partial class Form_Client : Form
    {
        private int userId;
        private ListBox listPlats;
        private Button btnCommander;
        public Form_Client(int id)
        {
            InitializeComponent();
            this.Text = "Espace Client";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            userId = id;
            listPlats = new ListBox { Width = 400, Height = 200, Top = 30, Left = 40 };
            try
            {
                string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Nom FROM Plat";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string platNom = reader.GetString("Nom");
                            listPlats.Items.Add(platNom);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des plats : " + ex.Message);
            }


            btnCommander = new Button
            {
                Text = "Commander",
                Width = 150,
                Height = 40,
                Top = 250,
                Left = 180
            };
            btnCommander.Click += BtnCommander_Click;

            this.Controls.Add(listPlats);
            this.Controls.Add(btnCommander);
        }

        private void BtnCommander_Click(object sender, EventArgs e)
        {
            if (listPlats.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un plat.");
                return;
            }

            string plat = listPlats.SelectedItem.ToString();
            MessageBox.Show($"Commande enregistrée pour le plat : {plat}");
        }
    }
}