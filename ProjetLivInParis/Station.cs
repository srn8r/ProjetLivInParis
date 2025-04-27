using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_LivInParis
{
    public class Station
    {
        public int Identifiant { get; set; }
        public string Nom { get; set; }
        public string Precedent { get; set; }
        public string Suivant { get; set; }
        public string TempsEntre2Stations { get; set; }
        public string TempsDeChangement { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Ligne { get; set; }

        public Station(int identifiant, string nom, string precedent, string suivant, string tempsEntre, string tempsChangement, double longitude, double latitude, string ligne)
        {
            Identifiant = identifiant;
            Nom = nom;
            Precedent = precedent;
            Suivant = suivant;
            TempsEntre2Stations = tempsEntre;
            TempsDeChangement = tempsChangement;
            Longitude = longitude;
            Latitude = latitude;
            Ligne = ligne;
        }
    }

}

