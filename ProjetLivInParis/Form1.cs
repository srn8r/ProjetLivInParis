using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Project_LivInParis;
using static System.Collections.Specialized.BitVector32;

namespace TestLinInParis
{
    public partial class Form1 : Form
    {
        private StationManager stationManager;
        private Graphe<Station> graphe;
        private Station selectedStation = null;

        private double minLat = double.MaxValue, maxLat = double.MinValue;
        private double minLon = double.MaxValue, maxLon = double.MinValue;
        private float zoom = 1.0f;
        private PointF offset = new PointF(0, 0);
        private bool isDragging = false;
        private Point lastMousePos;

        private Dictionary<string, Color> couleursParLigne = new Dictionary<string, Color>()
        {
            { "1", Color.Gold },
            { "2", Color.Blue },
            { "3", Color.Brown },
            { "4", Color.Purple },
            { "5", Color.Orange },
            { "6", Color.LightGreen },
            { "7", Color.Pink },
            { "8", Color.Gray },
            { "9", Color.Olive },
            { "10", Color.DarkGoldenrod },
            { "11", Color.DarkOrange },
            { "12", Color.SeaGreen },
            { "13", Color.Teal },
            { "14", Color.DarkViolet },
        };

        private TextBox searchBox;
        private Button searchButton;
        private Panel legendPanel;

        public Form1()
        {
            InitializeComponent();
            //InitUI();

            this.Load += Form1_Load;
            panel1.Paint += panel1_Paint;
            panel1.MouseWheel += panel1_MouseWheel;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            panel1.MouseUp += panel1_MouseUp;
            panel1.DoubleClick += panel1_DoubleClick;
            panel1.MouseClick += panel1_MouseClick;
            panel1.MouseEnter += (s, e) => panel1.Focus();
        }

        /*
        private void InitUI()
        {
            searchBox = new TextBox { Location = new Point(10, 10), Width = 200 };
            searchButton = new Button { Text = "Rechercher", Location = new Point(220, 8) };
            searchButton.Click += SearchButton_Click;

            legendPanel = new Panel { Location = new Point(10, panel1.Height - 130), Size = new Size(180, 300), AutoScroll = true };
            legendPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            legendPanel.BackColor = Color.White;
            legendPanel.BorderStyle = BorderStyle.FixedSingle;

            int y = 10;
            foreach (var pair in couleursParLigne)
            {
                Label colorBox = new Label
                {
                    BackColor = pair.Value,
                    Size = new Size(20, 20),
                    Location = new Point(10, y)
                };
                Label text = new Label
                {
                    Text = $"Ligne {pair.Key}",
                    Location = new Point(35, y + 2),
                    AutoSize = true
                };
                //legendPanel.Controls.Add(colorBox);
                //legendPanel.Controls.Add(text);
                y += 25;
            }

            panel1.Controls.Add(searchBox);
            panel1.Controls.Add(searchButton);

        }*/

        private void Form1_Load(object sender, EventArgs e)
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (graphe == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font font = new Font(this.Font.FontFamily, 9);

            foreach (var lien in graphe.Liens)
            {
                string ligne = lien.Source.Valeur.Ligne.ToString();
                if (!couleursParLigne.TryGetValue(ligne, out Color couleur))
                    couleur = Color.DarkGray;

                using (Pen penLigne = new Pen(couleur, 3))
                {
                    Point p1 = ConvertirCoordonnees(lien.Source.Valeur.Longitude, lien.Source.Valeur.Latitude);
                    Point p2 = ConvertirCoordonnees(lien.Destination.Valeur.Longitude, lien.Destination.Valeur.Latitude);
                    g.DrawLine(penLigne, p1, p2);
                }
            }

            foreach (var noeud in graphe.Noeuds)
            {
                var station = noeud.Valeur;
                Point pos = ConvertirCoordonnees(station.Longitude, station.Latitude);

                Brush fill = station == selectedStation ? Brushes.Red : Brushes.White;

                g.FillEllipse(fill, pos.X - 5, pos.Y - 5, 10, 10);
                g.DrawEllipse(Pens.Black, pos.X - 5, pos.Y - 5, 10, 10);
                if (station == selectedStation)
                {
                    g.DrawString(station.Nom, font, Brushes.Black, pos.X + 5, pos.Y - 15);
                }
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

        /*
        private void SearchButton_Click(object sender, EventArgs e)
        {
            string texte = searchBox.Text.Trim().ToLower();
            selectedStation = graphe.Noeuds.Select(n => n.Valeur)
                .FirstOrDefault(s => s.Nom.ToLower().Contains(texte));
            panel1.Invalidate();
            searchBox.PlaceholderText = "Rechercher une station...";

        }*/

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var noeud in graphe.Noeuds)
            {
                var station = noeud.Valeur;
                Point pos = ConvertirCoordonnees(station.Longitude, station.Latitude);
                if (Math.Abs(e.X - pos.X) <= 6 && Math.Abs(e.Y - pos.Y) <= 6)
                {
                    selectedStation = station;
                    panel1.Invalidate();
                    break;
                }
            }
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
    }
}
