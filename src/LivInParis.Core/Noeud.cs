namespace LivInParis.Core;

/// <summary>
/// Nœud d'un graphe orienté pondéré, portant une valeur de type <typeparamref name="T"/>.
/// </summary>
public class Noeud<T>
{
    public Noeud(int id, T valeur)
    {
        Id = id;
        Valeur = valeur;
    }

    public int Id { get; }
    public T Valeur { get; set; }
    public List<Lien<T>> LiensSortants { get; } = new();

    public override string ToString() => $"{Id}: {Valeur}";
}
