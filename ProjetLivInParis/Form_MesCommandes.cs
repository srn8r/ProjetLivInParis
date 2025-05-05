using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Project_LivInParis;

namespace ProjetLivInParis
{
    public partial class Form_MesCommandes : Form
    {
        private int userId;
        private ListView listCommandes;
        private Label lblTitre;
        private Label lblTotal;
        private StationManager stationManager;
        private ContextMenuStrip contextMenu;
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";

        public Form_MesCommandes(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            this.Text = "Mes Commandes";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Beige;

            stationManager = new StationManager(connectionString);
            stationManager.ChargerStationsDepuisBDD();
            stationManager.LierCorrespondances();

            lblTitre = new Label
            {
                Text = "Vos commandes",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Location = new Point(30, 20)
            };
            this.Controls.Add(lblTitre);

            listCommandes = new ListView
            {
                Width = 720,
                Height = 300,
                Top = 70,
                Left = 30,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            listCommandes.Columns.Add("Commande", 100);
            listCommandes.Columns.Add("Nom du Plat", 200);
            listCommandes.Columns.Add("Prix (€)", 100);
            listCommandes.Columns.Add("Adresse Livraison", 200);
            listCommandes.Columns.Add("Date", 120);

            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Supprimer la commande", null, (s, ev) => SupprimerCommande());
            contextMenu.Items.Add("Voir le trajet", null, (s, ev) => VoirTrajet());

            listCommandes.MouseUp += ListCommandes_MouseUp;
            listCommandes.DoubleClick += ListCommandes_DoubleClick;
            this.Controls.Add(listCommandes);

            lblTotal = new Label
            {
                Text = "Total : 0 €",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                AutoSize = true,
                Location = new Point(30, 380)
            };
            this.Controls.Add(lblTotal);

            ChargerCommandes();
        }

        private void ChargerCommandes()
        {
            listCommandes.Items.Clear();
            decimal total = 0;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT c.CommandeID, p.Nom, p.Prix, c.AdresseLivraison, c.DateCommande
                        FROM Commande c
                        JOIN Plat p ON c.CommandeID = p.CommandeID
                        WHERE c.ClientID = @ClientID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string idCommande = reader.GetInt32("CommandeID").ToString();
                                string nomPlat = reader.GetString("Nom");
                                decimal prix = reader.GetDecimal("Prix");
                                string adresse = reader.GetString("AdresseLivraison");
                                string date = reader.GetDateTime("DateCommande").ToString("dd/MM/yyyy");

                                var item = new ListViewItem(idCommande);
                                item.SubItems.Add(nomPlat);
                                item.SubItems.Add(prix.ToString("0.00"));
                                item.SubItems.Add(adresse);
                                item.SubItems.Add(date);

                                listCommandes.Items.Add(item);
                                total += prix;
                            }
                        }
                    }
                }
                lblTotal.Text = $"Total : {total:0.00} €";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des commandes : " + ex.Message);
            }
        }

        private void ListCommandes_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = listCommandes.HitTest(e.Location);
                if (hitTest.Item != null)
                {
                    listCommandes.FocusedItem = hitTest.Item;
                    contextMenu.Show(listCommandes, e.Location);
                }
            }
        }


        private void SupprimerCommande()
        {
            if (listCommandes.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner une commande.");
                return;
            }

            string commandeId = listCommandes.SelectedItems[0].Text;

            var confirm = MessageBox.Show($"Supprimer la commande {commandeId} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Supprimer le plat lié à cette commande
                    string deletePlat = "DELETE FROM Plat WHERE CommandeID = @CommandeID";
                    using (MySqlCommand cmdPlat = new MySqlCommand(deletePlat, conn))
                    {
                        cmdPlat.Parameters.AddWithValue("@CommandeID", commandeId);
                        cmdPlat.ExecuteNonQuery();
                    }

                    // Supprimer la commande
                    string deleteCommande = "DELETE FROM Commande WHERE CommandeID = @CommandeID";
                    using (MySqlCommand cmdCommande = new MySqlCommand(deleteCommande, conn))
                    {
                        cmdCommande.Parameters.AddWithValue("@CommandeID", commandeId);
                        cmdCommande.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Commande supprimée avec succès.");
                ChargerCommandes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la suppression : " + ex.Message);
            }
        }

        private void VoirTrajet()
        {
            if (listCommandes.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner une commande.");
                return;
            }

            string platNom = listCommandes.SelectedItems[0].SubItems[1].Text;
            string adresseCuisinier = "";
            string adresseClient = listCommandes.SelectedItems[0].SubItems[3].Text;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT u.Adresse
                        FROM Plat p
                        JOIN Identite u ON p.CuisinierID = u.ID
                        WHERE p.Nom = @Nom AND p.CommandeID IS NOT NULL
                        LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nom", platNom);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                adresseCuisinier = reader.GetString("Adresse");
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(adresseCuisinier))
                {
                    MessageBox.Show("Impossible de retrouver le trajet (adresse cuisinier manquante).");
                    return;
                }

                // Calculer le trajet
                var graphe = stationManager.ObtenirGraphe();
                var noeudCuisinier = graphe.Noeuds.FirstOrDefault(n => n.Valeur.Nom == adresseCuisinier);
                var noeudClient = graphe.Noeuds.FirstOrDefault(n => n.Valeur.Nom == adresseClient);

                if (noeudCuisinier == null || noeudClient == null)
                {
                    MessageBox.Show("Stations non trouvées pour calculer le trajet.");
                    return;
                }

                var chemin = graphe.CheminCourt(noeudCuisinier, noeudClient);
                if (chemin == null || chemin.Count == 0)
                {
                    MessageBox.Show("Aucun chemin trouvé entre les stations.");
                    return;
                }

                // ✅ Afficher le trajet sous forme de MessageBox simple (pour le moment)
                var trajet = new StringBuilder();
                trajet.AppendLine($"Trajet : {adresseCuisinier} ➔ {adresseClient}\n");
                for (int i = 0; i < chemin.Count; i++)
                {
                    trajet.Append(chemin[i].Valeur.Nom);
                    if (i < chemin.Count - 1)
                        trajet.Append(" ➔ ");
                }

                MessageBox.Show(trajet.ToString(), "Trajet", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération du trajet : " + ex.Message);
            }
        }

        private void ListCommandes_DoubleClick(object sender, EventArgs e)
        {
            VoirTrajet();  // Double-clic = voir le trajet directement
        }
    }
}
