using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Project_LivInParis;
using ProjetLivInParis;


namespace TestLinInParis
{
    public partial class FormAccueil : Form
    {
        private Label lblTitre;
        private Label lblSousTitre;
        private Button btnAdmin;
        private Button btnUtilisateur;

        public FormAccueil()
        {
            InitializeComponent();

            this.Text = "LivInParis";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.DarkMagenta;

            // Titre
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

            // Sous-titre
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

            // Bouton Admin
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
            btnAdmin.Click += (s, e) => new FormAdmin().ShowDialog();
            this.Controls.Add(btnAdmin);
            btnAdmin.BringToFront();

            // Bouton Utilisateur
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
            btnUtilisateur.Click += (s, e) => new FormUtilisateur().ShowDialog();
            this.Controls.Add(btnUtilisateur);
            btnUtilisateur.BringToFront();
        }
    }
}
