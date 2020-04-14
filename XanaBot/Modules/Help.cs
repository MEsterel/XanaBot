using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XanaBot.Modules
{
    public class Help : ModuleBase<ICommandContext>
    {
        private const string _commandNamespace = "XanaBot.Modules";

        public static Dictionary<string, string> CommandsHelp { get; private set; }
        public static Dictionary<string, string> CommandsUsage { get; private set; }

        [Command("help")]
        [Description("Affiche ce message.", "x!help [nom de la commande]")]
        public async Task HelpAsync(string specificCommand = "")
        {
            if (String.IsNullOrWhiteSpace(specificCommand))
            {
                EmbedBuilder embedbuilder = new EmbedBuilder()
                {
                    Title = "X.A.N.A. - Liste des commandes",
                    Color = Color.Red
                };
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Insérez 'x!' devant chaque commande.");
                sb.AppendLine();
                foreach (KeyValuePair<string, string> command in CommandsHelp)
                {
                    sb.AppendLine("**" + command.Key + "** : " + command.Value);
                }
                embedbuilder.Description = sb.ToString();

                await Context.Channel.SendMessageAsync("", false, embedbuilder.Build());
                return;
            }
            else
            {
                if (CommandsHelp.Keys.Contains(specificCommand))
                {
                    EmbedBuilder embedbuilder = new EmbedBuilder()
                    {
                        Title = "X.A.N.A. - Aide sur la commande **" + specificCommand + "**",
                        Color = Color.Red
                    };
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("**" + specificCommand + "** : " + CommandsHelp[specificCommand]);
                    sb.AppendLine("Utilisation : " + CommandsUsage[specificCommand]);
                    embedbuilder.Description = sb.ToString();

                    await Context.Channel.SendMessageAsync("", false, embedbuilder.Build());
                    return;
                }
                else
                {
                    await ReplyAsync("Commande inconnue. Tapez **x!help** pour obtenir la liste des commandes disponibles.");
                }
            }
        }

        internal static void LoadModuleHelp()
        {
            CommandsHelp = new Dictionary<string, string>();
            CommandsUsage = new Dictionary<string, string>();

            // Use reflection to load all of the classes in the Commands namespace:
            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == _commandNamespace
                    select t;
            var commandClasses = q.ToList();

            foreach (var commandClass in commandClasses)
            {
                // Load the method info from each class into a dictionary:
                var methods = commandClass.GetMethods().Where(m => m.GetCustomAttributes(false)
                                                            .Where(a => a.GetType().Name.Contains(typeof(Description).Name)).Count() > 0);
                foreach (var method in methods)
                {
                    // Get the Attribute as MMasterCommand
                    object[] methodAttributes = method.GetCustomAttributes(false).ToArray();
                    Discord.Commands.CommandAttribute cattribute;
                    Description dattribute;
                    try
                    {
                        cattribute = (Discord.Commands.CommandAttribute)methodAttributes[2];
                        dattribute = (Description)methodAttributes[3];


                        CommandsHelp.Add(cattribute.Text, dattribute.Text);
                        CommandsUsage.Add(cattribute.Text, dattribute.Usage);
                    }
                    catch { }

                }
            }

            CommandsHelp = CommandsHelp.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            CommandsUsage = CommandsUsage.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Description : System.Attribute
    {
        public readonly string Text;
        public readonly string Usage;

        public Description(string text, string usage)
        {
            Text = text;
            Usage = usage;
        }
    }
}
