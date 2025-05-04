using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Project_LivInParis;

namespace ProjetLivInParis
{
    public partial class Form_Graphe : Form
    {
        #region propriétés

        private Panel panel1;
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

        private Dictionary<string, Color> couleursParLigne = new Dictionary<string, Color>(){
            { "1", Color.Gold },
            { "2", Color.Blue },
            { "3", Color.Brown },
            { "3bis", Color.LightSkyBlue }, // Ajouté : 3bis
            { "4", Color.Purple },
            { "5", Color.Orange },
            { "6", Color.LightGreen },
            { "7", Color.Pink },
            { "7bis", Color.LightPink }, // Ajouté : 7bis
            { "8", Color.Gray },
            { "9", Color.Olive },
            { "10", Color.DarkGoldenrod },
            { "11", Color.DarkOrange },
            { "12", Color.SeaGreen },
            { "13", Color.Teal },
            { "14", Color.DarkViolet },
        };

        #endregion

        #region constructeur
        public Form_Graphe()
        {
            InitializeComponent();
            this.Text = "Visualisation du graphe";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            panel1 = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            Panel controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };

            algoSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(controlPanel.Width - 220, 15)
            };
            algoSelector.Items.AddRange(new[] { "Dijkstra", "Bellman-Ford", "Floyd-Warshall" });
            algoSelector.SelectedIndex = 0;

            searchBox = new TextBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(algoSelector.Left - 370, 15),
                Padding = new Padding(5),
                PlaceholderText = "Rechercher une station..."
            };

            searchButton = new Button
            {
                Text = "Rechercher",
                Width = 100,
                Height = searchBox.Height + 2,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(searchBox.Right + 10, searchBox.Top)
            };
            searchButton.Click += SearchButton_Click;

            controlPanel.Controls.Add(searchBox);
            controlPanel.Controls.Add(searchButton);
            controlPanel.Controls.Add(algoSelector);

            this.Controls.Add(panel1);
            this.Controls.Add(controlPanel);

            panel1.Paint += panel1_Paint;
            panel1.MouseWheel += panel1_MouseWheel;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            panel1.MouseUp += panel1_MouseUp;
            panel1.DoubleClick += panel1_DoubleClick;
            panel1.MouseClick += panel1_MouseClick;
            panel1.MouseEnter += (s, e) => panel1.Focus();

            this.Load += FormGraphe_Load;
            this.Resize += FormGraphe_Resize;

            this.Resize += (s, e) => {
                algoSelector.Location = new Point(controlPanel.Width - algoSelector.Width - 20, 15);
                searchButton.Location = new Point(algoSelector.Left - searchButton.Width - 20, 15);
                searchBox.Location = new Point(searchButton.Left - searchBox.Width - 10, 15);
            };
        }
        #endregion

        #region format idéal
        private void FormGraphe_Resize(object sender, EventArgs e)
        {
            int spacing = 20;
            int top = spacing;
            int rightMargin = spacing;

            searchBox.Location = new Point(this.ClientSize.Width - searchBox.Width - searchButton.Width - spacing * 2, top);
            searchButton.Location = new Point(searchBox.Right + spacing, top);
            algoSelector.Location = new Point(searchBox.Left, searchBox.Bottom + spacing);
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
        #endregion

        #region souris
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
        #endregion

        #region import
        private void FormGraphe_Load(object sender, EventArgs e)
        {
            string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
            stationManager = new StationManager(connectionString);
            stationManager.ChargerStationsDepuisBDD();
            stationManager.LierCorrespondances();
            graphe = stationManager.ObtenirGraphe();

            foreach (var noeud in graphe.Noeuds)
            {
                var station = noeud.Valeur;
                minLat = Math.Min(minLat, station.Latitude);
                maxLat = Math.Max(maxLat, station.Latitude);
                minLon = Math.Min(minLon, station.Longitude);
                maxLon = Math.Max(maxLon, station.Longitude);
            }

            CentrerGraphe();
            panel1.Invalidate();
        }
        #endregion

        #region tracé graphe
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
        private List<string> ExtraireTrajetParLigne(List<Noeud<Station>> chemin)
        {
            var lignesDetaillees = new List<string>();

            if (chemin == null || chemin.Count < 2)
                return lignesDetaillees;

            string ligneActuelle = chemin[0].Valeur.Ligne;
            string depart = chemin[0].Valeur.Nom;

            for (int i = 1; i < chemin.Count; i++)
            {
                var courant = chemin[i - 1];
                var suivant = chemin[i];

                // Si on change de ligne OU si on arrive au dernier arrêt
                bool ligneChange = suivant.Valeur.Ligne != ligneActuelle;
                bool dernierSegment = i == chemin.Count - 1;

                if (ligneChange || dernierSegment)
                {
                    string destination = suivant.Valeur.Nom;

                    // ⚠️ Évite de créer des segments où départ == arrivée (cas des doubles)
                    if (depart != destination)
                    {
                        lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} -> {destination}");
                    }

                    depart = suivant.Valeur.Nom;
                    ligneActuelle = suivant.Valeur.Ligne;
                }
            }

            lignesDetaillees.Add($"Algorithme utilisé : {algoSelector.SelectedItem?.ToString() ?? "?"}");

            return lignesDetaillees;
        }

        #endregion 

        #region trajet entre 2 stations
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
        #endregion 

        #region recherche
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
        #endregion
    }
}
