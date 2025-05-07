using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Project_LivInParis;

namespace ProjetLivInParis
{
    public partial class Form_AfficherCommandes : Form
    {
        #region propriétés
        private ListView lvCommandes;
        private Button btnFermer;
        private Button btnSupprimer;
        private Button btnEffacerTout;
        private Label lblTitre;
        #endregion

        #region constructeur
        public Form_AfficherCommandes()
        {
            InitializeComponent();
            this.Text = "Gestion des Commandes";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Lavender;

            InitialiserComposants();
            ChargerCommandes();
        }
        #endregion

        #region structure
        private void InitialiserComposants()
        {
            // Titre
            lblTitre = new Label
            {
                Text = "Toutes les commandes",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitre);

            // ListView
            lvCommandes = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(20, 60),
                Size = new Size(840, 400)
            };

            // Colonnes
            lvCommandes.Columns.Add("Commande", 50);
            lvCommandes.Columns.Add("Date", 140);
            lvCommandes.Columns.Add("Nom du plat", 200);
            lvCommandes.Columns.Add("Adresse Livraison", 180);
            lvCommandes.Columns.Add("Prix Total (€)", 100);
            lvCommandes.Columns.Add("Statut", 120);
            lvCommandes.Columns.Add("Client", 180);
            lvCommandes.Columns.Add("Mode Paiement", 120);

            this.Controls.Add(lvCommandes);

            // Bouton Supprimer
            btnSupprimer = new Button
            {
                Text = "Supprimer",
                Size = new Size(200, 40),
                Location = new Point(20, 480),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSupprimer.Click += BtnSupprimer_Click;
            this.Controls.Add(btnSupprimer);

            // Bouton Effacer Tout
            btnEffacerTout = new Button
            {
                Text = "Effacer Tout",
                Size = new Size(250, 40),
                Location = new Point(240, 480),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnEffacerTout.Click += BtnEffacerTout_Click;
            this.Controls.Add(btnEffacerTout);

            // Bouton Fermer
            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 40),
                Location = new Point(740, 480),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnFermer.Click += (s, e) => this.Close();
            this.Controls.Add(btnFermer);
        }
        #endregion

        #region méthodes
        private void ChargerCommandes()
        {
            try
            {
                var db = new InterfaceBDD();
                db.Connexion();

                var commandes = db.RecupererToutesLesCommandes();

                lvCommandes.Items.Clear();

                foreach (var cmd in commandes)
                {
                    ListViewItem item = new ListViewItem(cmd.CommandeID.ToString());
                    item.SubItems.Add(cmd.DateCommande.ToString("g"));
                    item.SubItems.Add(cmd.NomPlat);
                    item.SubItems.Add(cmd.AdresseLivraison);
                    item.SubItems.Add(cmd.PrixTotal.ToString("0.00"));
                    item.SubItems.Add(cmd.Statut);
                    item.SubItems.Add($"{cmd.ClientNom} {cmd.ClientPrenom}");
                    item.SubItems.Add(cmd.ModePaiement);


                    lvCommandes.Items.Add(item);
                }

                db.FermerConnexion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des commandes : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (lvCommandes.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner une commande à supprimer.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int commandeId = int.Parse(lvCommandes.SelectedItems[0].SubItems[0].Text);

            DialogResult result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer la commande #{commandeId} ?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var db = new InterfaceBDD();
                    db.Connexion();

                    bool success = db.SupprimerCommande(commandeId);

                    db.FermerConnexion();

                    if (success)
                    {
                        MessageBox.Show($"La commande #{commandeId} a été supprimée.",
                            "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ChargerCommandes();
                    }
                    else
                    {
                        MessageBox.Show("La suppression a échoué.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de la suppression : " + ex.Message,
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void BtnEffacerTout_Click(object sender, EventArgs e)
        {
            if (lvCommandes.Items.Count == 0)
            {
                MessageBox.Show("Il n'y a aucune commande à effacer.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "ATTENTION : Cette action va supprimer TOUTES les commandes de la base de données.\n\n" +
                "Cette opération est irréversible. Voulez-vous vraiment continuer ?",
                "⚠️ Confirmation de suppression massive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DialogResult secondConfirmation = MessageBox.Show(
                    "Dernière chance : Confirmez-vous la suppression de TOUTES les commandes ?",
                    "⚠️ Confirmation finale",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (secondConfirmation == DialogResult.Yes)
                {
                    try
                    {
                        var db = new InterfaceBDD();
                        db.Connexion();

                        bool success = db.SupprimerToutesLesCommandes();

                        db.FermerConnexion();

                        if (success)
                        {
                            MessageBox.Show("Toutes les commandes ont été supprimées.",
                                "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ChargerCommandes(); 
                        }
                        else
                        {
                            MessageBox.Show("La suppression a échoué.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors de la suppression : " + ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion
    }
}
