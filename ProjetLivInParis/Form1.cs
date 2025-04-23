using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Project_LivInParis;
using ProjetLivInParis;


namespace TestLinInParis
{
    public partial class Form1 : Form
    {
        private Label lblTitre;
        private Label lblSousTitre;
        private Button btnInscrire, btnVoirUtilisateurs, btnCommander, btnAfficherGraphe;

        private StationManager stationManager;
        private Graphe<Station> graphe;
        private Station selectedStation1 = null;
        private Station selectedStation2 = null;
        private List<Noeud<Station>> chemin = new();

        private double minLat = double.MaxValue, maxLat = double.MinValue;
        private double minLon = double.MaxValue, maxLon = double.MinValue;
        private float zoom = 1.0f;
        private PointF offset = new PointF(0, 0);
        private bool isDragging = false;
        private Point lastMousePos;
        private ComboBox algoSelector;
        private TextBox searchBox;
        private Button searchButton;



        private Dictionary<string, Color> couleursParLigne = new Dictionary<string, Color>()
        {
            { "1", Color.Gold }, { "2", Color.Blue }, { "3", Color.Brown }, { "4", Color.Purple },
            { "5", Color.Orange }, { "6", Color.LightGreen }, { "7", Color.Pink }, { "8", Color.Gray },
            { "9", Color.Olive }, { "10", Color.DarkGoldenrod }, { "11", Color.DarkOrange },
            { "12", Color.SeaGreen }, { "13", Color.Teal }, { "14", Color.DarkViolet },
        };

        public Form1()
        {
            InitializeComponent();
            this.Text = "LivInParis";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.DarkMagenta;
            this.Resize += Form1_Resize;


            /* Panel pour le graphe */
            panel1 = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };
            this.Controls.Add(panel1);
            

            /* Titre */
            lblTitre = new Label
            {
                Text = "LivInParis",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
            };
            this.Controls.Add(lblTitre);
            lblTitre.BringToFront();

            /* Sous-titre */
            lblSousTitre = new Label
            {
                Text = "Livraison de vos commandes dans Paris",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.White,
            };
            this.Controls.Add(lblSousTitre);
            lblSousTitre.BringToFront();

            /* Boutons */
            btnInscrire = new Button
            {
                Text = "S'inscrire",
                Size = new Size(200, 40),
                Cursor = Cursors.Hand,
                BackColor = Color.Purple,
                ForeColor = Color.White
            };
            btnInscrire.Click += BtnOuvrirForm2_Click;
            btnInscrire.BringToFront();

            btnVoirUtilisateurs = new Button 
            { 
                Text = "Voir les utilisateurs", 
                Size = new Size(200, 40) ,
                Cursor = Cursors.Hand,
                BackColor = Color.Purple,
                ForeColor = Color.White
            };
            btnVoirUtilisateurs.Click += BtnVoirUtilisateurs_Click;
            btnVoirUtilisateurs.BringToFront();

            btnCommander = new Button 
            { 
                Text = "Passer une commande", 
                Size = new Size(200, 40),
                Enabled = false, // désactive le clic
                ForeColor = Color.Gray,
                BackColor = Color.LightGray
            };
            btnCommander.BringToFront();

            btnAfficherGraphe = new Button 
            { 
                Text = "Afficher le graphe", 
                Size = new Size(200, 40),
                Cursor = Cursors.Hand,
                BackColor = Color.Purple,
                ForeColor = Color.White
            };
            btnAfficherGraphe.Click += BtnAfficherGraphe_Click;
            btnAfficherGraphe.BringToFront();

            algoSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200,
                Visible = false,
            };
            algoSelector.Items.AddRange(new[] { "Dijkstra", "Bellman-Ford", "Floyd-Warshall" });
            algoSelector.SelectedIndex = 0;
            this.Controls.Add(algoSelector);

            searchBox = new TextBox
            {
                Width = 200,
                Location = new Point(10, 300),
                Visible = false

            };
            this.Controls.Add(searchBox);

