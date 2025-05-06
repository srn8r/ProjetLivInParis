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
        private Button btnVoirUtilisateurs, btnAfficherGraphe, btnVoirCommandes, btnColoration;

        #endregion

        #region constructeur
        public Form_Admin()
        {
            InitializeComponent();

            this.Text = "Espace Administrateur";
            this.Size = new Size(600, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;
            this.Resize += FormAdmin_Resize;

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
                Location = new Point((this.ClientSize.Width - 220) / 2, 140),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnVoirUtilisateurs.Click += (s, e) => new Form_AfficherUtilisateurs().ShowDialog();
            this.Controls.Add(btnVoirUtilisateurs);

            // Bouton Voir Commandes
            btnVoirCommandes = new Button
            {
                Text = "Voir les commandes",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 210),
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

            // Bouton Afficher le Graphe
            btnAfficherGraphe = new Button
            {
                Text = "Afficher le graphe",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 280),
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

            // ✅ NOUVEAU : Bouton Coloration du Graphe
            btnColoration = new Button
            {
                Text = "Coloration du graphe",
                Size = new Size(220, 50),
                Location = new Point((this.ClientSize.Width - 220) / 2, 350),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnColoration.Click += (s, e) =>
            {
                // 👉 Ouvre la future form qui montrera la coloration (à créer)
                var graphe = new GrapheCommandes();
                graphe.CreerDepuisBDD();
                graphe.AppliquerColorationWelshPowell();

                var colorationForm = new Form_ColorationGraphe(graphe);
                colorationForm.ShowDialog();
            };
            this.Controls.Add(btnColoration);
        }
        #endregion

        #region resize
        private void FormAdmin_Resize(object sender, EventArgs e)
        {
            lblTitre.Location = new Point((this.ClientSize.Width - lblTitre.Width) / 2, 50);
            btnVoirUtilisateurs.Location = new Point((this.ClientSize.Width - btnVoirUtilisateurs.Width) / 2, 140);
            btnVoirCommandes.Location = new Point((this.ClientSize.Width - btnVoirCommandes.Width) / 2, 210);
            btnAfficherGraphe.Location = new Point((this.ClientSize.Width - btnAfficherGraphe.Width) / 2, 280);
            btnColoration.Location = new Point((this.ClientSize.Width - btnColoration.Width) / 2, 350);
        }
        #endregion
    }
}