using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public partial class Form_Cuisinier : Form
    {
        #region propriétés
        private int userId;
        private string prenomCuisinier;
        private TextBox txtNomPlat, txtRegime;
        private DateTimePicker dpFabrication, dpPeremption;
        private NumericUpDown numPrix, numQuantite;
        private Button btnAjouter, btnVoirPlats, btnVoirCommandes;
        private Label lblBienvenue;
        #endregion

        #region constructeur
        public Form_Cuisinier(int id)
        {
            InitializeComponent();
            this.Text = "Espace Cuisinier";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;
            userId = id;

            InitialiserUI();
            ChargerPrenomCuisinier();
        }
        #endregion

        #region structure 
        private void InitialiserUI()
        {
            lblBienvenue = new Label
            {
                Text = "Bonjour !",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 20)
            };
            this.Controls.Add(lblBienvenue);

            GroupBox grpAjout = new GroupBox
            {
                Text = "Ajouter un nouveau plat",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Size = new Size(400, 450),
                Location = new Point(30, 70)
            };
            this.Controls.Add(grpAjout);

            int top = 40;
            int leftLabel = 20, leftInput = 180;
            int labelWidth = 150, inputWidth = 180, spacingY = 50;

            grpAjout.Controls.Add(CreateLabel("Nom du plat :", top, leftLabel));
            txtNomPlat = CreateTextBox(top, leftInput, inputWidth);
            grpAjout.Controls.Add(txtNomPlat);
            top += spacingY;


            grpAjout.Controls.Add(CreateLabel("Quantité :", top, leftLabel));
            numQuantite = new NumericUpDown
            {
                Top = top,
                Left = leftInput,
                Width = inputWidth,
                Minimum = 1,
                Maximum = 100,
                Value = 1,
                Font = new Font("Segoe UI", 10)
            };
            grpAjout.Controls.Add(numQuantite);
            top += spacingY;

            grpAjout.Controls.Add(CreateLabel("Date de fabrication :", top, leftLabel));
            dpFabrication = new DateTimePicker
            {
                Top = top,
                Left = leftInput,
                Width = inputWidth,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10)
            };
            grpAjout.Controls.Add(dpFabrication);
            top += spacingY;

            grpAjout.Controls.Add(CreateLabel("Date de péremption :", top, leftLabel));
            dpPeremption = new DateTimePicker
            {
                Top = top,
                Left = leftInput,
                Width = inputWidth,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(1),
                Font = new Font("Segoe UI", 10)
            };
            grpAjout.Controls.Add(dpPeremption);
            top += spacingY;

            grpAjout.Controls.Add(CreateLabel("Prix (€) :", top, leftLabel));
            numPrix = new NumericUpDown
            {
                Top = top,
                Left = leftInput,
                Width = inputWidth,
                Minimum = 0.1M,
                Maximum = 1000,
                DecimalPlaces = 2,
                Increment = 0.5M,
                Font = new Font("Segoe UI", 10)
            };
            grpAjout.Controls.Add(numPrix);
            top += spacingY;

            grpAjout.Controls.Add(CreateLabel("Régime :", top, leftLabel));
            txtRegime = CreateTextBox(top, leftInput, inputWidth);
            txtRegime.Text = "Standard";  
            grpAjout.Controls.Add(txtRegime);
            top += spacingY + 10;

            btnAjouter = new Button
            {
                Text = "Ajouter le plat",
                Width = 180,
                Height = 40,
                Top = top,
                Left = leftInput,
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnAjouter.Click += BtnAjouter_Click;
            grpAjout.Controls.Add(btnAjouter);

            GroupBox grpGestion = new GroupBox
            {
                Text = "Mes Actions",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.MediumSlateBlue,
                Size = new Size(250, 200),
                Location = new Point(460, 100)
            };
            this.Controls.Add(grpGestion);

            btnVoirPlats = new Button
            {
                Text = "Voir mes plats",
                Width = 180,
                Height = 45,
                Top = 40,
                Left = 30,
                BackColor = Color.Teal,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVoirPlats.Click += (s, e) =>
            {
                var form = new Form_PlatsCuisinier(userId);
                form.ShowDialog();
            };
            grpGestion.Controls.Add(btnVoirPlats);

            btnVoirCommandes = new Button
            {
                Text = "Commandes reçues",
                Width = 180,
                Height = 45,
                Top = 110,
                Left = 30,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVoirCommandes.Click += (s, e) =>
            {
                var form = new Form_CommandesCuisinier(userId);
                form.ShowDialog();
            };
            grpGestion.Controls.Add(btnVoirCommandes);
        }

        private Label CreateLabel(string text, int top, int left)
        {
            return new Label
            {
                Text = text,
                Top = top,
                Left = left,
                Width = 150,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
        }

        private TextBox CreateTextBox(int top, int left, int width)
        {
            return new TextBox
            {
                Top = top,
                Left = left,
                Width = width
            };
        }

        private void ChargerPrenomCuisinier()
        {
            try
            {
                string connStr = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Prenom FROM Identite WHERE ID = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            prenomCuisinier = result.ToString();
                            lblBienvenue.Text = $"Bonjour {prenomCuisinier}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération du prénom : " + ex.Message);
            }
        }
        #endregion 


        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (!ValiderFormulaire()) return;

            string nomPlat = txtNomPlat.Text.Trim();
            string regime = txtRegime.Text.Trim();
            int quantite = (int)numQuantite.Value;
            DateTime dateFab = dpFabrication.Value.Date;
            DateTime datePeremp = dpPeremption.Value.Date;
            decimal prix = numPrix.Value;

            btnAjouter.Enabled = false;

            try
            {
                string connectionString = "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string insertPlat = @"INSERT INTO Plat (Nom, Quantite, date_de_fabrication, Date_de_peremption, Prix, Regime, CuisinierID)
                                          VALUES (@Nom, @Quantite, @DateFab, @DatePeremp, @Prix, @Regime, @CuisinierID)";

                    using (MySqlCommand cmd = new MySqlCommand(insertPlat, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nom", nomPlat);
                        cmd.Parameters.AddWithValue("@Quantite", quantite);
                        cmd.Parameters.AddWithValue("@DateFab", dateFab);
                        cmd.Parameters.AddWithValue("@DatePeremp", datePeremp);
                        cmd.Parameters.AddWithValue("@Prix", prix);
                        cmd.Parameters.AddWithValue("@Regime", regime);
                        cmd.Parameters.AddWithValue("@CuisinierID", userId);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show($"✅ Le plat '{nomPlat}' a été ajouté avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetForm();
                        }
                        else
                        {
                            MessageBox.Show("❌ L'ajout a échoué.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout : " + ex.Message, "Erreur critique", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAjouter.Enabled = true;
            }
        }

        private bool ValiderFormulaire()
        {
            if (string.IsNullOrWhiteSpace(txtNomPlat.Text))
            {
                MessageBox.Show("Le nom du plat est requis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNomPlat.Focus();
                return false;
            }
            if (numQuantite.Value <= 0)
            {
                MessageBox.Show("Veuillez indiquer une quantité supérieure à zéro.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numQuantite.Focus();
                return false;
            }
            if (numPrix.Value <= 0)
            {
                MessageBox.Show("Le prix doit être supérieur à zéro.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPrix.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtRegime.Text))
            {
                MessageBox.Show("Le régime est requis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRegime.Focus();
                return false;
            }
            if (dpPeremption.Value.Date <= dpFabrication.Value.Date)
            {
                MessageBox.Show("La date de péremption doit être postérieure à la date de fabrication.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void ResetForm()
        {
            txtNomPlat.Clear();
            numQuantite.Value = 1;
            dpFabrication.Value = DateTime.Now;
            dpPeremption.Value = DateTime.Now.AddDays(1);  // ➔ Remet à demain
            numPrix.Value = numPrix.Minimum;
            txtRegime.Text = "Standard";  // ➔ Remet à Standard
            txtNomPlat.Focus();
        }

    }
}
