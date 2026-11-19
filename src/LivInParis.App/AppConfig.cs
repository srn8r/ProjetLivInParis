namespace ProjetLivInParis
{
    /// <summary>
    /// Configuration de l'application, en un seul endroit.
    /// </summary>
    public static class AppConfig
    {
        /// <summary>Variable d'environnement qui remplace la chaîne de connexion par défaut.</summary>
        public const string VariableConnexion = "LIVINPARIS_DB";

        private const string ConnexionParDefaut =
            "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;";

        /// <summary>
        /// Chaîne de connexion MySQL. Lue dans <c>LIVINPARIS_DB</c> si elle est définie,
        /// sinon serveur local avec les identifiants par défaut.
        /// </summary>
        public static string ConnectionString =>
            Environment.GetEnvironmentVariable(VariableConnexion) is { Length: > 0 } valeur
                ? valeur
                : ConnexionParDefaut;
    }
}
