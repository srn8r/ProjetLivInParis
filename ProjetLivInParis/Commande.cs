using System;

namespace Project_LivInParis
{
    public class Commande
    {
        public int CommandeID { get; set; }
        public DateTime DateCommande { get; set; }
        public string AdresseLivraison { get; set; }
        public decimal PrixTotal { get; set; }
        public string Statut { get; set; }
        public string ModePaiement { get; set; }

        public int ClientID { get; set; }
        public string ClientNom { get; set; }
        public string ClientPrenom { get; set; }
    }
}
