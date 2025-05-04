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

        // Structure pour stocker les informations d'un plat
        private class PlatInfo
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string Type { get; set; }
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

            // Initialiser le gestionnaire de stations pour calculer les temps de trajet
            stationManager = new StationManager(connectionString);
            stationManager.ChargerStationsDepuisBDD();
            stationManager.LierCorrespondances();

            // Récupérer l'adresse du client (station de métro)
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

            // Créer les contrôles UI
            Label titre = new Label
            {
                Text = "Liste des plats disponibles",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Location = new Point(40, 20)
            };
            this.Controls.Add(titre);

            // Créer une ListView au lieu d'une ListBox pour plus de détails
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

            // Ajouter les colonnes
            listPlats.Columns.Add("Nom du plat", 200);
            listPlats.Columns.Add("Type", 100);
            listPlats.Columns.Add("Prix (€)", 80);
            listPlats.Columns.Add("Régime", 100);
            listPlats.Columns.Add("Temps de livraison", 150);

            // Ajouter des gestionnaires d'événements
            listPlats.DoubleClick += ListPlats_DoubleClick;
            listPlats.Click += ListPlats_Click;

            // Charger les plats depuis la base de données
            ChargerPlats(clientAdresse);

            // Bouton Commander
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

            // Bouton Voir Détails
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
                        SELECT p.PlatID, p.Nom, p.Type, p.date_de_fabrication, p.Date_de_peremption, 
                               p.Prix, p.Regime, p.CuisinierID, u.Adresse
                        FROM Plat p
                        JOIN Identite u ON p.CuisinierID = u.ID
                        WHERE p.Date_de_peremption >= CURDATE()";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int platId = reader.GetInt32("PlatID");
                            string platNom = reader.GetString("Nom");
                            string platType = reader.GetString("Type");
                            DateTime dateFab = reader.GetDateTime("date_de_fabrication");
                            DateTime datePer = reader.GetDateTime("Date_de_peremption");
                            decimal prix = reader.GetDecimal("Prix");
                            string regime = reader.GetString("Regime");
                            int cuisinierId = reader.GetInt32("CuisinierID");
                            string cuisinierAdresse = reader.GetString("Adresse");

                            // Calculer le temps de trajet et le chemin entre le cuisinier et le client
                            var (tempsTrajet, chemin) = CalculerTrajet(cuisinierAdresse, clientAdresse);

                            // Créer un objet PlatInfo pour stocker toutes les informations
                            PlatInfo infos = new PlatInfo
                            {
                                Id = platId,
                                Nom = platNom,
                                Type = platType,
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

                            // Ajouter à la ListView
                            ListViewItem item = new ListViewItem(platNom);
                            item.SubItems.Add(platType);
                            item.SubItems.Add(prix.ToString("0.00"));
                            item.SubItems.Add(regime);
                            item.SubItems.Add(tempsTrajet + " min");
                            item.Tag = platNom; // Stocker le nom du plat pour référence future

                            listPlats.Items.Add(item);
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
                    Size = new Size(400, 350),
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
                    RowCount = 8
                };

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

                AddDetailRow(layout, 0, "Nom:", infos.Nom);
                AddDetailRow(layout, 1, "Type:", infos.Type);
                AddDetailRow(layout, 2, "Date de fabrication:", infos.DateFabrication.ToShortDateString());
                AddDetailRow(layout, 3, "Date de péremption:", infos.DatePeremption.ToShortDateString());
                AddDetailRow(layout, 4, "Prix:", infos.Prix.ToString("0.00") + " €");
                AddDetailRow(layout, 5, "Régime:", infos.Regime);
                AddDetailRow(layout, 6, "Station du cuisinier:", infos.CuisinierAdresse);

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

            // Récupérer les segments par ligne pour l'affichage
            var segmentsParLigne = ExtraireTrajetParLigne(infos.Chemin);

            // Créer un nouveau formulaire pour afficher le trajet
            Form formTrajet = new Form
            {
                Text = $"Trajet de livraison: {platNom}",
                Size = new Size(800, Math.Max(400, 100 + segmentsParLigne.Count * 90)), // Ajustement dynamique de la hauteur
                StartPosition = FormStartPosition.CenterParent,
                AutoScroll = true // Activer le défilement si nécessaire
            };

            // Panel avec autoscroll
            Panel panelTrajet = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            // Dictionnaire des couleurs par ligne
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

            // Dessiner le trajet
            panelTrajet.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Font stationFont = new Font("Segoe UI", 9);
                Font numLigneFont = new Font("Segoe UI", 9, FontStyle.Bold);
                Font titreFont = new Font("Segoe UI", 12, FontStyle.Bold);

                // Calculer paramètres d'affichage
                int paddingLateral = 80; // Plus grand pour les noms de stations
                int hauteurSegment = 90; // Plus d'espace vertical entre segments
                int largeurTotale = panelTrajet.ClientSize.Width - (paddingLateral * 2);
                int rayonStation = 8; // Stations un peu plus grandes

                // Zone de titre
                int hauteurTitre = 50;
                Rectangle zoneHeader = new Rectangle(10, 10, panelTrajet.ClientSize.Width - 20, hauteurTitre);
                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 250)), zoneHeader);
                g.DrawRectangle(Pens.LightGray, zoneHeader);

                // Afficher le temps total
                g.DrawString($"Temps de trajet: {infos.TempsTrajet} minutes",
                    titreFont, Brushes.DarkBlue, zoneHeader.X + 10, zoneHeader.Y + 15);

                int yPosition = hauteurTitre + 30; // Position de départ après le titre

                // Set pour suivre les segments déjà affichés afin d'éviter les doublons
                HashSet<string> segmentsAffichés = new HashSet<string>();

                // Pour chaque segment de ligne, dessiner un segment horizontal
                foreach (var segment in segmentsParLigne)
                {
                    try
                    {
                        // Vérifier si ce segment a déjà été affiché (pour éviter les doublons)
                        if (segmentsAffichés.Contains(segment))
                            continue;

                        segmentsAffichés.Add(segment);

                        // Extraire les informations du segment
                        string[] parts = segment.Split(':');
                        if (parts.Length != 2) continue;

                        string ligne = parts[0].Replace("Ligne ", "").Trim();
                        string[] stations = parts[1].Split('→');
                        if (stations.Length != 2) continue;

                        string stationDepart = stations[0].Trim();
                        string stationArrivee = stations[1].Trim();

                        // Mesurer la taille des textes pour un meilleur positionnement
                        SizeF tailleDepart = g.MeasureString(stationDepart, stationFont);
                        SizeF tailleArrivee = g.MeasureString(stationArrivee, stationFont);
                        SizeF tailleLigne = g.MeasureString($"Ligne {ligne}", numLigneFont);

                        // Couleur de la ligne
                        Color couleurLigne = Color.Red; // par défaut
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

                if (suivant.Valeur.Ligne != ligneActuelle)
                {
                    lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} → {courant.Valeur.Nom}");
                    depart = courant.Valeur.Nom;
                    ligneActuelle = suivant.Valeur.Ligne;
                }
            }

            lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} → {chemin.Last().Valeur.Nom}");
            return lignesDetaillees;
        }


        private (int tempsTrajet, List<Noeud<Station>> chemin) CalculerTrajet(string stationCuisinierNom, string stationClientNom)
        {
            var graphe = stationManager.ObtenirGraphe();

            // Trouver les nœuds correspondant aux stations
            var noeudCuisinier = graphe.Noeuds.FirstOrDefault(n => n.Valeur.Nom == stationCuisinierNom);
            var noeudClient = graphe.Noeuds.FirstOrDefault(n => n.Valeur.Nom == stationClientNom);

            if (noeudCuisinier != null && noeudClient != null)
            {
                // Calculer le plus court chemin
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

            // Valeur par défaut si le calcul n'est pas possible
            return (new Random().Next(15, 45), new List<Noeud<Station>>()); // Simuler un temps entre 15 et 45 minutes
        }

        private void BtnCommander_Click(object sender, EventArgs e)
        {
            if (listPlats.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un plat.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string platNom = listPlats.SelectedItems[0].Text;
            if (informationsPlats.TryGetValue(platNom, out PlatInfo infos))
            {
                // Enregistrer la commande dans la base de données
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string insertCmd = @"
                        INSERT INTO Commande (ClientID, PlatID, Date_commande, Status)
                        VALUES (@ClientID, @PlatID, NOW(), 'En préparation')";

                        using (MySqlCommand cmd = new MySqlCommand(insertCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@ClientID", userId);
                            cmd.Parameters.AddWithValue("@PlatID", infos.Id);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show($"Commande enregistrée pour le plat : {platNom}\nTemps de livraison estimé : {infos.TempsTrajet} minutes",
                                    "Commande réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        }

        private void ListPlats_Click(object sender, EventArgs e)
        {
            // Vérifier si l'utilisateur a cliqué sur la colonne temps de livraison
            if (listPlats.SelectedItems.Count > 0)
            {
                var mousePos = listPlats.PointToClient(MousePosition);
                var hitTest = listPlats.HitTest(mousePos);

                // Si le clic est sur un sous-élément (colonne) et c'est la colonne de temps
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

            // Ajouter cette partie manquante pour afficher les détails
            string platNom = listPlats.SelectedItems[0].Text;
            AfficherDetailsPlat(platNom);
        }
    }
}