            searchButton = new Button
            {
                Text = "Rechercher",
                Width = 100,
                Height = 40,
                Location = new Point(10, 350),
                Visible = false,
                ForeColor = Color.White

            };
            searchButton.Click += SearchButton_Click;
            this.Controls.Add(searchButton);

            // Ajouter les boutons principaux à la form
            this.Controls.AddRange(new Control[] { btnInscrire, btnVoirUtilisateurs, btnCommander, btnAfficherGraphe });

            // Attacher les événements du graphe
            panel1.Paint += panel1_Paint;
            panel1.MouseWheel += panel1_MouseWheel;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            panel1.MouseUp += panel1_MouseUp;
            panel1.DoubleClick += panel1_DoubleClick;
            panel1.MouseClick += panel1_MouseClick;
            panel1.MouseEnter += (s, e) => panel1.Focus();


            this.Load += Form1_Load;

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
            stationManager = new StationManager(connectionString);
            stationManager.ChargerStationsDepuisBDD();
            stationManager.LierCorrespondances();
            graphe = stationManager.ObtenirGraphe();
            Form1_Resize(null, null);

            foreach (var noeud in graphe.Noeuds)
            {
                var station = noeud.Valeur;
                minLat = Math.Min(minLat, station.Latitude);
                maxLat = Math.Max(maxLat, station.Latitude);
                minLon = Math.Min(minLon, station.Longitude);
                maxLon = Math.Max(maxLon, station.Longitude);
            }

