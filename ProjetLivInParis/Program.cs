using ProjetLivInParis;

namespace TestLinInParis
{
    internal static class Program
    {
        /// <summary>
        ///  Point d'entrée de l'application
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region export données de visual studio vers XML (à dé-commenter pour tester)
            /*
            ExportXML export = new ExportXML(
                "SERVER=127.0.0.1;PORT=3306;DATABASE=livinparis;UID=root;PASSWORD=root;",
                "C:\\ProgramData\\EditiX\\livinparis.xml");

            try
            {
                export.ImportDataToXml();
                MessageBox.Show("Exportation XML terminée avec succès!", "Succès",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exportation XML: {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */
            #endregion

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form_Accueil());

        }
    }
}