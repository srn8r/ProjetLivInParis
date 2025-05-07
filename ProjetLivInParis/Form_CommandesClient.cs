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
    public partial class Form_CommandesClient : Form
    {
        private int userId;
        private ListView listCommandes;
        private Label lblTitre;
        private Label lblTotal;
        private StationManager stationManager;
        private ContextMenuStrip contextMenu;
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";

        public Form_CommandesClient(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            this.Text = "Mes Commandes";
            this.Size = new Size(940, 500);
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
                Width = 840,
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
            listCommandes.Columns.Add("Statut", 120);

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
        /// <summary>
        /// Charge les commandes du client depuis la base de données et les affiche dans la ListView.
        /// </summary>
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
                        SELECT c.CommandeID, p.Nom, p.Prix, c.AdresseLivraison, c.DateCommande, c.statut
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
                                string statut = reader.GetString("Statut");

                                var item = new ListViewItem(idCommande);
                                item.SubItems.Add(nomPlat);
                                item.SubItems.Add(prix.ToString("0.00"));
                                item.SubItems.Add(adresse);
                                item.SubItems.Add(date);
                                item.SubItems.Add(statut);

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
        /// <summary>
        /// Gère l'événement de clic droit sur la ListView pour afficher le menu contextuel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Supprime la commande sélectionnée.
        /// </summary>
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

                    string deletePlat = "DELETE FROM Plat WHERE CommandeID = @CommandeID";
                    using (MySqlCommand cmdPlat = new MySqlCommand(deletePlat, conn))
                    {
                        cmdPlat.Parameters.AddWithValue("@CommandeID", commandeId);
                        cmdPlat.ExecuteNonQuery();
                    }

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
        /// <summary>
        /// Affiche le trajet entre le cuisinier et le client pour la commande sélectionnée.
        /// </summary>
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

                int tempsTrajet = chemin.Count * 2;

                var formTrajet = new Form_Trajet(chemin, tempsTrajet, $"Trajet de {adresseCuisinier} ➔ {adresseClient}");
                formTrajet.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération du trajet : " + ex.Message);
            }
        }
        /// <summary>
        /// Gère l'événement de double clic sur la ListView pour afficher le trajet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListCommandes_DoubleClick(object sender, EventArgs e)
        {
            VoirTrajet();
        }
    }
}
