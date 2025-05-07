using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_LivInParis
{
    public class Noeud<T>
    {
        #region champs et propriétés

        public T Valeur { get; set; }
        public int Id { get; }  
        public List<Lien<T>> LiensSortants { get; set; }

        #endregion

        #region constructeur
        public Noeud(int id, T valeur)
        {
            Id = id;
            Valeur = valeur;
            LiensSortants = new List<Lien<T>>();
        }
        #endregion
    }
}
