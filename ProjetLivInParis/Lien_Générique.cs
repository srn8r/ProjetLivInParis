using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_LivInParis
{
    public class Lien<T>
    {
        #region propriétés

        public Noeud<T> Source { get; }
        public Noeud<T> Destination { get; }
        public double Poids { get; }
        #endregion

        #region constructeur
        public Lien(Noeud<T> source, Noeud<T> destination, double poids)
        {
            Source = source;
            Destination = destination;
            Poids = poids;
        }
        #endregion
    }
}
