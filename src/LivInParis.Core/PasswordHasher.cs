using System.Security.Cryptography;
using System.Text;

namespace LivInParis.Core;

/// <summary>
/// Hachage des mots de passe avec PBKDF2-SHA256 et un sel aléatoire.
/// Format stocké : <c>pbkdf2$&lt;itérations&gt;$&lt;sel base64&gt;$&lt;hash base64&gt;</c>.
/// </summary>
public static class PasswordHasher
{
    private const string Prefixe = "pbkdf2";
    private const int Iterations = 100_000;
    private const int TailleSel = 16;
    private const int TailleHash = 32;

    public static string Hacher(string motDePasse)
    {
        ArgumentNullException.ThrowIfNull(motDePasse);
        var sel = RandomNumberGenerator.GetBytes(TailleSel);
        var hash = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, Iterations, HashAlgorithmName.SHA256, TailleHash);
        return $"{Prefixe}${Iterations}${Convert.ToBase64String(sel)}${Convert.ToBase64String(hash)}";
    }

    public static bool EstHache(string? valeurStockee) =>
        valeurStockee is not null && valeurStockee.StartsWith(Prefixe + "$", StringComparison.Ordinal);

    /// <summary>
    /// Vérifie un mot de passe. Les anciens comptes stockés en clair sont encore acceptés :
    /// <paramref name="doitEtreRehache"/> vaut alors true pour que l'appelant enregistre le hash.
    /// </summary>
    public static bool Verifier(string motDePasse, string? valeurStockee, out bool doitEtreRehache)
    {
        doitEtreRehache = false;
        if (string.IsNullOrEmpty(valeurStockee)) return false;

        if (!EstHache(valeurStockee))
        {
            bool ok = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(motDePasse), Encoding.UTF8.GetBytes(valeurStockee));
            doitEtreRehache = ok;
            return ok;
        }

        var parties = valeurStockee.Split('$');
        if (parties.Length != 4 || !int.TryParse(parties[1], out int iterations)) return false;
        try
        {
            var sel = Convert.FromBase64String(parties[2]);
            var attendu = Convert.FromBase64String(parties[3]);
            var calcule = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, iterations, HashAlgorithmName.SHA256, attendu.Length);
            bool ok = CryptographicOperations.FixedTimeEquals(calcule, attendu);
            doitEtreRehache = ok && iterations < Iterations;
            return ok;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
