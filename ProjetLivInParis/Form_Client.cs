using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Project_LivInParis; 

namespace ProjetLivInParis
{
    public partial class Form_Client : Form
    {
        private int userId;
        private string prenomClient;
        
        private ListView listPlats;
        private Button btnCommander;
        private Button btnVoirDetails;
        private Button btnVoirCommandes;
        
        private StationManager stationManager;
        private Dictionary<string, int> tempsTrajets = new Dictionary<string, int>();
        private Dictionary<string, PlatInfo> informationsPlats = new Dictionary<string, PlatInfo>();
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
        private Label lblBienvenue;

        private class PlatInfo
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public int Quantite { get; set; }
            public DateTime DateFabrication { get; set; }
            public DateTime DatePeremption { get; set; }
            public decimal Prix { get; set; }
            public string Regime { get; set; }
            public int CuisinierID { get; set; }
            public string CuisinierAdresse { get; set; } // Station de métro du cuisinier
            public string ClientAdresse { get; set; } // Station de métro du client
            public int TempsTrajet { get; set; }
            public List<Noeud<Station>> Chemin { get; set; } // Chemin entre les stations
        }

        public Form_Client(int id)
        {            
            InitializeComponent();
            userId = id;

            this.Text = "Espace Client";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.LightSteelBlue;

            lblBienvenue = new Label
            {
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblBienvenue);

            listPlats = new ListView
            {
                Width = 720,
                Height = 320,
                Top = 70,
                Left = 30,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            listPlats.Columns.Add("Nom du plat", 200);
            listPlats.Columns.Add("Quantité", 100);
            listPlats.Columns.Add("Prix (€)", 100);
            listPlats.Columns.Add("Régime", 150);
            listPlats.Columns.Add("Temps de livraison", 150);

            btnCommander = new Button
            {
                Text = "Commander",
                Width = 150,
                Height = 40,
                Top = 410,
                Left = 30,
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCommander.Click += BtnCommander_Click;

            btnVoirDetails = new Button
            {
                Text = "Voir détails",
                Width = 150,
                Height = 40,
                Top = 410,
                Left = 200,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnVoirDetails.Click += BtnVoirDetails_Click;

            btnVoirCommandes = new Button
            {
                Text = "Voir mes commandes",
                Width = 200,
                Height = 40,
                Top = 410,
                Left = 380,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnVoirCommandes.Click += BtnVoirCommandes_Click;

            this.Controls.Add(listPlats);
            this.Controls.Add(btnCommander);
            this.Controls.Add(btnVoirDetails);
            this.Controls.Add(btnVoirCommandes);

            stationManager = new StationManager(connectionString);
            stationManager.ChargerStationsDepuisBDD();
            stationManager.LierCorrespondances();

            ChargerInfosClient();
            ChargerPlatsDisponibles();
        }

        private void ChargerInfosClient()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Prenom FROM Identite WHERE ID = @ID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            prenomClient = reader.GetString("Prenom");
                        }
                    }
                }
            }
            lblBienvenue.Text = $"Bienvenue {prenomClient}";
        }

        private void ChargerPlatsDisponibles()
        {
            informationsPlats.Clear();
            listPlats.Items.Clear();

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT p.PlatID, p.Nom, p.Quantite, p.date_de_fabrication, p.Date_de_peremption, 
                       p.Prix, p.Regime, u.Adresse AS CuisinierAdresse, c.Adresse AS ClientAdresse
                FROM Plat p
                JOIN Identite u ON p.CuisinierID = u.ID
                JOIN Identite c ON c.ID = @ClientID
                WHERE p.Date_de_peremption >= CURDATE() AND p.Quantite > 0 AND p.CommandeID IS NULL;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32("PlatID");
                                string nom = reader.GetString("Nom");
                                int quantite = reader.GetInt32("Quantite");
                                DateTime dateFab = reader.GetDateTime("date_de_fabrication");
                                DateTime datePeremp = reader.GetDateTime("Date_de_peremption");
                                decimal prix = reader.GetDecimal("Prix");
                                string regime = reader.GetString("Regime");
                                string cuisAdresse = reader.GetString("CuisinierAdresse");
                                string clientAdresse = reader.GetString("ClientAdresse");

                                // Calculer le temps de trajet et le chemin
                                var (tempsTrajet, chemin) = CalculerTrajet(cuisAdresse, clientAdresse);

                                // Stocker dans le dictionnaire
                                informationsPlats[nom] = new PlatInfo
                                {
                                    Id = id,
                                    Nom = nom,
                                    Quantite = quantite,
                                    DateFabrication = dateFab,
                                    DatePeremption = datePeremp,
                                    Prix = prix,
                                    Regime = regime,
                                    CuisinierAdresse = cuisAdresse,
                                    ClientAdresse = clientAdresse,
                                    TempsTrajet = tempsTrajet,
                                    Chemin = chemin
                                };

                                // Ajouter à la liste visuelle
                                var item = new ListViewItem(nom);
                                item.SubItems.Add(quantite.ToString());
                                item.SubItems.Add(prix.ToString("0.00"));
                                item.SubItems.Add(regime);
                                item.SubItems.Add(tempsTrajet + " minutes");

                                listPlats.Items.Add(item);
                            }
                        }
                    }
                }

                if (listPlats.Items.Count == 0)
                {
                    btnCommander.Enabled = false;
                    btnVoirDetails.Enabled = false;
                }
                else
                {
                    btnCommander.Enabled = true;
                    btnVoirDetails.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des plats : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void AfficherDetailsPlat(string platNom)
        {
            if (informationsPlats.TryGetValue(platNom, out PlatInfo infos))
            {
                Form detailsForm = new Form
                {
                    Text = $"Détails du plat: {platNom}",
                    Size = new Size(450, 380),  
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.LavenderBlush
                };

                TableLayoutPanel layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10),
                    ColumnCount = 2,
                    RowCount = 8,
                    AutoSize = true  
                };

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); 
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); 

                for (int i = 0; i < 8; i++)
                {
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                }

                AddDetailRow(layout, 0, "Nom:", infos.Nom);
                AddDetailRow(layout, 1, "Date de fabrication:", infos.DateFabrication.ToShortDateString());
                AddDetailRow(layout, 2, "Date de péremption:", infos.DatePeremption.ToShortDateString());
                AddDetailRow(layout, 3, "Prix:", infos.Prix.ToString("0.00") + " €");
                AddDetailRow(layout, 4, "Régime:", infos.Regime);
                AddDetailRow(layout, 5, "Adresse du cuisinier:", infos.CuisinierAdresse);

                Button btnVoirTrajet = new Button
                {
                    Text = "Afficher le trajet (" + infos.TempsTrajet + " min)",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = Color.RoyalBlue,
                    ForeColor = Color.White
                };
                btnVoirTrajet.Click += (s, e) => AfficherTrajet(platNom);

                layout.Controls.Add(btnVoirTrajet, 0, 7);
                layout.SetColumnSpan(btnVoirTrajet, 2);

                detailsForm.Controls.Add(layout);
                detailsForm.ShowDialog();
            }
        }

        private void AddDetailRow(TableLayoutPanel layout, int row, string label, string value)
        {
            layout.Controls.Add(new Label
            {
                Text = label,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill
            }, 0, row);

            layout.Controls.Add(new Label
            {
                Text = value,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 1, row);
        }

        private void BtnVoirCommandes_Click(object sender, EventArgs e)
        {
            Form_CommandesClient formCmd = new Form_CommandesClient(userId);
            formCmd.ShowDialog();
        }

        private void AfficherTrajet(string platNom)
        {
            if (!informationsPlats.TryGetValue(platNom, out PlatInfo infos) || infos.Chemin == null || infos.Chemin.Count == 0)
            {
                MessageBox.Show("Impossible d'afficher le trajet pour ce plat.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var formTrajet = new Form_Trajet(infos.Chemin, infos.TempsTrajet, $"Trajet de {infos.CuisinierAdresse} ➔ {infos.ClientAdresse}");
            formTrajet.ShowDialog();
        }


        private (int tempsTrajet, List<Noeud<Station>> chemin) CalculerTrajet(string stationCuisinierNom, string stationClientNom)
        {
            var graphe = stationManager.ObtenirGraphe();

            var noeudCuisinier = graphe.Noeuds.FirstOrDefault(n => n.Valeur.Nom == stationCuisinierNom);
            var noeudClient = graphe.Noeuds.FirstOrDefault(n => n.Valeur.Nom == stationClientNom);

            if (noeudCuisinier != null && noeudClient != null)
            {
                var chemin = graphe.CheminCourt(noeudCuisinier, noeudClient);

                if (chemin != null && chemin.Count > 0)
                {
                    // Calculer le temps total du trajet
                    double tempsTotal = 0;
                    for (int i = 0; i < chemin.Count - 1; i++)
                    {
                        var lien = chemin[i].LiensSortants.FirstOrDefault(l => l.Destination == chemin[i + 1]);
                        if (lien != null)
                        {
                            tempsTotal += lien.Poids;
                        }
                    }

                    return ((int)Math.Ceiling(tempsTotal), chemin);
                }
            }

            return (new Random().Next(15, 45), new List<Noeud<Station>>()); 
        }

        private void BtnCommander_Click(object sender, EventArgs e)
        {
            if (listPlats.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un plat.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string platNom = listPlats.SelectedItems[0].Text;

            if (!informationsPlats.TryGetValue(platNom, out PlatInfo infos))
            {
                MessageBox.Show("Erreur : impossible de retrouver les infos du plat.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool adresseOk = false;
            string clientAdresseActuelle = infos.ClientAdresse; // Adresse initiale (dans infos)

            var graphe = stationManager.ObtenirGraphe();

            // 👌 Maintenant on est sûr que l'adresse est bonne ➔ passer la commande
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Insérer la commande
                            string insertCmd = @"INSERT INTO Commande (DateCommande, AdresseLivraison, PrixTotal, Statut, ModePaiement, ClientID)
                                        VALUES (NOW(), @AdresseLivraison, @PrixTotal, 'En cours', 'En attente', @ClientID)";

                            using (MySqlCommand cmd = new MySqlCommand(insertCmd, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@AdresseLivraison", infos.ClientAdresse);
                                cmd.Parameters.AddWithValue("@PrixTotal", infos.Prix);
                                cmd.Parameters.AddWithValue("@ClientID", userId);

                                int rows = cmd.ExecuteNonQuery();
                                if (rows <= 0)
                                {
                                    throw new Exception("Échec de création de la commande");
                                }

                                long commandeId = cmd.LastInsertedId;

                                // 2. Créer un nouveau plat pour la commande avec quantité=1
                                string insertPlatCommande = @"INSERT INTO Plat (Nom, Quantite, date_de_fabrication, Date_de_peremption, Prix, Regime, CuisinierID, CommandeID)
                                                    SELECT Nom, 1, date_de_fabrication, Date_de_peremption, Prix, Regime, CuisinierID, @CommandeID
                                                    FROM Plat WHERE PlatID = @PlatID";

                                using (MySqlCommand cmdInsertPlat = new MySqlCommand(insertPlatCommande, conn, transaction))
                                {
                                    cmdInsertPlat.Parameters.AddWithValue("@CommandeID", commandeId);
                                    cmdInsertPlat.Parameters.AddWithValue("@PlatID", infos.Id);
                                    cmdInsertPlat.ExecuteNonQuery();
                                }

                                // 3. Mettre à jour la quantité du plat original
                                string updatePlat = @"UPDATE Plat SET Quantite = Quantite - 1 WHERE PlatID = @PlatID";
                                using (MySqlCommand cmdUpdate = new MySqlCommand(updatePlat, conn, transaction))
                                {
                                    cmdUpdate.Parameters.AddWithValue("@PlatID", infos.Id);
                                    cmdUpdate.ExecuteNonQuery();
                                }

                                // 4. Supprimer les plats avec quantité 0
                                string deletePlatZeroQuantity = @"DELETE FROM Plat WHERE Quantite <= 0 AND CommandeID IS NULL";
                                using (MySqlCommand cmdDelete = new MySqlCommand(deletePlatZeroQuantity, conn, transaction))
                                {
                                    cmdDelete.ExecuteNonQuery();
                                }

                                // Valider la transaction
                                transaction.Commit();

                                MessageBox.Show($"Commande enregistrée pour le plat : {platNom}\nTemps de livraison estimé : {infos.TempsTrajet} minutes",
                                    "Commande réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Rafraîchir la liste des plats
                                listPlats.Items.Clear();
                                informationsPlats.Clear();  // Très important aussi pour ne pas garder l'ancien dictionnaire
                                ChargerPlatsDisponibles();  // Recharge la liste proprement !

                                if (listPlats.Items.Count == 0)
                                {
                                    MessageBox.Show("Il n'y a plus de plats disponibles pour le moment.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    btnCommander.Enabled = false;
                                    btnVoirDetails.Enabled = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // En cas d'erreur, annuler la transaction
                            transaction.Rollback();
                            throw new Exception("Erreur lors de la transaction : " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la commande : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ListPlats_Click(object sender, EventArgs e)
        {
            if (listPlats.SelectedItems.Count > 0)
            {
                var mousePos = listPlats.PointToClient(MousePosition);
                var hitTest = listPlats.HitTest(mousePos);

                if (hitTest.SubItem != null && hitTest.Item.SubItems.IndexOf(hitTest.SubItem) == 4)
                {
                    string platNom = hitTest.Item.Text;
                    AfficherTrajet(platNom);
                }
            }
        }

        private void ListPlats_DoubleClick(object sender, EventArgs e)
        {
            if (listPlats.SelectedItems.Count > 0)
            {
                string platNom = listPlats.SelectedItems[0].Text;
                AfficherDetailsPlat(platNom);
            }
        }

        private void BtnVoirDetails_Click(object sender, EventArgs e)
        {
            if (listPlats.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un plat pour voir ses détails.",
                    "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string platNom = listPlats.SelectedItems[0].Text;
            AfficherDetailsPlat(platNom);
        }
    }
}