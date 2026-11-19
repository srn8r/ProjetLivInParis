using LivInParis.Core;

namespace LivInParis.Core.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hacher_PuisVerifier_Fonctionne()
    {
        var hash = PasswordHasher.Hacher("motdepasse123");
        Assert.True(PasswordHasher.EstHache(hash));
        Assert.True(PasswordHasher.Verifier("motdepasse123", hash, out var rehash));
        Assert.False(rehash);
    }

    [Fact]
    public void Verifier_MauvaisMotDePasse_Refuse()
    {
        var hash = PasswordHasher.Hacher("secret");
        Assert.False(PasswordHasher.Verifier("Secret", hash, out _));
    }

    [Fact]
    public void Hacher_DeuxFois_DonneDesHashDifferents()
    {
        Assert.NotEqual(PasswordHasher.Hacher("abc"), PasswordHasher.Hacher("abc"));
    }

    [Fact]
    public void Verifier_AncienMotDePasseEnClair_AccepteEtDemandeUnRehash()
    {
        Assert.True(PasswordHasher.Verifier("ancien", "ancien", out var rehash));
        Assert.True(rehash);
        Assert.False(PasswordHasher.Verifier("faux", "ancien", out rehash));
        Assert.False(rehash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("pbkdf2$abc")]
    [InlineData("pbkdf2$1000$pas-du-base64$xx")]
    public void Verifier_ValeurStockeeInvalide_Refuse(string? stockee)
    {
        Assert.False(PasswordHasher.Verifier("x", stockee, out _));
    }
}
