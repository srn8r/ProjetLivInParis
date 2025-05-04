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
    public partial class Form_ChoisirStation : Form
    {
        public string StationChoisie { get; private set; }
        private TextBox txtRecherche;
        private ListBox lstStations;
        private List<string> toutesStations;

        public Form_ChoisirStation(List<string> stations)
        {
            this.Text = "Choisissez la station de métroqui est proche de votre adresse";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            toutesStations = stations;

            Label lbl = new Label
            {
                Text = "Rechercher une station :",
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };
            this.Controls.Add(lbl);

            txtRecherche = new TextBox
            {
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10)
            };
            txtRecherche.TextChanged += TxtRecherche_TextChanged;
            this.Controls.Add(txtRecherche);

            lstStations = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            lstStations.DoubleClick += (s, e) => ValiderChoix();
            this.Controls.Add(lstStations);

            Button btnOK = new Button
            {
                Text = "Valider",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.Green,
                ForeColor = Color.White
            };
            btnOK.Click += (s, e) => ValiderChoix();
            this.Controls.Add(btnOK);

            // Remplir la liste complète
            RafraichirListe(stations);
        }

        private void TxtRecherche_TextChanged(object sender, EventArgs e)
        {
            string recherche = txtRecherche.Text.ToLower();
            var filtres = toutesStations.Where(s => s.ToLower().Contains(recherche)).ToList();
            RafraichirListe(filtres);
        }

        private void RafraichirListe(List<string> stations)
        {
            lstStations.Items.Clear();
            foreach (var s in stations)
                lstStations.Items.Add(s);
        }

        private void ValiderChoix()
        {
            if (lstStations.SelectedItem != null)
            {
                StationChoisie = lstStations.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une station.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}