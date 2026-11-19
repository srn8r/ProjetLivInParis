using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProjetLivInParis
{
    public class ExportXML
    {
        #region propriétés
        private string connectionString;
        private string xmlOutputPath;
        #endregion

        #region constructeur
        public ExportXML(string dbConnectionString, string outputPath)
        {
            connectionString = dbConnectionString;
            xmlOutputPath = outputPath;
        }
        #endregion

        #region méthodes
        /// <summary>
        /// Importe les données de la base de données vers un fichier XML.
        /// </summary>
        public void ImportDataToXml()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement root = xmlDoc.CreateElement("livinparis");
            root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            root.SetAttribute("xsi:noNamespaceSchemaLocation", "livinparis_schema.xsd");
            root.SetAttribute("version", "1.0");
            xmlDoc.AppendChild(xmlDeclaration);
            xmlDoc.AppendChild(root);

            XmlElement identitesElement = xmlDoc.CreateElement("identites");
            XmlElement clientsElement = xmlDoc.CreateElement("clients");
            XmlElement cuisiniersElement = xmlDoc.CreateElement("cuisiniers");
            XmlElement commandesElement = xmlDoc.CreateElement("commandes");
            XmlElement platsElement = xmlDoc.CreateElement("plats");

            root.AppendChild(identitesElement);
            root.AppendChild(clientsElement);
            root.AppendChild(cuisiniersElement);
            root.AppendChild(commandesElement);
            root.AppendChild(platsElement);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                ImportIdentites(connection, xmlDoc, identitesElement);

                ImportClients(connection, xmlDoc, clientsElement);

                ImportCuisiniers(connection, xmlDoc, cuisiniersElement);

                ImportCommandes(connection, xmlDoc, commandesElement);

                ImportPlats(connection, xmlDoc, platsElement);

                connection.Close();
            }

            xmlDoc.Save(xmlOutputPath);
            Console.WriteLine($"Données exportées avec succès vers {xmlOutputPath}");
        }

        private void ImportIdentites(MySqlConnection connection, XmlDocument xmlDoc, XmlElement parentElement)
        {
            string query = "SELECT ID, Nom, Prenom, Adresse, Telephone, Email FROM identite";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        XmlElement identiteElement = xmlDoc.CreateElement("identite");

                        XmlElement idElement = xmlDoc.CreateElement("ID");
                        idElement.InnerText = reader["ID"].ToString();
                        identiteElement.AppendChild(idElement);

                        XmlElement nomElement = xmlDoc.CreateElement("Nom");
                        nomElement.InnerText = reader["Nom"].ToString();
                        identiteElement.AppendChild(nomElement);

                        XmlElement prenomElement = xmlDoc.CreateElement("Prenom");
                        prenomElement.InnerText = reader["Prenom"].ToString();
                        identiteElement.AppendChild(prenomElement);

                        XmlElement adresseElement = xmlDoc.CreateElement("Adresse");
                        adresseElement.InnerText = reader["Adresse"].ToString();
                        identiteElement.AppendChild(adresseElement);

                        XmlElement telephoneElement = xmlDoc.CreateElement("Telephone");
                        telephoneElement.InnerText = reader["Telephone"].ToString();
                        identiteElement.AppendChild(telephoneElement);

                        XmlElement emailElement = xmlDoc.CreateElement("Email");
                        emailElement.InnerText = reader["Email"].ToString();
                        identiteElement.AppendChild(emailElement);

                        parentElement.AppendChild(identiteElement);
                    }
                }
            }
        }

        private void ImportClients(MySqlConnection connection, XmlDocument xmlDoc, XmlElement parentElement)
        {
            string query = "SELECT ID, TypeClient, NomEntreprise, Referent FROM Client";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        XmlElement clientElement = xmlDoc.CreateElement("client");

                        XmlElement idElement = xmlDoc.CreateElement("ID");
                        idElement.InnerText = reader["ID"].ToString();
                        clientElement.AppendChild(idElement);

                        XmlElement typeClientElement = xmlDoc.CreateElement("TypeClient");
                        typeClientElement.InnerText = reader["TypeClient"].ToString();
                        clientElement.AppendChild(typeClientElement);

                        XmlElement nomEntrepriseElement = xmlDoc.CreateElement("NomEntreprise");
                        nomEntrepriseElement.InnerText = reader["NomEntreprise"] == DBNull.Value ? "" : reader["NomEntreprise"].ToString();
                        clientElement.AppendChild(nomEntrepriseElement);

                        XmlElement referentElement = xmlDoc.CreateElement("Referent");
                        referentElement.InnerText = reader["Referent"] == DBNull.Value ? "" : reader["Referent"].ToString();
                        clientElement.AppendChild(referentElement);

                        parentElement.AppendChild(clientElement);
                    }
                }
            }
        }

        private void ImportCuisiniers(MySqlConnection connection, XmlDocument xmlDoc, XmlElement parentElement)
        {
            string query = "SELECT ID FROM Cuisinier";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        XmlElement cuisinierElement = xmlDoc.CreateElement("cuisinier");

                        XmlElement idElement = xmlDoc.CreateElement("ID");
                        idElement.InnerText = reader["ID"].ToString();
                        cuisinierElement.AppendChild(idElement);

                        parentElement.AppendChild(cuisinierElement);
                    }
                }
            }
        }

        private void ImportCommandes(MySqlConnection connection, XmlDocument xmlDoc, XmlElement parentElement)
        {
            string query = "SELECT CommandeID, DateCommande, AdresseLivraison, PrixTotal, Statut, ModePaiement, ClientID FROM Commande";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        XmlElement commandeElement = xmlDoc.CreateElement("commande");

                        XmlElement commandeIdElement = xmlDoc.CreateElement("CommandeID");
                        commandeIdElement.InnerText = reader["CommandeID"].ToString();
                        commandeElement.AppendChild(commandeIdElement);

                        XmlElement dateCommandeElement = xmlDoc.CreateElement("DateCommande");
                        dateCommandeElement.InnerText = Convert.ToDateTime(reader["DateCommande"]).ToString("yyyy-MM-dd");
                        commandeElement.AppendChild(dateCommandeElement);

                        XmlElement adresseLivraisonElement = xmlDoc.CreateElement("AdresseLivraison");
                        adresseLivraisonElement.InnerText = reader["AdresseLivraison"].ToString();
                        commandeElement.AppendChild(adresseLivraisonElement);

                        XmlElement prixTotalElement = xmlDoc.CreateElement("PrixTotal");
                        prixTotalElement.InnerText = reader["PrixTotal"].ToString();
                        commandeElement.AppendChild(prixTotalElement);

                        XmlElement statutElement = xmlDoc.CreateElement("Statut");
                        statutElement.InnerText = reader["Statut"].ToString();
                        commandeElement.AppendChild(statutElement);

                        XmlElement modePaiementElement = xmlDoc.CreateElement("ModePaiement");
                        modePaiementElement.InnerText = reader["ModePaiement"].ToString();
                        commandeElement.AppendChild(modePaiementElement);

                        XmlElement clientIdElement = xmlDoc.CreateElement("ClientID");
                        clientIdElement.InnerText = reader["ClientID"].ToString();
                        commandeElement.AppendChild(clientIdElement);

                        parentElement.AppendChild(commandeElement);
                    }
                }
            }
        }

        private void ImportPlats(MySqlConnection connection, XmlDocument xmlDoc, XmlElement parentElement)
        {
            string query = "SELECT PlatID, Nom, Quantite, date_de_fabrication, Date_de_peremption, Prix, Regime, CuisinierID, CommandeID FROM Plat";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        XmlElement platElement = xmlDoc.CreateElement("plat");

                        XmlElement platIdElement = xmlDoc.CreateElement("PlatID");
                        platIdElement.InnerText = reader["PlatID"].ToString();
                        platElement.AppendChild(platIdElement);

                        XmlElement nomElement = xmlDoc.CreateElement("Nom");
                        nomElement.InnerText = reader["Nom"].ToString();
                        platElement.AppendChild(nomElement);

                        XmlElement quantiteElement = xmlDoc.CreateElement("Quantite");
                        quantiteElement.InnerText = reader["Quantite"].ToString();
                        platElement.AppendChild(quantiteElement);

                        XmlElement dateFabricationElement = xmlDoc.CreateElement("date_de_fabrication");
                        dateFabricationElement.InnerText = Convert.ToDateTime(reader["date_de_fabrication"]).ToString("yyyy-MM-dd");
                        platElement.AppendChild(dateFabricationElement);

                        XmlElement datePeremptionElement = xmlDoc.CreateElement("Date_de_peremption");
                        datePeremptionElement.InnerText = Convert.ToDateTime(reader["Date_de_peremption"]).ToString("yyyy-MM-dd");
                        platElement.AppendChild(datePeremptionElement);

                        XmlElement prixElement = xmlDoc.CreateElement("Prix");
                        prixElement.InnerText = reader["Prix"].ToString();
                        platElement.AppendChild(prixElement);

                        XmlElement regimeElement = xmlDoc.CreateElement("Regime");
                        regimeElement.InnerText = reader["Regime"].ToString();
                        platElement.AppendChild(regimeElement);

                        XmlElement cuisinierIdElement = xmlDoc.CreateElement("CuisinierID");
                        cuisinierIdElement.InnerText = reader["CuisinierID"].ToString();
                        platElement.AppendChild(cuisinierIdElement);

                        XmlElement commandeIdElement = xmlDoc.CreateElement("CommandeID");
                        commandeIdElement.InnerText = reader["CommandeID"] == DBNull.Value ? "" : reader["CommandeID"].ToString();
                        platElement.AppendChild(commandeIdElement);

                        parentElement.AppendChild(platElement);
                    }
                }
            }
        }
        #endregion
    }
}
