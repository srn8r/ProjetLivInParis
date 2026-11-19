namespace LivInParis.Core;

/// <summary>
/// Arc orienté pondéré entre deux nœuds.
/// </summary>
public class Lien<T>
{
    public Lien(Noeud<T> source, Noeud<T> destination, double poids)
    {
        Source = source;
        Destination = destination;
        Poids = poids;
    }

    public Noeud<T> Source { get; }
    public Noeud<T> Destination { get; }
    public double Poids { get; }
}
