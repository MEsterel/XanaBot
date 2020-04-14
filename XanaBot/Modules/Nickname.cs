using Discord;
using Discord.Commands;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Discord.WebSocket;
using XanaBot.Data;

namespace XanaBot.Modules
{
    public class Nickname : ModuleBase<ICommandContext>
    {
        public static Dictionary<ulong, string> surnomsActifs = new Dictionary<ulong, string>();

        [Command("surnom")]
        [Description("Attribue un surnom aléatoire à l'utilisateur spécifié.", "x!surnom <nom de l'utilisateur> [default|fame|halloween|agario|wreckit]")]
        public async Task SurnomAsync(IUser iuser, [Remainder] string theme = "default")
        {
            SocketGuildUser user = (SocketGuildUser)iuser;
            SocketGuild guild = user.Guild;

            if (guild.OwnerId == user.Id)
            {
                await ReplyAsync("Impossible de modifier le surnom du propriétaire " + user.Username + ".");
                return;
            }
            else if (user.IsBot || user.IsWebhook)
            {
                await ReplyAsync("Impossible de modifier le surnom d'un Bot. Tu ne voulais tout de même pas changer un nom qui relève de la perfection j'éspère ?!");
                return;
            }
            else if (surnomsActifs.Keys.Contains(user.Id))
            {
                await ReplyAsync(user.Mention + " a déjà un surnom attrbué par X.A.N.A. en ce moment, impossible de lui en attribuer un autre pour l'instant.");
                return;
            }

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.codetunnel.net/random-nick");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"theme\":\"" + theme + "\"," +
                              "\"sizeLimit\":21}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();
                dynamic djson = System.Web.Helpers.Json.Decode(result);

                if (djson.success != true)
                {
                    await ReplyAsync("Une erreur est survenue. Détails : " + djson.error.message);
                    return;
                }

                surnomsActifs.Add(user.Id, user.Nickname);
                await user.ModifyAsync(x => x.Nickname = (String)djson.nickname);

                await ReplyAsync(user.Username + " s'appelle désormais " + user.Mention + ". Un surnom bien ridicule...");
                

                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer(async (obj) =>
                {
                    await Task.Run(() => ResetSurnom(user));
                    timer.Dispose();
                },
                            null, (int)(Config._INSTANCE.GuildConfigs[Context.Guild.Id].SurnomTime * 1000), System.Threading.Timeout.Infinite);

                return;
            }            
        }

        private async Task ResetSurnom(IUser iuser)
        {
            SocketGuildUser user = (SocketGuildUser)iuser;

            if (!surnomsActifs.Keys.Contains(user.Id))
            {
                // erreur
                // await ReplyAsync("Tentative de reset de surnom par X.A.N.A. non donné par X.A.N.A.");
                return;
            }

            if (user.Nickname == surnomsActifs[user.Id]) // le précédent surnom a déjà été remis?!
            {
                surnomsActifs.Remove(user.Id);
                return;
            }

            await user.ModifyAsync(x => x.Nickname = surnomsActifs[user.Id]);
            surnomsActifs.Remove(user.Id);

            await ReplyAsync("X.A.N.A a été clément avec " + user.Mention + ", il lui a retiré son surnom venu tout droit de Pétaouchnok.");
        }
    }
}