            panel1.Invalidate();
            btnInscrire.BringToFront();
            btnVoirUtilisateurs.BringToFront();
            btnCommander.BringToFront();
            btnAfficherGraphe.BringToFront();
        }

        private void BtnAfficherGraphe_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
            searchButton.Visible = !searchButton.Visible;
            searchBox.Visible = !searchBox.Visible;
            btnAfficherGraphe.Text = panel1.Visible ? "Cacher le graphe" : "Afficher le graphe";
            algoSelector.Visible = !algoSelector.Visible;

            this.Controls.Add(panel1);
            panel1.BringToFront();
            btnAfficherGraphe.BringToFront();
            searchBox.BringToFront();
            searchButton.BringToFront();
            algoSelector.BringToFront();

            Form1_Resize(null, null); // Repositionne à la volée

        }
        private void BtnVoirUtilisateurs_Click(object sender, EventArgs e)
        {
            Form3 fenetreUtilisateurs = new Form3();
            fenetreUtilisateurs.ShowDialog();
        }
        private void BtnOuvrirForm2_Click(object sender, EventArgs e)
        {
            Form2 fenetreAjout = new Form2();
            fenetreAjout.ShowDialog();
        }


        /* Pour le traçage du graphe */
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (graphe == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font font = new Font(this.Font.FontFamily, 9);

            // Dessin des lignes
            foreach (var lien in graphe.Liens)
            {
                string ligne = lien.Source.Valeur.Ligne.ToString();
                if (!couleursParLigne.TryGetValue(ligne, out Color couleur))
                    couleur = Color.DarkGray;

                using Pen penLigne = new Pen(couleur, 3);
                Point p1 = ConvertirCoordonnees(lien.Source.Valeur.Longitude, lien.Source.Valeur.Latitude);
                Point p2 = ConvertirCoordonnees(lien.Destination.Valeur.Longitude, lien.Destination.Valeur.Latitude);
                g.DrawLine(penLigne, p1, p2);
            }

            // Chemin le plus court
            if (chemin.Count > 1)
            {
                for (int i = 0; i < chemin.Count - 1; i++)
                {
                    var actuel = chemin[i];
                    var suivant = chemin[i + 1];

                    var ligne = actuel.Valeur.Ligne.ToString();
                    if (!couleursParLigne.TryGetValue(ligne, out Color couleur))
                        couleur = Color.Red; // couleur par défaut si ligne inconnue

                    using Pen penCheminLigne = new Pen(couleur, 8);
                    var p1 = ConvertirCoordonnees(actuel.Valeur.Longitude, actuel.Valeur.Latitude);
                    var p2 = ConvertirCoordonnees(suivant.Valeur.Longitude, suivant.Valeur.Latitude);
                    g.DrawLine(penCheminLigne, p1, p2);
                }


                double total = chemin.Zip(chemin.Skip(1), (a, b) =>
                    a.LiensSortants.FirstOrDefault(l => l.Destination == b)?.Poids ?? 0).Sum();

                g.DrawString($"Trajet : {selectedStation1.Nom} -> {selectedStation2.Nom} ({total} min)",
                    font, Brushes.Black, 10, 10);
            }

            // Dessin des stations
            foreach (var noeud in graphe.Noeuds)
            {
                var station = noeud.Valeur;
                Point pos = ConvertirCoordonnees(station.Longitude, station.Latitude);
                Brush fill = Brushes.White;
                if (station == selectedStation1 || station == selectedStation2)
                {
                    g.DrawString(station.Nom, font, Brushes.Black, pos.X + 5, pos.Y - 15);
                    fill = Brushes.Red;
                }

                g.FillEllipse(fill, pos.X - 5, pos.Y - 5, 10, 10);
                g.DrawEllipse(Pens.Black, pos.X - 5, pos.Y - 5, 10, 10);
            }
            var lignesDetaillees = ExtraireTrajetParLigne(chemin);
            float yText = 50;
            foreach (var ligne in lignesDetaillees)
            {
                g.DrawString(ligne, font, Brushes.DarkGreen, 10, yText);
                yText += 18;
            }


        }

        private Point ConvertirCoordonnees(double lon, double lat)
        {
            int padding = 40;
            double lonRange = maxLon - minLon;
            double latRange = maxLat - minLat;
            double baseZoom = Math.Min(panel1.Width / lonRange, panel1.Height / latRange);
            double effectiveZoom = baseZoom * zoom;
            double x = (lon - minLon) * effectiveZoom + padding + offset.X;
            double y = (maxLat - lat) * effectiveZoom + padding + offset.Y;
            return new Point((int)x, (int)y);
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var noeud in graphe.Noeuds)
            {
                var station = noeud.Valeur;
                Point pos = ConvertirCoordonnees(station.Longitude, station.Latitude);
                if (Math.Abs(e.X - pos.X) <= 6 && Math.Abs(e.Y - pos.Y) <= 6)
                {
                    if (selectedStation1 == null || (selectedStation1 != null && selectedStation2 != null))
                    {
                        selectedStation1 = station;
                        selectedStation2 = null;
                        chemin.Clear();
                    }
                    else
                    {
                        selectedStation2 = station;
                        var n1 = graphe.Noeuds.First(n => n.Valeur == selectedStation1);
                        var n2 = graphe.Noeuds.First(n => n.Valeur == selectedStation2);
                        if (algoSelector.SelectedItem.ToString() == "Dijkstra")
                        {
                            chemin = graphe.CheminCourt(n1, n2);
                        }
                        else if (algoSelector.SelectedItem.ToString() == "Bellman-Ford")
                        {
                            var (distances, previous) = graphe.BellmanFord(n1);
                            chemin = new List<Noeud<Station>>();
                            var current = n2;
                            while (current != null)
                            {
                                chemin.Insert(0, current);
                                current = previous[current];
                            }
                        }
                        else if (algoSelector.SelectedItem.ToString() == "Floyd-Warshall")
                        {
                            var dist = graphe.FloydWarshall(out var next);
                            var index = graphe.Noeuds;
                            var indexMap = index.Select((node, i) => (node, i)).ToDictionary(x => x.node, x => x.i);

                            chemin = new List<Noeud<Station>>();
                            int u = indexMap[n1];
                            int v = indexMap[n2];
                            if (next[u, v] == null)
                                chemin = new List<Noeud<Station>>();
                            else
                            {
                                chemin.Add(n1);
                                while (u != v)
                                {
                                    var suivant = next[u, v];
                                    chemin.Add(suivant);
                                    u = indexMap[suivant];
                                }
                            }
                        }

                    }

                    panel1.Invalidate();
                    break;
                }
            }
        }

        private void panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoom;
            zoom *= (e.Delta > 0) ? 1.1f : 0.9f;
            float dx = e.X - panel1.Width / 2;
            float dy = e.Y - panel1.Height / 2;
            offset.X -= dx * (zoom - oldZoom);
            offset.Y -= dy * (zoom - oldZoom);
            panel1.Invalidate();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePos = e.Location;
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                PointF delta = new PointF(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                offset.X += delta.X;
                offset.Y += delta.Y;
                lastMousePos = e.Location;
                panel1.Invalidate();
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            CentrerGraphe();
            panel1.Invalidate();
        }

        private void CentrerGraphe()
        {
            int padding = 40;
            double lonRange = maxLon - minLon;
            double latRange = maxLat - minLat;
            double baseZoom = Math.Min(panel1.Width / lonRange, panel1.Height / latRange);
            float totalWidth = (float)(lonRange * baseZoom);
            float totalHeight = (float)(latRange * baseZoom);
            offset = new PointF((panel1.Width - totalWidth) / 2f - padding, (panel1.Height - totalHeight) / 2f - padding);
        }
        private void SearchButton_Click(object sender, EventArgs e)
        {
            string texte = searchBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(texte)) return;

            var stationTrouvee = graphe.Noeuds
                .Select(n => n.Valeur)
                .FirstOrDefault(s => s.Nom.ToLower().Contains(texte));

            if (stationTrouvee != null)
            {
                selectedStation1 = stationTrouvee;
                selectedStation2 = null;
                chemin.Clear();
                panel1.Invalidate();
            }
            else
            {
                MessageBox.Show("Aucune station trouvée.", "Recherche", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private List<string> ExtraireTrajetParLigne(List<Noeud<Station>> chemin)
        {
            var lignesDetaillees = new List<string>();

            if (chemin == null || chemin.Count < 2) return lignesDetaillees;

            string ligneActuelle = chemin[0].Valeur.Ligne.ToString();
            string depart = chemin[0].Valeur.Nom;

            for (int i = 1; i < chemin.Count; i++)
            {
                var courant = chemin[i - 1];
                var suivant = chemin[i];

                if (suivant.Valeur.Ligne.ToString() != ligneActuelle)
                {
                    lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} -> {courant.Valeur.Nom}");
                    depart = courant.Valeur.Nom;
                    ligneActuelle = suivant.Valeur.Ligne.ToString();
                }
            }

            lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} -> {chemin.Last().Valeur.Nom}");
            lignesDetaillees.Add($"Algorithme utilisé : {algoSelector.SelectedItem.ToString()}");
            return lignesDetaillees;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int spacing = 20; 
            int buttonHeight = 40;
            int boxWidth = 200;
            int totalHeight = (buttonHeight + spacing) * 4 - spacing;
            int centerX = (this.ClientSize.Width - boxWidth) / 2;
            int startY = (this.ClientSize.Height - totalHeight) / 2;

            if (!panel1.Visible)
            {
                
                btnInscrire.Location = new Point(centerX, startY);
                btnVoirUtilisateurs.Location = new Point(centerX, startY + (buttonHeight + spacing) * 1);
                btnCommander.Location = new Point(centerX, startY + (buttonHeight + spacing) * 2);
                btnAfficherGraphe.Location = new Point(centerX, startY + (buttonHeight + spacing) * 3);

                lblTitre.Location = new Point((this.ClientSize.Width - lblTitre.Width) / 2, btnInscrire.Location.Y - 100);

                lblSousTitre.Location = new Point((this.ClientSize.Width - lblSousTitre.Width) / 2, lblTitre.Location.Y + 50);
            }
            else
            {
                btnAfficherGraphe.Location = new Point(this.ClientSize.Width - btnAfficherGraphe.Width - 50, this.ClientSize.Height - btnAfficherGraphe.Height - 50);
                CentrerGraphe();
            }

            // Toujours en haut à droite
            searchBox.Location = new Point(this.ClientSize.Width - searchBox.Width - searchButton.Width - spacing * 3, spacing);
            searchButton.Location = new Point(searchBox.Right + spacing, spacing);

            algoSelector.Location = new Point(searchBox.Left, searchBox.Bottom + spacing);
        }


    }
}
