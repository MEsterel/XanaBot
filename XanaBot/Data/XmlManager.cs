using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace XanaBot.Data
{
    internal static class XmlManager
    {
        private static DataContractSerializer _Serializer = new DataContractSerializer(typeof(Config));
        private static XmlWriterSettings _settings = new XmlWriterSettings { Indent = true };

        public static void SaveXmlConfig()
        {
            CFormat.Print("Sauvegarde du fichier de configuration.", "XmlManager", DateTime.Now, ConsoleColor.Yellow);

            StreamWriter sr = new StreamWriter(Properties.Settings.Default.configFileName, false);
            XmlWriter writer = XmlWriter.Create(sr, _settings);

            _Serializer.WriteObject(writer, Config._INSTANCE);

            writer.Flush();            
            sr.Flush();
            writer.Close();
            sr.Close();
        }
        
        public static void LoadXmlConfig()
        {
            if (!File.Exists(Properties.Settings.Default.configFileName))
            {
                CreateFile();
                return;
            }

            CFormat.Print("Chargement du fichier de configuration.", "XmlManager", DateTime.Now, ConsoleColor.Yellow);
            FileStream reader = new FileStream(Properties.Settings.Default.configFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            try
            {
                Config._INSTANCE = (Config)_Serializer.ReadObject(reader);

                reader.Close();
            }
            catch (Exception ex)
            {
                reader.Close();

                CFormat.Print("Erreur lors de la lecture du fichier de configuration. Détails : " + ex.Message, "XmlManager", DateTime.Now, ConsoleColor.Yellow);
                CreateFile(); // force file creation
            }
        }

        private static void CreateFile()
        {
            try
            {
                CFormat.Print("Création d'un nouveau fichier de configuration.", "XmlManager", DateTime.Now, ConsoleColor.Yellow);
                Config._INSTANCE.ResetDefault();
                SaveXmlConfig();
                return;
            }
            catch (Exception ex)
            {
                CFormat.Print("Impossible de créer un fichier de configuration. Détails : " + ex.Message, "XmlManager", DateTime.Now, ConsoleColor.Yellow);
                CFormat.Write("Appuyez sur une touche pour quitter l'application...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }
        }

        #region "Errors"
        private static void _Serializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
            CFormat.Print("Erreur lors de la lecture du fichier de configuration : objet non référencé (détails : " + e.UnreferencedObject.ToString() + ".",
                "XmlManager", DateTime.Now, ConsoleColor.Red);
        }

        private static void _Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            CFormat.Print("Erreur lors de la lecture du fichier de configuration : noeud inconnu (nom : " + e.Name + ".",
                "XmlManager", DateTime.Now, ConsoleColor.Red);
        }

        private static void _Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            CFormat.Print("Erreur lors de la lecture du fichier de configuration : élément inconnu (nom : " + e.Element.Name + ".",
                "XmlManager", DateTime.Now, ConsoleColor.Red);
        }

        private static void _Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            CFormat.Print("Erreur lors de la lecture du fichier de configuration : attribut inconnu (nom : " + e.Attr.Name + ".",
                "XmlManager", DateTime.Now, ConsoleColor.Red);
        }
        #endregion
    }
}
