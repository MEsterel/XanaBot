using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XanaBot.Data;
using System.Threading.Tasks;
using Discord;

namespace XanaBot.Modules
{
    public class Money : ModuleBase<ICommandContext>
    {
        [Command("money")]
        [Description("Consultez votre porte-monnaie.", "x!money")]
        public async Task MoneyAsync()
        {
            double money = Config._INSTANCE.GuildConfigs[Context.Guild.Id].XUsers[Context.User.Id].Money;
            await ReplyAsync("Solde de " + Context.User.Mention + " : **" + money + "** pièce" + CFormat.AddPluralS(money));
        }

        public static void AddMoney(IUser user, double amount, ICommandContext _context)
        {
            Config._INSTANCE.GuildConfigs[_context.Guild.Id].XUsers[_context.User.Id].Money += amount;
            double money = Config._INSTANCE.GuildConfigs[_context.Guild.Id].XUsers[_context.User.Id].Money;

            _context.Channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = "X.A.N.A. - Comptabilité",
                Color = Color.Red,
                Description = user.Mention + " a gagné " + amount + " pièce" + CFormat.AddPluralS(amount) + "."
                + Environment.NewLine + "Nouveau solde : **" + money + "** pièce" + CFormat.AddPluralS(money)

            }.Build());

            XmlManager.SaveXmlConfig();
        }

        public static void RetrieveMoney(IUser user, double amount, ICommandContext _context)
        {
            Config._INSTANCE.GuildConfigs[_context.Guild.Id].XUsers[_context.User.Id].Money -= amount;
            double money = Config._INSTANCE.GuildConfigs[_context.Guild.Id].XUsers[_context.User.Id].Money;

            _context.Channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = "X.A.N.A. - Comptabilité",
                Color = Color.Red,
                Description = user.Mention + " a perdu " + amount + " pièce" + CFormat.AddPluralS(amount) + "."
                + Environment.NewLine + "Nouveau solde : **" + money + "** pièce" + CFormat.AddPluralS(money)
            }.Build());

            XmlManager.SaveXmlConfig();
        }
    }
}
