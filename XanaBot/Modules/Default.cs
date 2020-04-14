using Discord;
using Discord.Commands;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;
using System.Linq;
using XanaBot.Data;

namespace XanaBot.Modules
{
    public class Default : ModuleBase<ICommandContext>
    {
        [Command("bonjour")]
        [Description("X.A.N.A. vous répond bonjour.", "x!bonjour")]
        public async Task BonjourAsync()
        {
            await ReplyAsync("Bonjour " + Context.User.Mention + ". Je viens d'activer une tour. Va te faire foutre.");
        }
        

        [Command("piece")]
        [Description("X.A.N.A. tire à pile ou face.", "x!piece [pile|face]")]
        public async Task PieceAsync([Remainder] string choix = "")
        {
            Random rnd = new Random();
            choix = choix.ToLower();

            if (String.IsNullOrWhiteSpace(choix))
            {
                if (rnd.Next(2) == 0)
                {
                    await ReplyAsync("Pile.");
                }
                else
                {
                    await ReplyAsync("Face.");
                }
            }
            else if (choix == "pile" || choix == "face")
            {
                int nbre = rnd.Next(2);

                if (nbre == 0 && choix == "pile")
                {
                    await ReplyAsync("Pile. Tu as gagné " + Context.User.Mention + ", mais on verra la prochaine fois.");
                } 
                else if (nbre == 0 && choix == "face")
                {
                    await ReplyAsync("Pile. Tu as perdu " + Context.User.Mention + ", n'oublies pas que tu n'es qu'un humain !");
                }
                else if (nbre == 1 && choix == "pile")
                {
                    await ReplyAsync("Face. Tu as perdu " + Context.User.Mention + ", n'oublies pas que tu n'es qu'un humain !");
                }
                else if (nbre == 1 && choix == "face")
                {
                    await ReplyAsync("Face. Tu as gagné " + Context.User.Mention + ", mais on verra la prochaine fois.");
                }
            }
            else
            {
                await ReplyAsync("Veuillez choisir pile ou face. Tapez **x!help piece** pour plus d'infos.");
            }
        }



        [Command("select")]
        [Description("Sélectionne un joueur connecté au hasard.", "x!select")]
        public async Task SelectAsync()
        {
            Random rnd = new Random();

            SocketGuild guild = (SocketGuild)Context.Guild;

            var users = guild.Users.Where(x => x.IsBot == false && x.IsWebhook == false && x.Status == UserStatus.Online).ToArray();

            await ReplyAsync(users[rnd.Next(users.Count())].Mention + " a été pointé du doigt par X.A.N.A.");
        }



        [Command("play")]
        [Description("Met de la musique..?", "x!play")]
        public async Task PlayAsync()
        {
            await ReplyAsync("Cet idiot de " + Context.User.Mention + " a tenté de mettre de la musique avec X.A.N.A. Quel abruti...");
        }



        [Command("punchline")]
        [Description("Lance une punchline.", "x!punchline")]
        public async Task PunchlineAsync()
        {
            if (Config._INSTANCE.GuildConfigs[Context.Guild.Id].Blagues.Count == 0)
            {
                await ReplyAsync("Il n'y a pas de punchlines sauvegardées. Ajoutez-en via la commande **x!settings AddPunchlines**.");
                return;
            }

            Random rnd = new Random();

            await ReplyAsync(Config._INSTANCE.GuildConfigs[Context.Guild.Id].Punchlines
                .ElementAt(rnd.Next(Config._INSTANCE.GuildConfigs[Context.Guild.Id].Punchlines.Count())));
        }



        [Command("blague")]
        [Description("Lance une blague.", "x!blague")]
        public async Task BlagueAsync()
        {
            if (Config._INSTANCE.GuildConfigs[Context.Guild.Id].Blagues.Count == 0)
            {
                await ReplyAsync("Il n'y a pas de blagues sauvegardées. Ajoutez-en via la commande **x!settings AddBlague**.");
                return;
            }

            Random rnd = new Random();            

            await ReplyAsync(Config._INSTANCE.GuildConfigs[Context.Guild.Id].Blagues
                .ElementAt(rnd.Next(Config._INSTANCE.GuildConfigs[Context.Guild.Id].Blagues.Count())));
        }


        [Command("roleid")]
        public async Task GetRoleIdAsync(IRole role)
        {
            await ReplyAsync("RoleID : " + role.Id);
        }        
    }
}
