
using System.Data;

namespace ProjetLivInParis
{
    public partial class Form_AfficherUtilisateurs : Form
    {
        #region propriétés
        private ListView lvUtilisateurs;
        private ComboBox cbFiltre;
        private Button btnFermer, btnSupprimer, btnEnregistrer;
        private Label lblTitre;
        private TextBox txtNom, txtPrenom, txtEmail, txtTel, txtMdp, txtAdresse;
        #endregion

        #region constructeur
        public Form_AfficherUtilisateurs()
        {
            InitializeComponent();
            this.Text = "Gestion des Utilisateurs";
            this.Size = new Size(1190, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;
            this.Font = new Font("Segoe UI", 10);

            InitialiserComposants();
            ChargerUtilisateurs();
        }
        #endregion

        #region structure
        private void InitialiserComposants()
        {
            lblTitre = new Label
            {
                Text = "Gestion des Utilisateurs",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitre);

            cbFiltre = new ComboBox
            {
                Items = { "Tous", "Clients", "Cuisiniers" },
                SelectedIndex = 0,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(25, 70),
                Width = 200,
                Font = new Font("Segoe UI", 10)
            };
            cbFiltre.SelectedIndexChanged += CbFiltre_SelectedIndexChanged;
            this.Controls.Add(cbFiltre);

            lvUtilisateurs = new ListView
            {
                Location = new Point(25, 110),
                Width = 1120,
                Height = 320,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 10)
            };

            lvUtilisateurs.Columns.Add("ID", 50);
            lvUtilisateurs.Columns.Add("Nom", 120);
            lvUtilisateurs.Columns.Add("Prénom", 120);
            lvUtilisateurs.Columns.Add("Email", 220);
            lvUtilisateurs.Columns.Add("Téléphone", 120);
            lvUtilisateurs.Columns.Add("Adresse", 210);
            lvUtilisateurs.Columns.Add("Type", 100);
            lvUtilisateurs.Columns.Add("Mot de Passe", 140);

            lvUtilisateurs.SelectedIndexChanged += LvUtilisateurs_SelectedIndexChanged;
            this.Controls.Add(lvUtilisateurs);

            int labelWidth = 150, tbWidth = 200, lineHeight = 35, startY = 450, spacingY = 40, startX1 = 50, startX2 = 500;

            txtNom = CreateField("Nom:", startX1, startY, labelWidth, tbWidth);
            txtPrenom = CreateField("Prénom:", startX1, startY + spacingY, labelWidth, tbWidth);
            txtEmail = CreateField("Email:", startX1, startY + 2 * spacingY, labelWidth, tbWidth);
            txtTel = CreateField("Téléphone:", startX2, startY, labelWidth, tbWidth);
            txtAdresse = CreateField("Adresse:", startX2, startY + spacingY, labelWidth, tbWidth);
            txtMdp = CreateField("Mot de Passe:", startX2, startY + 2 * spacingY, labelWidth, tbWidth);

            btnEnregistrer = new Button
            {
                Text = "Enregistrer",
                Size = new Size(160, 45),
                Location = new Point(950, startY + spacingY),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEnregistrer.Click += BtnEnregistrer_Click;
            this.Controls.Add(btnEnregistrer);

            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 40),
                Location = new Point(1020, 620),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnFermer.Click += (s, e) => this.Close();
            this.Controls.Add(btnFermer);

            btnSupprimer = new Button
            {
                Text = "Supprimer",
                Size = new Size(120, 40),
                Location = new Point(880, 620),
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSupprimer.Click += BtnSupprimer_Click;
            this.Controls.Add(btnSupprimer);
        }
        /// <summary>
        /// Crée un champ de texte avec une étiquette.
        /// </summary>
        /// <param name="labelText"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="labelWidth"></param>
        /// <param name="tbWidth"></param>
        /// <returns></returns>
        private TextBox CreateField(string labelText, int x, int y, int labelWidth, int tbWidth)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Location = new Point(x, y + 5),
                Width = labelWidth,
                ForeColor = Color.White
            };
            TextBox tb = new TextBox
            {
                Location = new Point(x + labelWidth + 5, y),
                Width = tbWidth
            };
            this.Controls.Add(lbl);
            this.Controls.Add(tb);
            return tb;
        }
        #endregion

