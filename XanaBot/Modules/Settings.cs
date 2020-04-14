using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XanaBot.Data;

namespace XanaBot.Modules
{
    public class Settings : ModuleBase<ICommandContext>
    {
        public static Dictionary<string, PropertyInfo> XProperties { get; set; } // TOUTES les propriétés de la CONFIG
        public static Dictionary<string, string> XPropertiesHelp { get; private set; } // Propriétés avec de l'aide
        public static Dictionary<string, string> XPropertiesUsage { get; private set; } // et de l'utilisation et seulement

        [Command("settings")]
        [Description("Définissez les différents paramètres de X.A.N.A. via cette commande.", "x!settings <nom du paramètre>")]
        public async Task SettingsAsync(string settingName = "", [Remainder]string value = null)
        {
            settingName.ToLower();

            // GENERAL SETTINGS HELP
            if ((settingName == "help" && String.IsNullOrWhiteSpace(value)) || settingName == "")
            {

                EmbedBuilder embedbuilder = new EmbedBuilder()
                {
                    Title = "X.A.N.A. - Liste des paramètres",
                    Color = Color.Red
                };

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Insérez 'x!settings ' devant chaque commande.");
                sb.AppendLine();

                foreach (KeyValuePair<string, string> property in XPropertiesHelp)
                {
                    sb.AppendLine("**" + property.Key + "** : " + property.Value);
                }
                embedbuilder.Description = sb.ToString();

                await ReplyAsync("", false, embedbuilder.Build());
                return;
            }

            // SPECIFIC HELP SETTING
            else if (settingName == "help" && !String.IsNullOrWhiteSpace(value))
            {
                if (XPropertiesHelp.Keys.Contains(value))
                {
                    EmbedBuilder embedbuilder = new EmbedBuilder()
                    {
                        Title = "X.A.N.A. - Aide sur le paramètre **" + value + "**",
                        Color = Color.Red
                    };

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("**" + value + "** : " + XPropertiesHelp[value]);
                    sb.AppendLine("Utilisation : " + XPropertiesUsage[value]);
                    embedbuilder.Description = sb.ToString();

                    await ReplyAsync("", false, embedbuilder.Build());
                    return;
                }
                else
                {
                    await ReplyAsync("Paramètre inconnu. Tapez **x!settings help** pour obtenir la liste des paramètres disponibles.");
                    return;
                }
            }

            // GET OAUTH 2.0 URL
            else if (settingName == "oauth")
            {
                XmlManager.SaveXmlConfig();
                await ReplyAsync("Lien pour ajouter X.A.N.A. à un serveur : " + Config._INSTANCE.XanaOAuth2URL);
                return;
            }

            // SAVE CONFIG
            else if (settingName == "save")
            {
                XmlManager.SaveXmlConfig();
                await ReplyAsync("Paramètres sauvegardés.");
                return;
            }

            // SETTING COMMAND
            else if (XProperties.Keys.Contains(settingName)) // si paramètre existe
            {
                Type propertyType = XProperties[settingName].PropertyType;

                if (String.IsNullOrWhiteSpace(value)) // si valeur nulle
                {
                    if (propertyType == typeof(bool))
                    {
                        XProperties[settingName].SetValue(Config._INSTANCE, !(bool)XProperties[settingName].GetValue(Config._INSTANCE));
                    }
                    else
                    {
                        await ReplyAsync("Veuillez indiquer la nouvelle valeur du paramètre.");
                        await SettingsAsync("help", settingName);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        var parameter = CFormat.CoerceArgument(XProperties[settingName].PropertyType, value);

                        if (CheckPropertyPermission(XProperties[settingName], (SocketGuildUser)Context.User))
                        {
                            XProperties[settingName].SetValue(Config._INSTANCE.GuildConfigs[Context.Guild.Id], parameter);                          

                            await ReplyAsync("Paramètre mis à jour.");

                            XmlManager.SaveXmlConfig();
                            return;
                        }
                        else if (Context.Guild.Roles.Where(x => x.Id == Config._INSTANCE.GuildConfigs[Context.Guild.Id].AdminRoleId).Count() == 0)
                        {
                            await ReplyAsync("Veuillez définir le rôle pouvant modifier les paramètres Administrateurs de X.A.N.A. avec le paramètre AdminRoleId.");
                            await Task.Run(() => SettingsAsync("help", "AdminRoleId"));
                            return;
                        }
                        else
                        {
                            await ReplyAsync("**Accès au paramètre limité** aux utilisateurs possédant au moins le rôle avec l'identifiant définit par le paramètre AdminRoleId.");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync("Erreur lors de la mise à jour du paramètre. Détails : " + ex.Message);
                        return;
                    }
                }
            }
            else
            {
                await ReplyAsync("Paramètre inconnu. Tapez **x!settings help** pour obtenir la liste des paramètres disponibles.");
                return;
            }
        }

        private bool CheckPropertyPermission(PropertyInfo propertyInfo, SocketGuildUser user)
        {
            XProperty attr = (XProperty)propertyInfo.GetCustomAttribute(typeof(XProperty));

            // If property does not require admin rights
            if (!attr.AdminOnly)
            {
                return true;
            }

            // If property called is AdminRoleId and that admin role is not set 
            else if (propertyInfo.Name == "AdminRoleId"
                && Context.Guild.Roles.Where(x => x.Id == Config._INSTANCE.GuildConfigs[Context.Guild.Id].AdminRoleId).Count() == 0)
            {
                return true;
            }

            // Si propriété est admin only et que l'utilisateur a au moins 1 role de position supérieure ou égale à celle du role Admin spécifié
            else if (attr.AdminOnly)                
            {
                if (Context.Guild.Roles.Where(x => x.Id == Config._INSTANCE.GuildConfigs[Context.Guild.Id].AdminRoleId).Count() == 0)
                {
                    return false;
                }
                else if (user.Roles.Where(x => x.Position >= Context.Guild.Roles
                .FirstOrDefault(y => y.Id == Config._INSTANCE.GuildConfigs[Context.Guild.Id].AdminRoleId).Position).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // All other cases
            else
            {
                return false;
            }
        }

        internal static void LoadProperties()
        {
            XProperties = typeof(GuildConfig).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(XProperty)).Count() == 1)
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            XPropertiesHelp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            XPropertiesUsage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (PropertyInfo prop in XProperties.Values)
            {
                XProperty attribute = (XProperty)prop.GetCustomAttribute(typeof(XProperty));

                XPropertiesHelp.Add(prop.Name, attribute.Help);
                XPropertiesUsage.Add(prop.Name, attribute.Usage);
            }
        }
    }
}
