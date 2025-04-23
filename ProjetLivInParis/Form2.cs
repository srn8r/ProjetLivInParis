using System;
using System.Windows.Forms;
using Project_LivInParis;

namespace ProjetLivInParis
{
    public partial class Form2 : Form
    {
        private TextBox txtNom, txtPrenom, txtAdresse, txtTelephone, txtEmail, txtMotDePasse;
        private RadioButton rbClient, rbCuisinier;
        private ComboBox cbTypeClient;
        private TextBox txtNomEntreprise, txtReferent;
        private Button btnAjouter;

        public Form2()
        {
            InitializeComponent();
            this.Text = "Ajout Client / Cuisinier";
            this.Size = new System.Drawing.Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

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
            txtAdresse = new TextBox() { Left = 130, Top = top, Width = 300 };
            top += 35;

            Label lblTelephone = new Label() { Text = "Téléphone :", Left = 20, Top = top, Width = labelWidth };
            txtTelephone = new TextBox() { Left = 130, Top = top, Width = 300 };
            top += 35;

            Label lblEmail = new Label() { Text = "Email :", Left = 20, Top = top, Width = labelWidth };
            txtEmail = new TextBox() { Left = 130, Top = top, Width = 300 };
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

            btnAjouter = new Button() { Text = "Ajouter", Left = 180, Top = top, Width = 120 };
            btnAjouter.Click += BtnAjouter_Click;

            Controls.AddRange(new Control[]
            {
                lblNom, txtNom, lblPrenom, txtPrenom, lblAdresse, txtAdresse,
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
            string nom = txtNom.Text.Trim();
            string prenom = txtPrenom.Text.Trim();
            string adresse = txtAdresse.Text.Trim();
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
    }
}
