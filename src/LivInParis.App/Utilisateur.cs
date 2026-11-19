using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetLivInParis
{
    public class Utilisateur
    {
        public int ID { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
    }

    public class Client : Utilisateur
    {
        public string TypeClient { get; set; }
        public string NomEntreprise { get; set; }
        public string Referent { get; set; }
    }

    public class Cuisinier : Utilisateur
    {
      
    }
}