        #region méthodes
        /// <summary>
        /// Charge les utilisateurs dans la ListView.
        /// </summary>
        /// <param name="filtre"></param>
        private void ChargerUtilisateurs(string filtre = "Tous")
        {
            try
            {
                var db = new InterfaceBDD();
                db.Connexion();

                List<Utilisateur> utilisateurs = filtre switch
                {
                    "Clients" => db.RecupererClients().Cast<Utilisateur>().ToList(),
                    "Cuisiniers" => db.RecupererCuisiniers().Cast<Utilisateur>().ToList(),
                    _ => db.RecupererUtilisateurs()
                };

                lvUtilisateurs.Items.Clear();

                foreach (var user in utilisateurs)
                {
                    ListViewItem item = new ListViewItem(user.ID.ToString());
                    item.SubItems.Add(user.Nom);
                    item.SubItems.Add(user.Prenom);
                    item.SubItems.Add(user.Email);
                    item.SubItems.Add(user.Telephone);
                    item.SubItems.Add(user.Adresse);  
                    string type = user is Client ? "Client" : user is Cuisinier ? "Cuisinier" : "N/A";
                    item.SubItems.Add(type);
                    item.SubItems.Add("••••••••");

                    lvUtilisateurs.Items.Add(item);
                }

                db.FermerConnexion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des utilisateurs: " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Gère l'événement de changement de sélection dans le ComboBox de filtre. Recharge les utilisateurs en fonction du filtre sélectionné.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbFiltre_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChargerUtilisateurs(cbFiltre.SelectedItem.ToString());
        }
        /// <summary>
        /// Gère l'événement de changement de sélection dans la ListView. Remplit les champs de texte avec les informations de l'utilisateur sélectionné.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvUtilisateurs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvUtilisateurs.SelectedItems.Count == 0)
            {
                txtNom.Text = "";
                txtPrenom.Text = "";
                txtEmail.Text = "";
                txtTel.Text = "";
                txtAdresse.Text = "";
                txtMdp.Text = "";
                return;
            }

            var item = lvUtilisateurs.SelectedItems[0];
            txtNom.Text = item.SubItems[1].Text;
            txtPrenom.Text = item.SubItems[2].Text;
            txtEmail.Text = item.SubItems[3].Text;
            txtTel.Text = item.SubItems[4].Text;
            txtAdresse.Text = item.SubItems[5].Text;
            txtMdp.Text = ""; // laisser vide pour ne pas changer le mot de passe
        }
        /// <summary>
        /// Gère l'événement de clic sur le bouton "Supprimer". Supprime l'utilisateur sélectionné.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (lvUtilisateurs.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à supprimer.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idUtilisateur = int.Parse(lvUtilisateurs.SelectedItems[0].SubItems[0].Text);
            string nomUtilisateur = lvUtilisateurs.SelectedItems[0].SubItems[1].Text + " " +
                                    lvUtilisateurs.SelectedItems[0].SubItems[2].Text;

            DialogResult result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer l'utilisateur {nomUtilisateur} ?",
                "Confirmation de suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var db = new InterfaceBDD();
                    db.Connexion();
                    bool success = db.SupprimerUtilisateur(idUtilisateur);
                    db.FermerConnexion();

                    if (success)
                    {
                        MessageBox.Show($"L'utilisateur {nomUtilisateur} a été supprimé avec succès.",
                            "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ChargerUtilisateurs(cbFiltre.SelectedItem.ToString());
                    }
                    else
                    {
                        MessageBox.Show("La suppression a échoué.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de la suppression: " + ex.Message,
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        /// <summary>
        /// Gère l'événement de clic sur le bouton "Enregistrer". Met à jour les informations de l'utilisateur sélectionné.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            if (lvUtilisateurs.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à modifier.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idUtilisateur = int.Parse(lvUtilisateurs.SelectedItems[0].SubItems[0].Text);

            var utilisateur = new Utilisateur
            {
                ID = idUtilisateur,
                Nom = txtNom.Text,
                Prenom = txtPrenom.Text,
                Email = txtEmail.Text,
                Telephone = txtTel.Text,
                Adresse = txtAdresse.Text,
                MotDePasse = txtMdp.Text
            };

            try
            {
                var db = new InterfaceBDD();
                db.Connexion();
                bool success = db.MettreAJourUtilisateur(utilisateur);
                db.FermerConnexion();

                if (success)
                {
                    MessageBox.Show("Utilisateur modifié avec succès !");
                    ChargerUtilisateurs(cbFiltre.SelectedItem.ToString());
                }
                else
                {
                    MessageBox.Show("Erreur lors de la mise à jour.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la mise à jour: " + ex.Message);
            }
        }
        #endregion
    }
}
