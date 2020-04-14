using System;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using XanaBot.Modules;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Runtime.InteropServices;
using XanaBot.Data;

namespace XanaBot
{
    public class Program : ModuleBase<ICommandContext>
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();



        public static DiscordSocketClient _client { get; private set; }



        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            XmlManager.LoadXmlConfig();
            Help.LoadModuleHelp();
            Settings.LoadProperties();

            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            //event subscription
            _client.Log += Log;
            _client.MessageReceived += _client_MessageReceived;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.GuildUpdated += _client_GuildUpdated;
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeft;
            _client.Ready += _client_Ready;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, Config._INSTANCE.BotToken);

            await _client.StartAsync();            

            await Task.Delay(-1); // for client to stay alive
        }

        private Task _client_Ready()
        {
            Config._INSTANCE.RefreshGuilds();

            return Task.CompletedTask;
        }

        private Task _client_UserLeft(SocketGuildUser arg)
        {
            Config._INSTANCE.RefreshGuilds();

            return Task.CompletedTask;
        }

        private Task _client_UserJoined(SocketGuildUser arg)
        {
            Config._INSTANCE.RefreshGuilds();

            return Task.CompletedTask;
        }

        private Task _client_GuildUpdated(SocketGuild arg1, SocketGuild arg2)
        {
            Config._INSTANCE.RefreshGuilds();

            return Task.CompletedTask;
        }

        private Task _client_JoinedGuild(SocketGuild arg)
        {
            Config._INSTANCE.RefreshGuilds();

            arg.DefaultChannel.SendMessageAsync("", false,
                new EmbedBuilder()
                {
                    Title = "X.A.N.A. - Introduction",
                    Color = Color.Red,
                    Description = "Bonjour ! Je suis X.A.N.A, tapez **x!help** pour plus d'infos..."
                }.Build());

            return Task.CompletedTask;
        }






        private Task _client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id == Config._INSTANCE.XanaId)
            {
                CFormat.Print(arg.Content, "XANA", DateTime.Now, ConsoleColor.Red);
            }
            else if (arg.Content.Substring(0,2) == "x!")
            {
                CFormat.Print(arg.Content, arg.Author.ToString(), DateTime.Now);
            }

            return Task.CompletedTask;
        }



        public Task Log(LogMessage arg)
        {
            CFormat.WriteLine(arg.ToString());

            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            if (message is null || message.Author.IsBot) return;

            int argPos = 0;

            if (message.HasStringPrefix("x!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync("Commande inconnue. Tapez **x!help** pour obtenir la liste des commandes disponibles.");
                    }
                    else if (result.Error == CommandError.BadArgCount || result.Error == CommandError.ParseFailed)
                    {
                        string specificCommand = message.Content.Substring(2);
                        if (Help.CommandsHelp.Keys.Contains(specificCommand))
                        {
                            EmbedBuilder embedbuilder = new EmbedBuilder()
                            {
                                Title = "X.A.N.A. - Aide sur la commande **" + specificCommand + "**",
                                Color = Color.Red
                            };
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("**" + specificCommand + "** : " + Help.CommandsHelp[specificCommand]);
                            sb.AppendLine("Utilisation : " + Help.CommandsUsage[specificCommand]);
                            embedbuilder.Description = sb.ToString();

                            await context.Channel.SendMessageAsync("Mauvaise syntaxe.", false, embedbuilder.Build());
                            return;
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync("Commande inconnue. Tapez **x!help** pour obtenir la liste des commandes disponibles.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Une erreur est survenue. Détails : " + result.ErrorReason);
                    }
                }
            }
        }
        

        #region "Exit event catcher"
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                XmlManager.SaveXmlConfig();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        #endregion
    }
}