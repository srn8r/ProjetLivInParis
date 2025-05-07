using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Project_LivInParis;
using ProjetLivInParis;


namespace TestLinInParis
{
    public partial class Form_Accueil : Form
    {
        #region propriétés

        private Label lblTitre;
        private Label lblSousTitre;
        private Button btnAdmin;
        private Button btnUtilisateur;
        #endregion

        #region constructeur
        /// <summary>
        /// Constructeur de la classe Form_Accueil.
        /// </summary>
        public Form_Accueil()
        {
            InitializeComponent();

            this.Text = "LivInParis";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.DarkMagenta;
            this.Resize += FormAccueil_Resize;

            lblTitre = new Label
            {
                Text = "LivInParis",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 200) / 2, 100)
            };
            this.Controls.Add(lblTitre);
            lblTitre.BringToFront();

            lblSousTitre = new Label
            {
                Text = "Livraison de vos commandes dans Paris",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 400) / 2, 160)
            };
            this.Controls.Add(lblSousTitre);
            lblSousTitre.BringToFront();

            btnAdmin = new Button
            {
                Text = "Admin",
                Size = new Size(200, 50),
                Location = new Point((this.ClientSize.Width - 200) / 2, 250),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand

            };
            btnAdmin.Click += (s, e) => new Form_Admin().ShowDialog();
            this.Controls.Add(btnAdmin);
            btnAdmin.BringToFront();

            btnUtilisateur = new Button
            {
                Text = "Utilisateur",
                Size = new Size(200, 50),
                Location = new Point((this.ClientSize.Width - 200) / 2, 320),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnUtilisateur.Click += (s, e) => new Form_Utilisateur().ShowDialog();
            this.Controls.Add(btnUtilisateur);
            btnUtilisateur.BringToFront();
        }
        #endregion

        #region resize
        /// <summary>
        /// Gère l'événement Resize du formulaire.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAccueil_Resize(object sender, EventArgs e)
        {
            lblTitre.Location = new Point((this.ClientSize.Width - lblTitre.Width) / 2, 100);
            lblSousTitre.Location = new Point((this.ClientSize.Width - lblSousTitre.Width) / 2, 160);
            btnAdmin.Location = new Point((this.ClientSize.Width - btnAdmin.Width) / 2, 250);
            btnUtilisateur.Location = new Point((this.ClientSize.Width - btnUtilisateur.Width) / 2, 320);
        }
        #endregion
    }
}
