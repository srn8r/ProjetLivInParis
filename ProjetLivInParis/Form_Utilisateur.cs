using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjetLivInParis
{
    public partial class Form_Utilisateur : Form
    {
        private Button btnInscrire;
        private Button btnConnexion;
        private Label lblTitre;

        public Form_Utilisateur()
        {
            InitializeComponent();

            this.Text = "Espace Utilisateur";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;

            lblTitre = new Label
            {
                Text = "Bienvenue utilisateur",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 300) / 2, 50)
            };
            this.Controls.Add(lblTitre);

            btnInscrire = new Button
            {
                Text = "S'inscrire",
                Size = new Size(200, 50),
                Location = new Point((this.ClientSize.Width - 200) / 2, 150),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnInscrire.Click += (s, e) => new Form2().ShowDialog();
            this.Controls.Add(btnInscrire);

            btnConnexion = new Button
            {
                Text = "Se connecter",
                Size = new Size(200, 50),
                Location = new Point((this.ClientSize.Width - 200) / 2, 220),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnConnexion.Click += BtnConnexion_Click;
            this.Controls.Add(btnConnexion);
        }

        private void BtnConnexion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fonctionnalité à venir !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
