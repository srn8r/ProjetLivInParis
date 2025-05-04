using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Project_LivInParis;

namespace ProjetLivInParis
{
    public partial class Form_AjoutUtilisateurs : Form
    {
        private TextBox txtNom, txtPrenom, txtTelephone, txtEmail, txtMotDePasse;
        private ComboBox cbAdresse;
        private RadioButton rbClient, rbCuisinier;
        private ComboBox cbTypeClient;
        private TextBox txtNomEntreprise, txtReferent;
        private Button btnAjouter;
        private StationManager stationManager;

        public Form_AjoutUtilisateurs()
        {
            InitializeComponent();
            this.Text = "Ajout Client / Cuisinier";
            this.Size = new System.Drawing.Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;


            stationManager = new StationManager("SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;"); 
            stationManager.ChargerStationsDepuisBDD();

            InitialiserComposants();
        }

        private void InitialiserComposants()
        {
            int labelWidth = 100;
            int top = 20;

            Label lblNom = new Label() { Text = "Nom :", Left = 20, Top = top, Width = labelWidth };
            txtNom = new TextBox() { Left = 130, Top = top, Width = 300 };
            top += 35;

            Label lblPrenom = new Label() { Text = "Prénom :", Left = 20, Top = top, Width = labelWidth };
            txtPrenom = new TextBox() { Left = 130, Top = top, Width = 300 };
            top += 35;

            Label lblAdresse = new Label() { Text = "Adresse :", Left = 20, Top = top, Width = labelWidth };
            cbAdresse = new ComboBox() { Left = 130, Top = top, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            ChargerStationsMetro();
            top += 35;

            Label lblTelephone = new Label() { Text = "Téléphone :", Left = 20, Top = top, Width = labelWidth };
            txtTelephone = new TextBox() { Left = 130, Top = top, Width = 300 };
            // Ajout d'un placeholder pour le format du téléphone
            txtTelephone.Text = "10 chiffres (ex: 0123456789)";
            txtTelephone.ForeColor = System.Drawing.Color.Gray;
            txtTelephone.Enter += (s, e) => {
                if (txtTelephone.Text == "10 chiffres (ex: 0123456789)")
                {
                    txtTelephone.Text = "";
                    txtTelephone.ForeColor = System.Drawing.Color.Black;
                }
            };
            txtTelephone.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtTelephone.Text))
                {
                    txtTelephone.Text = "10 chiffres (ex: 0123456789)";
                    txtTelephone.ForeColor = System.Drawing.Color.Gray;
                }
            };
            top += 35;

