using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Project_LivInParis;

namespace ProjetLivInParis
{
    public partial class Form_Trajet : Form
    {
        private List<Noeud<Station>> chemin;
        private int tempsTrajet;
        private string titre;

        public Form_Trajet(List<Noeud<Station>> chemin, int tempsTrajet, string titre)
        {
            this.chemin = chemin;
            this.tempsTrajet = tempsTrajet;
            this.titre = titre;

            this.Text = titre;
            this.Size = new Size(800, Math.Max(400, 50 + chemin.Count * 20));
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoScroll = true;

            var panelTrajet = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            panelTrajet.Paint += PanelTrajet_Paint;

            this.Controls.Add(panelTrajet);
        }
        /// <summary>
        /// Dessine le trajet sur le panneau
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PanelTrajet_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var stationFont = new Font("Segoe UI", 9);
            var numLigneFont = new Font("Segoe UI", 9, FontStyle.Bold);
            var titreFont = new Font("Segoe UI", 12, FontStyle.Bold);

            int paddingLateral = 80;
            int hauteurSegment = 90;
            int largeurTotale = ((Panel)sender).ClientSize.Width - (paddingLateral * 2);
            int rayonStation = 8;

            int hauteurTitre = 50;
            Rectangle zoneHeader = new Rectangle(10, 10, ((Panel)sender).ClientSize.Width - 20, hauteurTitre);
            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 250)), zoneHeader);
            g.DrawRectangle(Pens.LightGray, zoneHeader);

            g.DrawString($"Temps de trajet: {tempsTrajet} minutes",
                titreFont, Brushes.DarkBlue, zoneHeader.X + 10, zoneHeader.Y + 15);

            int yPosition = hauteurTitre + 30;

            var segmentsParLigne = ExtraireTrajetParLigne(chemin);

            Dictionary<string, Color> couleursParLigne = new Dictionary<string, Color>(){
                { "1", Color.Gold }, { "2", Color.Blue }, { "3", Color.Brown }, { "3bis", Color.LightSkyBlue },
                { "4", Color.Purple }, { "5", Color.Orange }, { "6", Color.LightGreen }, { "7", Color.Pink },
                { "7bis", Color.LightPink }, { "8", Color.Gray }, { "9", Color.Olive }, { "10", Color.DarkGoldenrod },
                { "11", Color.DarkOrange }, { "12", Color.SeaGreen }, { "13", Color.Teal }, { "14", Color.DarkViolet },
            };

            HashSet<string> segmentsAffichés = new HashSet<string>();

            foreach (var segment in segmentsParLigne)
            {
                try
                {
                    if (segmentsAffichés.Contains(segment))
                        continue;
                    segmentsAffichés.Add(segment);

                    string[] parts = segment.Split(':');
                    if (parts.Length != 2) continue;

                    string ligne = parts[0].Replace("Ligne ", "").Trim();
                    string[] stations = parts[1].Split('→');
                    if (stations.Length != 2) continue;

                    string stationDepart = stations[0].Trim();
                    string stationArrivee = stations[1].Trim();

                    SizeF tailleDepart = g.MeasureString(stationDepart, stationFont);
                    SizeF tailleArrivee = g.MeasureString(stationArrivee, stationFont);
                    SizeF tailleLigne = g.MeasureString($"Ligne {ligne}", numLigneFont);

                    Color couleurLigne = Color.Red;
                    if (couleursParLigne.TryGetValue(ligne, out Color couleur))
                        couleurLigne = couleur;

                    using (Pen penLigne = new Pen(couleurLigne, 6))
                    {
                        g.DrawLine(penLigne, paddingLateral, yPosition, paddingLateral + largeurTotale, yPosition);

                        int yCircle = yPosition - rayonStation;
                        g.FillEllipse(Brushes.White, paddingLateral - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);
                        g.DrawEllipse(Pens.Black, paddingLateral - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);

                        g.FillEllipse(Brushes.White, paddingLateral + largeurTotale - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);
                        g.DrawEllipse(Pens.Black, paddingLateral + largeurTotale - rayonStation, yCircle, rayonStation * 2, rayonStation * 2);

                        float xDepartText = Math.Max(10, paddingLateral - (tailleDepart.Width / 2));
                        g.DrawString(stationDepart, stationFont, Brushes.Black, xDepartText, yPosition + 15);

                        float xArriveeText = paddingLateral + largeurTotale - (tailleArrivee.Width / 2);
                        g.DrawString(stationArrivee, stationFont, Brushes.Black, xArriveeText, yPosition + 15);

                        float xLigneText = paddingLateral + (largeurTotale / 2) - (tailleLigne.Width / 2);
                        RectangleF rectLigne = new RectangleF(xLigneText - 4, yPosition + 40, tailleLigne.Width + 8, tailleLigne.Height + 2);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 255, 255)), rectLigne);

                        using (SolidBrush brush = new SolidBrush(couleurLigne))
                        {
                            g.DrawString($"Ligne {ligne}", numLigneFont, brush, xLigneText, yPosition + 40);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur segment : {segment} - {ex.Message}");
                }

                yPosition += hauteurSegment;
            }
        }
        /// <summary>
        /// Extrait les trajets par ligne à partir du chemin donné.
        /// </summary>
        /// <param name="chemin"></param>
        /// <returns></returns>
        private List<string> ExtraireTrajetParLigne(List<Noeud<Station>> chemin)
        {
            var lignesDetaillees = new List<string>();
            if (chemin == null || chemin.Count < 2) return lignesDetaillees;

            string ligneActuelle = chemin[0].Valeur.Ligne;
            string depart = chemin[0].Valeur.Nom;

            for (int i = 1; i < chemin.Count; i++)
            {
                var courant = chemin[i - 1];
                var suivant = chemin[i];

                if (courant.Valeur.Nom == suivant.Valeur.Nom)
                    continue;

                if (suivant.Valeur.Ligne != ligneActuelle)
                {
                    lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} → {courant.Valeur.Nom}");
                    depart = courant.Valeur.Nom;
                    ligneActuelle = suivant.Valeur.Ligne;
                }
            }
            if (depart != chemin.Last().Valeur.Nom)
            {
                lignesDetaillees.Add($"Ligne {ligneActuelle} : {depart} → {chemin.Last().Valeur.Nom}");
            }

            return lignesDetaillees;
        }
    }
}