using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanaBot.Data;

namespace XanaBot.Modules
{
    public class Isolement : ModuleBase<ICommandContext>
    {
        private static Dictionary<ulong, ulong> joueursIsolés = new Dictionary<ulong, ulong>(); // id du joueur, previous voice channel id


        [Command("isoler")]
        [Description("Envoie un joueur en salle d'isolement.", "x!isoler <nom du joueur>")]
        public async Task IsolerAsync(IGuildUser iuser)
        {
            if (Context.Guild.GetCategoriesAsync().Result.Where(x => x.Id == Config._INSTANCE.GuildConfigs[Context.Guild.Id].IsolementCategoryVoiceChannelId).Count() == 0)
            {
                var _settings = new Settings();

                await ReplyAsync("La catégorie du salon d'isolement n'est pas définie. Remédiez-y avec la commande **x!settings IsolementCategoryVoiceChannelId**.");
                await ReplyAsync("N'oubliez pas d'ajouter des salons interdits aux utilisateurs en période d'isolement ! (**x!settings AddForbiddenIsolementChannelId**)");
                return;
            }



            SocketGuildUser user = (SocketGuildUser)iuser;

            if (user.IsBot || user.IsWebhook)
            {
                await ReplyAsync("Vous ne pouvez isoler que des joueurs. Tu ne comptais tout de même pas m'isoler j'éspère ?!");
                return;
            }

            if (user == null)
            {
                await ReplyAsync("Le joueur ciblé n'existe pas.");
                return;
            }

            if (user.VoiceChannel == null)
            {
                await ReplyAsync(user.Mention + " n'est pas connecté à un serveur vocal.");
                return;
            }

            SocketGuild guild = user.Guild;
            SocketChannel isolementChannel = Program._client.GetChannel(Config._INSTANCE.GuildConfigs[Context.Guild.Id].IsolementVoiceChannelId);

            if (isolementChannel == null)
            {
                await ReplyAsync("Création d'une nouvelle cellule d'isolement.");


                ICategoryChannel category = Program._client.GetGuild(guild.Id).CategoryChannels
                    .FirstOrDefault(x => x.Id == Config._INSTANCE.GuildConfigs[Context.Guild.Id].IsolementCategoryVoiceChannelId);

                RestVoiceChannel rest = await guild.CreateVoiceChannelAsync("isolement");

                await rest.ModifyAsync(x => x.CategoryId = category.Id);
                Config._INSTANCE.GuildConfigs[Context.Guild.Id].IsolementVoiceChannelId = rest.Id;
                XmlManager.SaveXmlConfig();
            }

            try
            {
                foreach (ulong forbiddenId in Config._INSTANCE.GuildConfigs[Context.Guild.Id].ForbiddenIsolementChannelsId)
                {
                    await guild.GetChannel(forbiddenId).AddPermissionOverwriteAsync(user, new OverwritePermissions(0, 13631488));
                }                
            }
            catch { }

            await user.ModifyAsync(x => x.ChannelId = Config._INSTANCE.GuildConfigs[Context.Guild.Id].IsolementVoiceChannelId);
            await ReplyAsync(user.Mention + " est désormais seul, au bord du suicide.");

            joueursIsolés.Add(user.Id, user.VoiceChannel.Id);

            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer(async (obj) =>
            {
                await Task.Run(() => LibreAsync(user, true, true));
                timer.Dispose();
            },
                        null, (int)(Config._INSTANCE.GuildConfigs[Context.Guild.Id].IsolementTime * 1000), System.Threading.Timeout.Infinite);

        }




        [Command("libre")]
        [Description("Libère un joueur de l'isolement.", "x!libre <nom du joueur>")]
        public async Task LibreAsync(IGuildUser iuser, bool noOutput = false, bool xanaOrder = false)
        {
            SocketGuildUser user = (SocketGuildUser)iuser;

            if (user.IsBot || user.IsWebhook)
            {
                await ReplyAsync("Vous ne pouvez rendre libre que des joueurs.");
                return;
            }

            if (!joueursIsolés.Keys.Contains(user.Id))
            {
                if (!noOutput)
                    await ReplyAsync(user.Mention + " n'est pas isolé...");

                return;
            }

            if (!xanaOrder)
            {
                if (Context.Message.Author.Id == user.Id)
                {
                    await ReplyAsync(user.Mention + " a tenté de se libérer tout seul ! Bien essayé...");
                    return;
                }
            }

            SocketGuild guild = user.Guild;
            SocketChannel isolementChannel = Program._client.GetChannel(Config._INSTANCE.GuildConfigs[Context.Guild.Id].IsolementVoiceChannelId);

            if (user == null)
            {
                await ReplyAsync("Le joueur ciblé n'existe pas.");
                joueursIsolés.Remove(user.Id);
                return;
            }

            //permissions
            try
            {
                foreach (ulong forbiddenId in Config._INSTANCE.GuildConfigs[Context.Guild.Id].ForbiddenIsolementChannelsId)
                {
                    await guild.GetChannel(forbiddenId).RemovePermissionOverwriteAsync(user);
                }
            }
            catch { }

            // channel d'isolement
            if (user.VoiceChannel != isolementChannel)
            {
                await ReplyAsync(user.Mention + " n'est pas connecté au serveur d'isolement.");
            }
            else
            {
                await user.ModifyAsync(x => x.ChannelId = joueursIsolés[user.Id]);
            }
            

            await ReplyAsync(user.Mention + " est de nouveau sociable.");

            joueursIsolés.Remove(user.Id);
        }
    }
}