            Label lblEmail = new Label() { Text = "Email :", Left = 20, Top = top, Width = labelWidth };
            txtEmail = new TextBox() { Left = 130, Top = top, Width = 300 };
            // Ajout d'un placeholder pour le format de l'email
            txtEmail.Text = "exemple@domaine.com (pas de majuscules)";
            txtEmail.ForeColor = System.Drawing.Color.Gray;
            txtEmail.Enter += (s, e) => {
                if (txtEmail.Text == "exemple@domaine.com (pas de majuscules)")
                {
                    txtEmail.Text = "";
                    txtEmail.ForeColor = System.Drawing.Color.Black;
                }
            };
            txtEmail.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    txtEmail.Text = "exemple@domaine.com (pas de majuscules)";
                    txtEmail.ForeColor = System.Drawing.Color.Gray;
                }
            };
            top += 35;

            Label lblMotDePasse = new Label() { Text = "Mot de passe :", Left = 20, Top = top, Width = labelWidth };
            txtMotDePasse = new TextBox() { Left = 130, Top = top, Width = 300, UseSystemPasswordChar = true };
            top += 40;

            rbClient = new RadioButton() { Text = "Client", Left = 130, Top = top };
            rbCuisinier = new RadioButton() { Text = "Cuisinier", Left = 250, Top = top };
            rbClient.Checked = true;
            rbClient.CheckedChanged += RbClient_CheckedChanged;
            top += 35;

            Label lblTypeClient = new Label() { Text = "Type client :", Left = 20, Top = top, Width = labelWidth };
            cbTypeClient = new ComboBox() { Left = 130, Top = top, Width = 150 };
            cbTypeClient.Items.AddRange(new[] { "Particulier", "Entreprise" });
            cbTypeClient.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTypeClient.SelectedIndex = 0;
            cbTypeClient.SelectedIndexChanged += CbTypeClient_SelectedIndexChanged;
            top += 35;

            Label lblNomEntreprise = new Label() { Text = "Nom entreprise :", Left = 20, Top = top, Width = labelWidth };
            txtNomEntreprise = new TextBox() { Left = 130, Top = top, Width = 300 };
            top += 35;

            Label lblReferent = new Label() { Text = "Référent :", Left = 20, Top = top, Width = labelWidth };
            txtReferent = new TextBox() { Left = 130, Top = top, Width = 300 };
            top += 50;

            btnAjouter = new Button() { Text = "Ajouter", Left = 180, Top = top, Width = 120 , Height = 40};
            btnAjouter.Click += BtnAjouter_Click;

            Controls.AddRange(new Control[]
            {
                lblNom, txtNom, lblPrenom, txtPrenom, lblAdresse, cbAdresse,
                lblTelephone, txtTelephone, lblEmail, txtEmail, lblMotDePasse, txtMotDePasse,
                rbClient, rbCuisinier,
                lblTypeClient, cbTypeClient,
                lblNomEntreprise, txtNomEntreprise,
                lblReferent, txtReferent,
                btnAjouter
            });

            CbTypeClient_SelectedIndexChanged(null, null);
            RbClient_CheckedChanged(null, null);

        }
        private void ChargerStationsMetro()
        {
            // Récupérer toutes les stations uniques du graphe
            var graphe = stationManager.ObtenirGraphe();
            var stationsUniques = graphe.Noeuds
                .Select(n => n.Valeur.Nom)
                .Distinct()
                .OrderBy(nom => nom)
                .ToArray();

            cbAdresse.Items.AddRange(stationsUniques);

            // Sélectionner la première station par défaut si disponible
            if (cbAdresse.Items.Count > 0)
                cbAdresse.SelectedIndex = 0;
        }

        private void RbClient_CheckedChanged(object sender, EventArgs e)
        {
            bool isClient = rbClient.Checked;
            cbTypeClient.Visible = isClient;
            txtNomEntreprise.Visible = isClient;
            txtReferent.Visible = isClient;
        }

        private void CbTypeClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isEntreprise = cbTypeClient.SelectedItem?.ToString() == "Entreprise";
            txtNomEntreprise.Enabled = isEntreprise;
            txtReferent.Enabled = isEntreprise;
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (!ValiderDonnees())
                return;

            string nom = txtNom.Text.Trim();
            string prenom = txtPrenom.Text.Trim();
            string adresse = cbAdresse.SelectedItem?.ToString() ?? string.Empty;
            string telephone = txtTelephone.Text.Trim();
            string email = txtEmail.Text.Trim();
            string motDePasse = txtMotDePasse.Text.Trim();
            try
            {
                var db = new InterfaceBDD();
                db.Connexion();

                int idUtilisateur = db.InsererUtilisateur(nom, prenom, adresse, telephone, email, motDePasse);

                if (rbClient.Checked)
                {
                    string typeClient = cbTypeClient.SelectedItem.ToString();
                    string nomEntreprise = txtNomEntreprise.Text.Trim();
                    string referent = txtReferent.Text.Trim();

                    db.InsererClient(idUtilisateur, typeClient, nomEntreprise, referent);
                }
                else
                {
                    db.InsererCuisinier(idUtilisateur);
                }

                db.FermerConnexion();
                MessageBox.Show("Utilisateur ajouté avec succès ", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // ou vider les champs si tu veux enchaîner
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool ValiderDonnees()
        {
            // Validation du téléphone (10 chiffres collés)
            if (!Regex.IsMatch(txtTelephone.Text.Trim(), @"^\d{10}$"))
            {
                MessageBox.Show("Le numéro de téléphone doit contenir exactement 10 chiffres sans espaces ni caractères spéciaux.",
                    "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTelephone.Focus();
                return false;
            }

            // Validation de l'email (contient @ et pas de majuscules)
            string email = txtTelephone.Text.Trim();
            if (!email.Contains("@") || email != email.ToLower())
            {
                MessageBox.Show("L'email doit contenir un @ et ne pas avoir de majuscules.",
                    "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return false;
            }

            // Validation de l'adresse (une station de métro doit être sélectionnée)
            if (cbAdresse.SelectedIndex < 0)
            {
                MessageBox.Show("Veuillez sélectionner une station de métro comme adresse.",
                    "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cbAdresse.Focus();
                return false;
            }

            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(txtNom.Text) || string.IsNullOrWhiteSpace(txtPrenom.Text) ||
                string.IsNullOrWhiteSpace(txtMotDePasse.Text))
            {
                MessageBox.Show("Tous les champs sont obligatoires.",
                    "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validation des champs spécifiques au type d'entreprise
            if (rbClient.Checked && cbTypeClient.SelectedItem?.ToString() == "Entreprise")
            {
                if (string.IsNullOrWhiteSpace(txtNomEntreprise.Text) || string.IsNullOrWhiteSpace(txtReferent.Text))
                {
                    MessageBox.Show("Le nom de l'entreprise et le référent sont obligatoires pour une entreprise.",
                        "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }
    }
}
