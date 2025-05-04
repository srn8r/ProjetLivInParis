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
using Project_LivInParis; 

namespace ProjetLivInParis
{
    public partial class Form_Client : Form
    {
        private int userId;
        private ListView listPlats;
        private Button btnCommander;
        private Button btnVoirDetails;
        private StationManager stationManager;
        private Dictionary<string, int> tempsTrajets = new Dictionary<string, int>();
        private Dictionary<string, PlatInfo> informationsPlats = new Dictionary<string, PlatInfo>();
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";

        private class PlatInfo
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string Quantite { get; set; }
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
            this.Text = "Espace Client";
            this.Size = new Size(750, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.LightSteelBlue;
            userId = id;

            stationManager = new StationManager(connectionString);
            stationManager.ChargerStationsDepuisBDD();
            stationManager.LierCorrespondances();

            string clientAdresse = "";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string queryClient = "SELECT Adresse FROM Identite WHERE ID = @ClientID";
                    using (MySqlCommand cmd = new MySqlCommand(queryClient, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                clientAdresse = reader.GetString("Adresse");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération des informations client : " + ex.Message);
            }

            Label titre = new Label
            {
                Text = "Liste des plats disponibles",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Location = new Point(40, 20)
            };
            this.Controls.Add(titre);

            listPlats = new ListView
            {
                Width = 650,
                Height = 300,
                Top = 60,
                Left = 40,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            listPlats.Columns.Add("Nom du plat", 200);
            listPlats.Columns.Add("Quantite", 120);
            listPlats.Columns.Add("Prix (€)", 80);
            listPlats.Columns.Add("Régime", 100);
            listPlats.Columns.Add("Temps de livraison", 150);

            listPlats.DoubleClick += ListPlats_DoubleClick;
            listPlats.Click += ListPlats_Click;

            ChargerPlats(clientAdresse);

            btnCommander = new Button
            {
                Text = "Commander",
                Width = 150,
                Height = 40,
                Top = 380,
                Left = 40,
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
                Top = 380,
                Left = 210,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnVoirDetails.Click += BtnVoirDetails_Click;

            this.Controls.Add(listPlats);
            this.Controls.Add(btnCommander);
            this.Controls.Add(btnVoirDetails);
        }

        private void ChargerPlats(string clientAdresse)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT p.PlatID, p.Nom, p.Quantite, p.date_de_fabrication, p.Date_de_peremption, 
                               p.Prix, p.Regime, p.CuisinierID, u.Adresse
                        FROM Plat p
                        JOIN Identite u ON p.CuisinierID = u.ID
                        WHERE p.Date_de_peremption >= CURDATE() AND p.CommandeID IS NULL
";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int platId = reader.GetInt32("PlatID");
                            string platNom = reader.GetString("Nom");
                            int quantite = reader.GetInt32("Quantite");
                            DateTime dateFab = reader.GetDateTime("date_de_fabrication");
                            DateTime datePer = reader.GetDateTime("Date_de_peremption");
                            decimal prix = reader.GetDecimal("Prix");
                            string regime = reader.GetString("Regime");
                            int cuisinierId = reader.GetInt32("CuisinierID");
                            string cuisinierAdresse = reader.GetString("Adresse");

                            var (tempsTrajet, chemin) = CalculerTrajet(cuisinierAdresse, clientAdresse);

 
                            PlatInfo infos = new PlatInfo
                            {
                                Id = platId,
                                Nom = platNom,
                                DateFabrication = dateFab,
                                DatePeremption = datePer,
                                Prix = prix,
                                Regime = regime,
                                CuisinierID = cuisinierId,
                                CuisinierAdresse = cuisinierAdresse,
                                ClientAdresse = clientAdresse,
                                TempsTrajet = tempsTrajet,
                                Chemin = chemin
                            };

                            informationsPlats.Add(platNom, infos);

                            ListViewItem item = new ListViewItem(platNom);
                            item.SubItems.Add(quantite.ToString());
                            item.SubItems.Add(prix.ToString("0.00"));
                            item.SubItems.Add(regime);
                            item.SubItems.Add(tempsTrajet + " min");
                            item.Tag = platNom; 

                            listPlats.Items.Add(item);
                            if (listPlats.Items.Count == 0)
                            {
                                MessageBox.Show("Il n'y a plus de plats disponibles pour le moment.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des plats : " + ex.Message);
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
                //AddDetailRow(layout, 1, "Quantite:", infos.Quantite);
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

        private void AfficherTrajet(string platNom)
        {
            if (!informationsPlats.TryGetValue(platNom, out PlatInfo infos) || infos.Chemin == null || infos.Chemin.Count == 0)
            {
                MessageBox.Show("Impossible d'afficher le trajet pour ce plat.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var segmentsParLigne = ExtraireTrajetParLigne(infos.Chemin);

            Form formTrajet = new Form
            {
                Text = $"Trajet de livraison: {platNom}",
                Size = new Size(800, Math.Max(400, 100 + segmentsParLigne.Count * 90)), // Ajustement dynamique de la hauteur
                StartPosition = FormStartPosition.CenterParent,
                AutoScroll = true // Activer le défilement si nécessaire
            };

            Panel panelTrajet = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            Dictionary<string, Color> couleursParLigne = new Dictionary<string, Color>(){
                { "1", Color.Gold },
                { "2", Color.Blue },
                { "3", Color.Brown },
                { "3bis", Color.LightSkyBlue },
                { "4", Color.Purple },
                { "5", Color.Orange },
                { "6", Color.LightGreen },
                { "7", Color.Pink },
                { "7bis", Color.LightPink },
                { "8", Color.Gray },
                { "9", Color.Olive },
                { "10", Color.DarkGoldenrod },
                { "11", Color.DarkOrange },
                { "12", Color.SeaGreen },
                { "13", Color.Teal },
                { "14", Color.DarkViolet },
            };

            panelTrajet.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Font stationFont = new Font("Segoe UI", 9);
                Font numLigneFont = new Font("Segoe UI", 9, FontStyle.Bold);
                Font titreFont = new Font("Segoe UI", 12, FontStyle.Bold);

                int paddingLateral = 80; 
                int hauteurSegment = 90;
                int largeurTotale = panelTrajet.ClientSize.Width - (paddingLateral * 2);
                int rayonStation = 8; 

   
                int hauteurTitre = 50;
                Rectangle zoneHeader = new Rectangle(10, 10, panelTrajet.ClientSize.Width - 20, hauteurTitre);
                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 250)), zoneHeader);
                g.DrawRectangle(Pens.LightGray, zoneHeader);

        
                g.DrawString($"Temps de trajet: {infos.TempsTrajet} minutes",
                    titreFont, Brushes.DarkBlue, zoneHeader.X + 10, zoneHeader.Y + 15);

                int yPosition = hauteurTitre + 30; 

                HashSet<string> segmentsAffichés = new HashSet<string>();

                foreach (var segment in segmentsParLigne)
                {
                    try
                    {
                        if (segmentsAffichés.Contains(segment))
                            continue;

                        segmentsAffichés.Add(segment);

                        string[] parts = segment.Split(':');
                        if (parts.Length != 2) continue;

                        string ligne = parts[0].Replace("Ligne ", "").Trim();
                        string[] stations = parts[1].Split('→');
                        if (stations.Length != 2) continue;

                        string stationDepart = stations[0].Trim();
                        string stationArrivee = stations[1].Trim();

                        SizeF tailleDepart = g.MeasureString(stationDepart, stationFont);
                        SizeF tailleArrivee = g.MeasureString(stationArrivee, stationFont);
                        SizeF tailleLigne = g.MeasureString($"Ligne {ligne}", numLigneFont);

                        Color couleurLigne = Color.Red;
                        if (couleursParLigne.TryGetValue(ligne, out Color couleur))
                            couleurLigne = couleur;

                        using (Pen penLigne = new Pen(couleurLigne, 6))
                        {
                            // Dessiner la ligne horizontale
                            g.DrawLine(penLigne, paddingLateral, yPosition, paddingLateral + largeurTotale, yPosition);

                            // Dessiner les stations (cercles)
                            int yCircle = yPosition - rayonStation;
                            // Station de départ
                            g.FillEllipse(Brushes.White, paddingLateral - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);
                            g.DrawEllipse(Pens.Black, paddingLateral - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);

                            // Station d'arrivée
                            g.FillEllipse(Brushes.White, paddingLateral + largeurTotale - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);
                            g.DrawEllipse(Pens.Black, paddingLateral + largeurTotale - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);

                            // Dessiner les noms des stations avec centrage
                            float xDepartText = Math.Max(10, paddingLateral - (tailleDepart.Width / 2));
                            g.DrawString(stationDepart, stationFont, Brushes.Black, xDepartText, yPosition + 15);

                            float xArriveeText = paddingLateral + largeurTotale - (tailleArrivee.Width / 2);
                            g.DrawString(stationArrivee, stationFont, Brushes.Black, xArriveeText, yPosition + 15);

                            // Rectangle pour le numéro de ligne (avec fond blanc semi-transparent)
                            float xLigneText = paddingLateral + (largeurTotale / 2) - (tailleLigne.Width / 2);
                            RectangleF rectLigne = new RectangleF(xLigneText - 4, yPosition + 40, tailleLigne.Width + 8, tailleLigne.Height + 2);
                            g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 255, 255)), rectLigne);

                            // Dessiner le numéro de ligne en dessous
                            using (SolidBrush brush = new SolidBrush(couleurLigne))
                            {
                                g.DrawString($"Ligne {ligne}", numLigneFont, brush, xLigneText, yPosition + 40);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // En cas d'erreur de format, continuer avec le segment suivant
                        System.Diagnostics.Debug.WriteLine($"Erreur d'affichage pour segment: {segment}. {ex.Message}");
                    }

                    yPosition += hauteurSegment; // Passage au segment suivant
                }

                // Assurer que le panel a une taille suffisante pour tout afficher
                panelTrajet.AutoScrollMinSize = new Size(panelTrajet.ClientSize.Width, yPosition + 20);
            };

            formTrajet.Controls.Add(panelTrajet);
            formTrajet.ShowDialog();
        }

        private List<string> ExtraireTrajetParLigne(List<Noeud<Station>> chemin)
        {
            var lignesDetaillees = new List<string>();

            if (chemin == null || chemin.Count < 2) return lignesDetaillees;

            string ligneActuelle = chemin[0].Valeur.Ligne;
            string depart = chemin[0].Valeur.Nom;

            for (int i = 1; i < chemin.Count; i++)
            {
                var courant = chemin[i - 1];
                var suivant = chemin[i];

                if (courant.Valeur.Nom == suivant.Valeur.Nom)
                {
                    continue;
                }

                if (suivant.Valeur.Ligne != ligneActuelle)
                {
                    lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} → {courant.Valeur.Nom}");
                    depart = courant.Valeur.Nom;
                    ligneActuelle = suivant.Valeur.Ligne;
                }
            }
            if (depart != chemin.Last().Valeur.Nom)
            {
                lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} → {chemin.Last().Valeur.Nom}");
            }


            lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} → {chemin.Last().Valeur.Nom}");
            return lignesDetaillees;
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

            while (!adresseOk)
            {
                var graphe = stationManager.ObtenirGraphe();
                adresseOk = graphe.Noeuds.Any(n => n.Valeur.Nom.Equals(clientAdresseActuelle, StringComparison.OrdinalIgnoreCase));

                if (!adresseOk)
                {
                    // Demander au client de choisir une station valide
                    List<string> toutesStations = graphe.Noeuds.Select(n => n.Valeur.Nom).OrderBy(n => n).ToList();
                    using (var formChoix = new Form_ChoisirStation(toutesStations))
                    {
                        var result = formChoix.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            string stationChoisie = formChoix.StationChoisie;

                            // ➔ Mettre à jour la table Identite
                            try
                            {
                                using (MySqlConnection conn = new MySqlConnection(connectionString))
                                {
                                    conn.Open();
                                    string update = "UPDATE Identite SET Adresse = @Station WHERE ID = @ClientID";
                                    using (MySqlCommand cmdMaj = new MySqlCommand(update, conn))
                                    {
                                        cmdMaj.Parameters.AddWithValue("@Station", stationChoisie);
                                        cmdMaj.Parameters.AddWithValue("@ClientID", userId);
                                        cmdMaj.ExecuteNonQuery();
                                    }
                                }
                                MessageBox.Show($"Votre station a été mise à jour : {stationChoisie}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Mettre à jour localement aussi
                                clientAdresseActuelle = stationChoisie;
                                infos.ClientAdresse = stationChoisie;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Erreur lors de la mise à jour de votre station : " + ex.Message);
                                return; // stoppe si erreur critique
                            }
                        }
                        else
                        {
                            // L'utilisateur a annulé -> on stoppe la commande
                            return;
                        }
                    }
                }
            }


