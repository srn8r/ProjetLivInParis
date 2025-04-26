using Project_LivInParis;
using System.Data;

namespace ProjetLivInParis
{
    public partial class Form_AfficherUtilisateur : Form
    {
        private ListView lvUtilisateurs;
        private Button btnFermer;
        private Button btnSupprimer;
        private ComboBox cbFiltre;
        private Label lblFiltre;

        public Form_AfficherUtilisateur()
        {
            InitializeComponent();
            this.Text = "Liste des Utilisateurs";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitialiserComposants();
            ChargerUtilisateurs();
        }

        private void InitialiserComposants()
        {
            // ListView pour les utilisateurs
            lvUtilisateurs = new ListView();
            lvUtilisateurs.Dock = DockStyle.Fill;
            lvUtilisateurs.View = View.Details;
            lvUtilisateurs.FullRowSelect = true;
            lvUtilisateurs.GridLines = true;

            // Ajout des colonnes
            lvUtilisateurs.Columns.Add("ID", 50);
            lvUtilisateurs.Columns.Add("Nom", 120);
            lvUtilisateurs.Columns.Add("Prénom", 120);
            lvUtilisateurs.Columns.Add("Email", 180);
            lvUtilisateurs.Columns.Add("Téléphone", 120);
            lvUtilisateurs.Columns.Add("Type", 80);
            lvUtilisateurs.Columns.Add("Détails", 120);

            // Panel pour les boutons (en bas)
            Panel panelBoutons = new Panel();
            panelBoutons.Dock = DockStyle.Bottom;
            panelBoutons.Height = 60;

            // Panel pour le filtre (en haut)
            Panel panelFiltre = new Panel();
            panelFiltre.Dock = DockStyle.Top;
            panelFiltre.Height = 40;
            panelFiltre.Padding = new Padding(10);

            // Filtre par type d'utilisateur
            lblFiltre = new Label();
            lblFiltre.Text = "Filtrer par:";
            lblFiltre.AutoSize = true;
            lblFiltre.Location = new Point(10, 10);

            cbFiltre = new ComboBox();
            cbFiltre.Items.AddRange(new object[] { "Tous", "Clients", "Cuisiniers" });
            cbFiltre.SelectedIndex = 0;
            cbFiltre.Location = new Point(lblFiltre.Right + 10, 7);
            cbFiltre.Width = 150;
            cbFiltre.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFiltre.SelectedIndexChanged += CbFiltre_SelectedIndexChanged;

            // Boutons
            btnFermer = new Button();
            btnFermer.Text = "Fermer";
            btnFermer.Size = new Size(100, 30);
            btnFermer.Location = new Point(panelBoutons.Width - 120, 15);
            btnFermer.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnFermer.Click += (s, e) => this.Close();

            btnSupprimer = new Button();
            btnSupprimer.Text = "Supprimer";
            btnSupprimer.Size = new Size(100, 30);
            btnSupprimer.Location = new Point(btnFermer.Left - 120, 15);
            btnSupprimer.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnSupprimer.Click += BtnSupprimer_Click;

            // Ajout des contrôles aux panels
            panelFiltre.Controls.Add(lblFiltre);
            panelFiltre.Controls.Add(cbFiltre);

            panelBoutons.Controls.Add(btnFermer);
            panelBoutons.Controls.Add(btnSupprimer);
            // Ajout des panels à la form
            this.Controls.Add(lvUtilisateurs);
            this.Controls.Add(panelBoutons);
            this.Controls.Add(panelFiltre);
        }

        private void ChargerUtilisateurs(string filtre = "Tous")
        {
            try
            {
                var db = new InterfaceBDD();
                db.Connexion();

                List<Utilisateur> utilisateurs;

                switch (filtre)
                {
                    case "Clients":
                        utilisateurs = db.RecupererClients().Cast<Utilisateur>().ToList();
                        break;
                    case "Cuisiniers":
                        utilisateurs = db.RecupererCuisiniers().Cast<Utilisateur>().ToList();
                        break;
                    default:
                        utilisateurs = db.RecupererUtilisateurs();
                        break;
                }

                lvUtilisateurs.Items.Clear();

                foreach (var user in utilisateurs)
                {
                    ListViewItem item = new ListViewItem(user.ID.ToString());
                    item.SubItems.Add(user.Nom);
                    item.SubItems.Add(user.Prenom);
                    item.SubItems.Add(user.Email);
                    item.SubItems.Add(user.Telephone);

                    string type = "N/A";
                    string details = "";

                    if (user is Client client)
                    {
                        type = "Client";
                        details = client.TypeClient;
                        if (client.TypeClient == "Entreprise")
                        {
                            details += $" - {client.NomEntreprise}";
                        }
                    }
                    else if (user is Cuisinier)
                    {
                        type = "Cuisinier";
                    }

                    item.SubItems.Add(type);
                    item.SubItems.Add(details);

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

        private void CbFiltre_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChargerUtilisateurs(cbFiltre.SelectedItem.ToString());
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            // Vérifier si un utilisateur est sélectionné
            if (lvUtilisateurs.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à supprimer.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Récupérer l'ID et le nom de l'utilisateur sélectionné
            int idUtilisateur = int.Parse(lvUtilisateurs.SelectedItems[0].SubItems[0].Text);
            string nomUtilisateur = lvUtilisateurs.SelectedItems[0].SubItems[1].Text + " " +
                                   lvUtilisateurs.SelectedItems[0].SubItems[2].Text;
            string typeUtilisateur = lvUtilisateurs.SelectedItems[0].SubItems[5].Text;

            // Demander confirmation avant de supprimer
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

                    // Supprimer l'utilisateur de la base de données
                    bool suppressionReussie = db.SupprimerUtilisateur(idUtilisateur);
                    db.FermerConnexion();

                    if (suppressionReussie)
                    {
                        MessageBox.Show($"L'utilisateur {nomUtilisateur} a été supprimé avec succès.",
                            "Suppression réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Rafraîchir la liste des utilisateurs
                        ChargerUtilisateurs(cbFiltre.SelectedItem.ToString());
                    }
                    else
                    {
                        MessageBox.Show("La suppression a échoué.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de la suppression de l'utilisateur: " + ex.Message,
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
