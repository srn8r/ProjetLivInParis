using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjetLivInParis
{
    public partial class Form_Choix : Form
    {
        private int userId;
        public Form_Choix(int id)
        {
            InitializeComponent();
            this.Text = "Choisir un mode";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;
            userId = id;

            Label lblTitre = new Label
            {
                Text = "Choisissez votre espace",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 30)
            };
            this.Controls.Add(lblTitre);

            Button btnClient = new Button
            {
                Text = "Accès Client",
                Size = new Size(200, 50),
                Location = new Point((this.ClientSize.Width - 200) / 2, 100),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClient.Click += (s, e) => new Form_Client(userId).ShowDialog();

            Button btnCuisinier = new Button
            {
                Text = "Accès Cuisinier",
                Size = new Size(200, 50),
                Location = new Point((this.ClientSize.Width - 200) / 2, 170),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCuisinier.Click += (s, e) => new Form_Cuisinier(userId).ShowDialog();

            this.Controls.Add(btnClient);
            this.Controls.Add(btnCuisinier);
        }
    }
}