            // 👌 Maintenant on est sûr que l'adresse est bonne ➔ passer la commande
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string insertCmd = @"INSERT INTO Commande (DateCommande, AdresseLivraison, PrixTotal, Statut, ModePaiement, ClientID)
                                    VALUES (NOW(), @AdresseLivraison, @PrixTotal, 'En préparation', 'En attente', @ClientID)";

                    using (MySqlCommand cmd = new MySqlCommand(insertCmd, conn))
                    {
                        cmd.Parameters.AddWithValue("@AdresseLivraison", infos.ClientAdresse);
                        cmd.Parameters.AddWithValue("@PrixTotal", infos.Prix);
                        cmd.Parameters.AddWithValue("@ClientID", userId);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            long commandeId = cmd.LastInsertedId;

                            string updatePlat = @"
                                    UPDATE Plat 
                                    SET Quantite = Quantite - 1, 
                                    CommandeID = CASE WHEN Quantite = 1 THEN @CommandeID ELSE CommandeID END
                                    WHERE PlatID = @PlatID";
                            using (MySqlCommand cmdUpdate = new MySqlCommand(updatePlat, conn))
                            {
                                cmdUpdate.Parameters.AddWithValue("@CommandeID", commandeId);
                                cmdUpdate.Parameters.AddWithValue("@PlatID", infos.Id);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            MessageBox.Show($"Commande enregistrée pour le plat : {platNom}\nTemps de livraison estimé : {infos.TempsTrajet} minutes",
                                "Commande réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                           

                            listPlats.Items.Clear();
                            informationsPlats.Clear();  // Très important aussi pour ne pas garder l'ancien dictionnaire
                            ChargerPlats(infos.ClientAdresse);  // Recharge la liste proprement !

                            if (listPlats.Items.Count == 0)
                            {
                                MessageBox.Show("Il n'y a plus de plats disponibles pour le moment.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                btnCommander.Enabled = false;
                                btnVoirDetails.Enabled = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Erreur lors de l'enregistrement de la commande.",
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la commande : " + ex.Message);
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