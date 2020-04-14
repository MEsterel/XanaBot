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
    public class Config
    {
        public static Config _INSTANCE = new Config();
        private Config() { }

        [DataMember]
        public string BotToken { get; set; }
        [DataMember]
        public ulong XanaId { get; set; }

        /// <summary>
        /// Key : GuildId, Value : GuildConfig
        /// </summary>
        [DataMember]
        public Dictionary<ulong, GuildConfig> GuildConfigs { get; set; }

        

        [DataMember]
        public Dictionary<int, int> TresorProbabilities { get; set; }



        [DataMember]
        public string XanaOAuth2URL { get; set; }

        /// <summary>
        /// Refreshes config file guilds
        /// </summary>
        public void RefreshGuilds()
        {
            foreach (SocketGuild guild in Program._client.Guilds)
            {
                if (GuildConfigs.ContainsKey(guild.Id))
                {
                    GuildConfigs[guild.Id].RefreshUsers();
                }
                else
                {
                    CreateGuild(guild.Id);
                    GuildConfigs[guild.Id].RefreshUsers();
                }
            }
        }

        private void CreateGuild(ulong guildId)
        {
            GuildConfig guildConfig = new GuildConfig(guildId);

            GuildConfigs.Add(guildConfig.GuildId, guildConfig);
        }

        /// <summary>
        /// /!\ WARNING /!\ NEVER USE UNLESS FILE CREATION
        /// </summary>
        public void ResetDefault()
        {
            CFormat.Print("Mise en place des paramètres par défaut.", "Config", DateTime.Now, ConsoleColor.Yellow);


            BotToken = (string)CInput.ReadFromConsole("BotToken=", ConsoleInputType.String, false, ConsoleColor.White);

            XanaId = (ulong)CInput.ReadFromConsole("XanaId=", ConsoleInputType.Ulong, false, ConsoleColor.White, 18);


            GuildConfigs = new Dictionary<ulong, GuildConfig>();            

            TresorProbabilities = new Dictionary<int, int>() { { 1, 18 }, { 2, 16 }, { 3, 15 }, { 4, 13 }, { 5, 11 }, { 6, 9 }, { 7, 7 }, { 8, 5 }, { 9, 4 }, { 10, 2 } };

            XanaOAuth2URL = "";


            CFormat.Print("Paramètres appliqués.", "Config", DateTime.Now, ConsoleColor.Yellow);
        }
    }
}
