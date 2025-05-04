using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjetLivInParis
{
    public partial class Form_Cuisinier : Form
    {
        private int userId;
        private TextBox txtNomPlat;
        private TextBox txtType;
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
            this.Size = new Size(600, 450);
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
                Location = new Point(100, top)
            };
            this.Controls.Add(titre);

            top += 50;

            // Nom du plat
            this.Controls.Add(new Label { Text = "Nom du plat :", Top = top, Left = 20, Width = 180, ForeColor = Color.White });
            txtNomPlat = new TextBox { Top = top, Left = 200, Width = 200 };
            this.Controls.Add(txtNomPlat);
            top += 35;

            // Quantite
            this.Controls.Add(new Label { Text = "Quantité :", Top = top, Left = 20, Width = 180, ForeColor = Color.White });
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
            this.Controls.Add(new Label { Text = "Date de fabrication :", Top = top, Left = 20, Width = 180, ForeColor = Color.White });
            dpFabrication = new DateTimePicker { Top = top, Left = 200, Width = 200, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dpFabrication);
            top += 35;

            // Date de péremption
            this.Controls.Add(new Label { Text = "Date de péremption :", Top = top, Left = 20, Width = 180, ForeColor = Color.White });
            dpPeremption = new DateTimePicker { Top = top, Left = 200, Width = 200, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dpPeremption);
            top += 35;

            // Prix
            this.Controls.Add(new Label { Text = "Prix (€) :", Top = top, Left = 20, Width = 180, ForeColor = Color.White });
            numPrix = new NumericUpDown { Top = top, Left = 200, Width = 200, Minimum = 0, Maximum = 1000 };
            this.Controls.Add(numPrix);
            top += 35;

            // Régime
            this.Controls.Add(new Label { Text = "Régime :", Top = top, Left = 20, Width = 120, ForeColor = Color.White });
            txtRegime = new TextBox { Top = top, Left = 200, Width = 200 };
            this.Controls.Add(txtRegime);
            top += 50;

            // Bouton Ajouter
            btnAjouter = new Button
            {
                Text = "Ajouter le plat",
                Top = top,
                Left = 150,
                Width = 150,
                Height = 40,
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAjouter.Click += BtnAjouter_Click;
            this.Controls.Add(btnAjouter);
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomPlat.Text) || string.IsNullOrWhiteSpace(txtRegime.Text) || numQuantite.Value <= 0)
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
                return;
            }

            string nomPlat = txtNomPlat.Text.Trim();
            string regime = txtRegime.Text.Trim();
            int quantite = (int)numQuantite.Value;
            DateTime dateFab = dpFabrication.Value.Date;
            DateTime datePeremp = dpPeremption.Value.Date;
            decimal prix = numPrix.Value;

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
                            MessageBox.Show($"Plat '{nomPlat}' ajouté avec succès !");
                        }
                        else
                        {
                            MessageBox.Show("L'ajout a échoué.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout : " + ex.Message);
            }
        }
    }

}
