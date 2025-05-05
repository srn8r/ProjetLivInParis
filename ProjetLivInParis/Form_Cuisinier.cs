using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public partial class Form_Cuisinier : Form
    {
        private int userId;
        private TextBox txtNomPlat;
        private DateTimePicker dpFabrication;
        private DateTimePicker dpPeremption;
        private NumericUpDown numPrix;
        private TextBox txtRegime;
        private Button btnAjouter;
        private NumericUpDown numQuantite;

        public Form_Cuisinier(int id)
        {
            InitializeComponent();
            this.Text = "Espace Cuisinier";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MediumPurple;
            userId = id;

            int top = 30;

            Label titre = new Label
            {
                Text = "Ajouter un nouveau plat",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 300) / 2, top)
            };
            this.Controls.Add(titre);
            top += 50;

            // Nom du plat
            AddLabel("Nom du plat :", top);
            txtNomPlat = AddTextBox(top);
            top += 35;

            // Quantité
            AddLabel("Quantité :", top);
            numQuantite = new NumericUpDown
            {
                Top = top,
                Left = 200,
                Width = 200,
                Minimum = 1,
                Maximum = 100,
                Value = 1
            };
            this.Controls.Add(numQuantite);
            top += 35;

            // Date de fabrication
            AddLabel("Date de fabrication :", top);
            dpFabrication = new DateTimePicker { Top = top, Left = 200, Width = 200, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dpFabrication);
            top += 35;

            // Date de péremption
            AddLabel("Date de péremption :", top);
            dpPeremption = new DateTimePicker { Top = top, Left = 200, Width = 200, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dpPeremption);
            top += 35;

            // Prix
            AddLabel("Prix (€) :", top);
            numPrix = new NumericUpDown
            {
                Top = top,
                Left = 200,
                Width = 200,
                Minimum = 0.1M,
                Maximum = 1000,
                DecimalPlaces = 2,
                Increment = 0.5M
            };
            this.Controls.Add(numPrix);
            top += 35;

            // Régime
            AddLabel("Régime :", top);
            txtRegime = AddTextBox(top);
            top += 50;

            // Bouton Ajouter
            btnAjouter = new Button
            {
                Text = "Ajouter le plat",
                Top = top,
                Left = 200,
                Width = 180,
                Height = 45,
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnAjouter.Click += BtnAjouter_Click;
            this.Controls.Add(btnAjouter);
        }

        private void AddLabel(string text, int top)
        {
            this.Controls.Add(new Label
            {
                Text = text,
                Top = top,
                Left = 20,
                Width = 180,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            });
        }

        private TextBox AddTextBox(int top)
        {
            var txt = new TextBox { Top = top, Left = 200, Width = 200 };
            this.Controls.Add(txt);
            return txt;
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomPlat.Text))
            {
                MessageBox.Show("Le nom du plat est requis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNomPlat.Focus();
                return;
            }
            if (numQuantite.Value <= 0)
            {
                MessageBox.Show("Veuillez indiquer une quantité supérieure à zéro.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numQuantite.Focus();
                return;
            }
            if (numPrix.Value <= 0)
            {
                MessageBox.Show("Le prix doit être supérieur à zéro.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPrix.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtRegime.Text))
            {
                MessageBox.Show("Le régime est requis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRegime.Focus();
                return;
            }
            if (dpPeremption.Value.Date <= dpFabrication.Value.Date)
            {
                MessageBox.Show("La date de péremption doit être postérieure à la date de fabrication.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                            MessageBox.Show($"✅ Le plat '{nomPlat}' a bien été ajouté !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void ResetForm()
        {
            txtNomPlat.Clear();
            numQuantite.Value = 1;
            dpFabrication.Value = DateTime.Now;
            dpPeremption.Value = DateTime.Now.AddDays(1);
            numPrix.Value = numPrix.Minimum; 
            txtRegime.Clear();
            txtNomPlat.Focus();
        }
    }
}
