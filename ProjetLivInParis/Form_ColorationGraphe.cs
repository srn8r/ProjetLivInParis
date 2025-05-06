using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjetLivInParis
{
    public partial class Form_ColorationGraphe : Form
    {
        private GrapheCommandes graphe;
        private Dictionary<int, Color> paletteCouleurs = new();

        public Form_ColorationGraphe(GrapheCommandes g)
        {
            InitializeComponent();
            this.graphe = g;
            this.Text = "Coloration de Graphe - Welsh-Powell";
            this.Size = new Size(900, 700);
            this.DoubleBuffered = true;

            // Génère des couleurs aléatoires pour chaque couleur utilisée
            var couleurs = new[]
            {
            Color.Red, Color.Green, Color.Blue, Color.Orange,
            Color.Purple, Color.Brown, Color.Cyan, Color.Magenta,
            Color.Teal, Color.Olive
        };

            for (int i = 0; i < couleurs.Length; i++)
                paletteCouleurs[i + 1] = couleurs[i];

            this.Paint += Form_ColorationGraph_Paint;
        }

        private void Form_ColorationGraph_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int rayon = 25;
            int nbSommets = graphe.Sommets.Count;
            int rayonCercle = 250;
            Point centre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);

            // Positionner les sommets en cercle
            Dictionary<Sommet, PointF> positions = new();
            for (int i = 0; i < nbSommets; i++)
            {
                double angle = 2 * Math.PI * i / nbSommets;
                float x = centre.X + (float)(rayonCercle * Math.Cos(angle));
                float y = centre.Y + (float)(rayonCercle * Math.Sin(angle));
                positions[graphe.Sommets[i]] = new PointF(x, y);
            }

            // 1️⃣ Dessiner les arêtes
            foreach (var arete in graphe.Aretes)
            {
                var p1 = positions[arete.Source];
                var p2 = positions[arete.Cible];
                g.DrawLine(Pens.Gray, p1, p2);
            }

            // 2️⃣ Dessiner les sommets
            foreach (var sommet in graphe.Sommets)
            {
                var pos = positions[sommet];
                var rect = new RectangleF(pos.X - rayon, pos.Y - rayon, rayon * 2, rayon * 2);
                var couleur = paletteCouleurs.GetValueOrDefault(sommet.Couleur, Color.LightGray);

                using (SolidBrush brush = new SolidBrush(couleur))
                    g.FillEllipse(brush, rect);

                g.DrawEllipse(Pens.Black, rect);

                var textSize = g.MeasureString(sommet.Nom, this.Font);
                g.DrawString(sommet.Nom, this.Font, Brushes.Black,
                    pos.X - textSize.Width / 2, pos.Y - textSize.Height / 2);
            }
        }
    }
}
