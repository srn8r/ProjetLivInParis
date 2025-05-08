using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Project_LivInParis;

namespace ProjetLivInParis
{
    public partial class Form_Statistiques : Form
    {
        private Label lblTitre;
        private ListBox lstStats;
        private Button btnFermer;

        public Form_Statistiques()
        {
            InitializeComponent();
            this.Text = "Statistiques";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MistyRose;

            lblTitre = new Label
            {
                Text = "Statistiques",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitre);

            lstStats = new ListBox
            {
                Location = new Point(20, 70),
                Size = new Size(640, 400),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(lstStats);

            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 40),
                Location = new Point(540, 490),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnFermer.Click += (s, e) => this.Close();
            this.Controls.Add(btnFermer);

            ChargerStatistiques();
        }
        /// <summary>
        /// Charge les statistiques dans la liste.
        /// </summary>
        private void ChargerStatistiques()
        {
            var db = new InterfaceBDD();
            try
            {
                lstStats.Items.Clear();

                var stats = db.GetStatistiquesGlobales();
                lstStats.Items.Add($"Total utilisateurs: {stats.TotalUtilisateurs}");
                lstStats.Items.Add($"Total clients: {stats.TotalClients}");
                lstStats.Items.Add($"Total cuisiniers: {stats.TotalCuisiniers}");
                lstStats.Items.Add($"Total commandes: {stats.TotalCommandes}");
                lstStats.Items.Add($"Plat le plus commandé: {stats.PlatPopulaire}");
                lstStats.Items.Add(new string('-', 80));

                var clientsPlus2 = db.GetClientsAvecPlusDeDeuxCommandes();
                if (clientsPlus2.Count > 0)
                {
                    lstStats.Items.Add("Clients avec +2 commandes:");
                    foreach (var c in clientsPlus2)
                        lstStats.Items.Add($"- {c.Nom} {c.Prenom} (ID: {c.ClientID}) - {c.NbCommandes} commandes");
                }
                else
                {
                    lstStats.Items.Add("Aucun client avec +2 commandes.");
                }
                lstStats.Items.Add(new string('-', 80));

                var platsJamais = db.GetPlatsJamaisCommandes();
                if (platsJamais.Count > 0)
                {
                    lstStats.Items.Add("Plats jamais commandés:");
                    foreach (var nom in platsJamais)
                        lstStats.Items.Add($"- {nom}");
                }
                else
                {
                    lstStats.Items.Add("Tous les plats ont été commandés au moins une fois.");
                }
                lstStats.Items.Add(new string('-', 80));

                var clientsMoy = db.GetClientsSuperieursMoyenne();
                if (clientsMoy.Count > 0)
                {
                    lstStats.Items.Add("Clients au-dessus de la moyenne:");
                    foreach (var c in clientsMoy)
                        lstStats.Items.Add($"- {c.Nom} {c.Prenom} (ID: {c.ClientID})");
                }
                else
                {
                    lstStats.Items.Add("Aucun client au-dessus de la moyenne trouvée.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération des statistiques: " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
