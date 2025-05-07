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
    public partial class Form_Admin : Form
    {
        #region propriétés

        private Label lblTitre;
        private Button btnVoirUtilisateurs, btnAfficherGraphe, btnVoirCommandes, btnColoration, btnStatistiques;

        #endregion

        #region constructeur
        /// <summary>
        /// Constructeur de la classe Form_Admin.
        /// </summary>
        public Form_Admin()
        {
            InitializeComponent();

            this.Text = "Espace Administrateur";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;
            this.Resize += FormAdmin_Resize;

            lblTitre = new Label
            {
                Text = "Espace Administrateur",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(200 / 2, 40)
            };
            this.Controls.Add(lblTitre);

            btnVoirUtilisateurs = new Button
            {
                Text = "Voir les utilisateurs",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 120),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnVoirUtilisateurs.Click += (s, e) => new Form_AfficherUtilisateurs().ShowDialog();
            this.Controls.Add(btnVoirUtilisateurs);

            btnVoirCommandes = new Button
            {
                Text = "Voir les commandes",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 190),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnVoirCommandes.Click += (s, e) =>
            {
                Form_AfficherCommandes formCommandes = new Form_AfficherCommandes();
                formCommandes.ShowDialog();
            };
            this.Controls.Add(btnVoirCommandes);

            btnAfficherGraphe = new Button
            {
                Text = "Afficher le graphe",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 260),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnAfficherGraphe.Click += (s, e) =>
            {
                Form_Graphe grapheForm = new Form_Graphe();
                grapheForm.ShowDialog();
            };
            this.Controls.Add(btnAfficherGraphe);

            btnColoration = new Button
            {
                Text = "Coloration du graphe",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 330),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnColoration.Click += (s, e) =>
            {
                var graphe = new GrapheCommandes();
                graphe.CreerDepuisBDD();
                graphe.AppliquerColorationWelshPowell();

                var colorationForm = new Form_ColorationGraphe(graphe);
                colorationForm.ShowDialog();
            };
            this.Controls.Add(btnColoration);

            btnStatistiques = new Button
            {
                Text = "Voir les statistiques",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 400),
                BackColor = Color.Teal,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnStatistiques.Click += (s, e) =>
            {
                Form_Statistiques statsForm = new Form_Statistiques();
                statsForm.ShowDialog();
            };
            this.Controls.Add(btnStatistiques);
        }
        #endregion

        #region resize
        /// <summary>
        /// Redimensionne les composants de la fenêtre lorsque la taille de la fenêtre change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAdmin_Resize(object sender, EventArgs e)
        {
            lblTitre.Location = new Point((this.ClientSize.Width - lblTitre.Width) / 2, 40);
            btnVoirUtilisateurs.Location = new Point((this.ClientSize.Width - btnVoirUtilisateurs.Width) / 2, 120);
            btnVoirCommandes.Location = new Point((this.ClientSize.Width - btnVoirCommandes.Width) / 2, 190);
            btnAfficherGraphe.Location = new Point((this.ClientSize.Width - btnAfficherGraphe.Width) / 2, 260);
            btnColoration.Location = new Point((this.ClientSize.Width - btnColoration.Width) / 2, 330);
            btnStatistiques.Location = new Point((this.ClientSize.Width - btnStatistiques.Width) / 2, 400);
        }
        #endregion
    }
}
