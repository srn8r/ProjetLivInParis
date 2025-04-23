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
    public partial class FormAdmin : Form
    {
        private Label lblTitre;
        private Label lblSousTitre;
        private Button btnVoirUtilisateurs, btnAfficherGraphe;

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

        public FormAdmin()
        {
            InitializeComponent();

            this.Text = "Espace Administrateur";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;

            // Titre
            lblTitre = new Label
            {
                Text = "Espace Administrateur",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 300) / 2, 50)
            };
            this.Controls.Add(lblTitre);

            // Bouton Voir Utilisateurs
            btnVoirUtilisateurs = new Button
            {
                Text = "Voir les utilisateurs",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 150),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnVoirUtilisateurs.Click += (s, e) => new Form3().ShowDialog();
            this.Controls.Add(btnVoirUtilisateurs);

            // Bouton Afficher le Graphe
            btnAfficherGraphe = new Button
            {
                Text = "Afficher le graphe",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 220),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnAfficherGraphe.Click += (s, e) =>
            {
                FormGraphe grapheForm = new FormGraphe(); // à créer en extrayant la partie graphe de Form1
                grapheForm.ShowDialog();
            };
            this.Controls.Add(btnAfficherGraphe);
        }
    }
}