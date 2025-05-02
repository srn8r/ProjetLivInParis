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
    public partial class Form_Choix : Form
    {
        private int userId;
        public Form_Choix(int id)
        {
            InitializeComponent();
            this.Text = "Choisir un mode";
            this.Size = new Size(350, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            userId = id;

            Button btnClient = new Button
            {
                Text = "Accès Client",
                Top = 30,
                Width = 150,
                Height = 40,
                Left = 40
            };
            btnClient.Click += (s, e) => new Form_Client(userId).ShowDialog();

            Button btnCuisinier = new Button
            {
                Text = "Accès Cuisinier",
                Top = 80,
                Width = 150,
                Height = 40,
                Left = 40
            };
            btnCuisinier.Click += (s, e) => new Form_Cuisinier(userId).ShowDialog();

            this.Controls.Add(btnClient);
            this.Controls.Add(btnCuisinier);
        }
    }
}
