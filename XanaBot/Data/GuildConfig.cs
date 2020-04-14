using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace XanaBot.Data
{
    [DataContract]
    public class GuildConfig
    {
        public GuildConfig(ulong guildId)
        {
            GuildId = guildId;
            ResetDefault();
        }
        
        
        [DataMember]
        public Dictionary<ulong,XUser> XUsers { get; set; }

        [DataMember]
        public ulong GuildId { get; set; }


        [DataMember]
        [XProperty("Paramétrez l'identifiant du channel d'isolement. [ATTENTION : valeur dynamique gérée par X.A.N.A.]", "x!settings IsolementVoiceChannelId <id>", true)]
        public ulong IsolementVoiceChannelId { get; set; }

        [DataMember]
        [XProperty("Paramétrez l'identifiant de la catégorie du channel d'isolement.", "x!settings IsolementCategoryVoiceChannelId <id>", true)]
        public ulong IsolementCategoryVoiceChannelId { get; set; }

        [DataMember]
        [XProperty("Paramétrez la durée d'isolement (en secondes).", "x!settings IsolementTime <durée en secondes>", true)]
        public double IsolementTime { get; set; }

        [DataMember]
        [XProperty("Paramétrez la durée de changement du surnom (en secondes).", "x!settings SurnomTime <durée en secondes>", true)]
        public double SurnomTime { get; set; }

        [DataMember]
        [XProperty("Paramétrez l'identifiant du rôle pouvant modifier les paramètres Administrateurs de X.A.N.A.", "x!settings AdminRoleId <id>", true)]
        public ulong AdminRoleId { get; set; }




        [XProperty("Ajoutez un salon interdit aux utilisateurs en période d'isolement.", "x!settings AddForbiddenIsolementChannelId <id>", true)]
        public ulong AddForbiddenIsolementChannelId
        {
            set
            {
                if (ForbiddenIsolementChannelsId.Contains(value))
                {
                    throw new Exception("Le salon indiqué a déjà été interdit.");
                }
                ForbiddenIsolementChannelsId.Add(value);
            }
        }

        [XProperty("Retirez un salon interdit aux utilisateurs en période d'isolement.", "x!settings RemoveForbiddenIsolementChannelId <id>", true)]
        public ulong RemoveForbiddenIsolementChannelId
        {
            set
            {
                if (!ForbiddenIsolementChannelsId.Contains(value))
                {
                    throw new Exception("Le salon indiqué n'est pas interdit.");
                }
                ForbiddenIsolementChannelsId.Remove(value);
            }
        }

        // Liste des identifiants des channels iterdites d'accès lors d'une période d'isolement.
        [DataMember]
        public List<ulong> ForbiddenIsolementChannelsId { get; set; }




        [XProperty("Ajoutez une punchline.", "x!settings AddPunchline <punchline>")]
        public string AddPunchline
        {
            set
            {
                if (Punchlines.Contains(value))
                {
                    throw new Exception("Une punchline identique a déjà été ajoutée.");
                }
                Punchlines.Add(value);
            }
        }

        [XProperty("Supprimez une punchline.", "x!settings RemovePunchline <punchline>", true)]
        public string RemovePunchline
        {
            set
            {
                if (!Punchlines.Contains(value))
                {
                    throw new Exception("La punchline spécifiée n'existe pas, impossible de la retirer.");
                }
                Punchlines.Remove(value);
            }
        }

        [DataMember]
        public List<string> Punchlines { get; set; }




        [XProperty("Ajoutez une punchline.", "x!settings AddPunchline <punchline>")]
        public string AddBlague
        {
            set
            {
                if (Blagues.Contains(value))
                {
                    throw new Exception("Une blague identique a déjà été ajoutée.");
                }
                Blagues.Add(value);
            }
        }

        [XProperty("Supprimez une punchline.", "x!settings RemovePunchline <punchline>", true)]
        public string RemoveBlague
        {
            set
            {
                if (!Blagues.Contains(value))
                {
                    throw new Exception("La blague spécifiée n'existe pas, impossible de la retirer.");
                }
                Blagues.Remove(value);
            }
        }

        [DataMember]
        public List<string> Blagues { get; set; }



        [DataMember]
        [XProperty("Modifiez le montant de pièces gagné avec une partie de Chifoumi.", "x!settings ChifoumiWinAmount <nombre de pièces>", true)]
        public double ChifoumiWinAmount { get; set; }
        [DataMember]
        [XProperty("Modifiez le montant de pièces perdu avec une partie de Chifoumi.", "x!settings ChifoumiLostAmount <nombre de pièces>", true)]
        public double ChifoumiLostAmount { get; set; }

        [DataMember]
        [XProperty("Modifiez la durée d'attente en secondes avant de pouvoir réutiliser x!tresor.", "x!settings TimerTresor <durée en secondes>", true)]
        public double TimerTresor { get; set; }




        internal void RefreshUsers()
        {
            foreach (SocketGuildUser user in Program._client.Guilds.First(x => x.Id == GuildId).Users)
            {
                if (user.IsBot || user.IsWebhook)
                {
                    continue;
                }

                if (!XUsers.Keys.Contains(user.Id))
                {
                    CreateUser(user.Id);
                }
            }
        }

        private void CreateUser(ulong id)
        {
            XUser xuser = new XUser(id);

            XUsers.Add(xuser.Id, xuser);
        }

        public void ResetDefault()
        {
            XUsers = new Dictionary<ulong, XUser>();

            IsolementVoiceChannelId = 0;

            IsolementCategoryVoiceChannelId = 0;

            IsolementTime = 10;

            SurnomTime = 60;

            AdminRoleId = 0;

            Punchlines = new List<string>();

            Blagues = new List<string>();

            ChifoumiWinAmount = 1;
            ChifoumiLostAmount = -1;

            TimerTresor = 30;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class XProperty : System.Attribute
    {
        public readonly string Help;
        public readonly string Usage;
        public readonly bool AdminOnly;

        public XProperty(string help, string usage, bool adminOnly = false)
        {
            Help = help;
            Usage = usage;
            AdminOnly = adminOnly;
        }
    }
}
