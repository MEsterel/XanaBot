using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanaBot.Data;

namespace XanaBot.Modules
{
    public class Casino : ModuleBase<ICommandContext>
    {
        private static List<ulong> JoueursTimerTresor = new List<ulong>();

        


        [Command("tresor")]
        [Description("Essayez de trouver le code du coffre au trésor pour gagner trois fois plus de pièces !", "x!tresor <code du coffre au trésor (nombre de 1 à 10)>")]
        public async Task TresorAsync(int number)
        {
            if (JoueursTimerTresor.Contains(Context.User.Id))
            {
                await ReplyAsync(Context.User.Mention + ", attends un peu avant d'essayer de trouver un nouveau trésor.");
                return;
            }


            if (number < 1 || number > 10)
            {
                await ReplyAsync("Veuillez indiquer un nombre de 1 à 10 pour jouer.");
                return;
            }

            Random rnd = new Random();
            int rndNum = rnd.Next(100) + 1;

            int tresorCode = 1;
            int previousVal = 0;

            // Assignation du résultat du tirage au sort
            for (int i = 1; i <= 10; i++)
            {
                if (rndNum <= Config._INSTANCE.TresorProbabilities[i] + previousVal)
                {
                    tresorCode = i;
                    break;
                }

                previousVal += Config._INSTANCE.TresorProbabilities[i];
            }

            // Vérification et résultat
            if (number == tresorCode)
            {
                await ReplyAsync(Context.User.Mention + " a trouvé le code du coffre au trésor (" + tresorCode + ") et a gagné " + tresorCode*3 + " pièce" + CFormat.AddPluralS(tresorCode*3) + " !");
                Money.AddMoney(Context.User, tresorCode*3, Context);

                JoueursTimerTresor.Add(Context.User.Id);

                System.Threading.Timer timer = new System.Threading.Timer((obj) =>
                {
                    JoueursTimerTresor.Remove(Context.User.Id);
                },
                        null, (int)(Config._INSTANCE.GuildConfigs[Context.Guild.Id].TimerTresor * 1000), System.Threading.Timeout.Infinite);
            }
            else
            {
                await ReplyAsync(Context.User.Mention + " n'a pas trouvé le code du coffre au trésor, qui était " + tresorCode + "...");
            }
        }




        [Command("chifoumi")]
        [Description("Jouez à pierre-feuille-ciseaux avec X.A.N.A.", "x!chifoumi <pierre|feuille|ciseaux>")]
        public async Task ChifoumiAsync([Remainder] string choice)
        {
            if (choice != "pierre" && choice != "feuille" && choice != "ciseaux")
            {
                await ReplyAsync("Veuillez choisir entre pierre, feuille et ciseaux.");
                return;
            }



            Random rnd = new Random();
            choice = choice.ToLower();

            var number = rnd.Next(3);

            // 0 = PIERRE
            // 1 = FEUILLE
            // 2 = CISEAUX

            if (number == 0 && choice == "pierre")
            {
                await ReplyAsync("Pierre. **Égalité**.");
            }
            else if (number == 0 && choice == "feuille")
            {
                await ReplyAsync("Pierre. MAIS, TU AS **GAGNÉ** ?! COMMENT EST-CE POSSIBLE ?! (Ce développeur de mes deux a vraiment mal fait son boulot...)");
                Money.AddMoney(Context.User, Config._INSTANCE.GuildConfigs[Context.Guild.Id].ChifoumiWinAmount, Context);
            }
            else if (number == 0 && choice == "ciseaux")
            {
                await ReplyAsync("Pierre. Tu as **perdu**. Comme c'est dommage.");
                Money.RetrieveMoney(Context.User, Config._INSTANCE.GuildConfigs[Context.Guild.Id].ChifoumiWinAmount, Context);
            }

            else if (number == 1 && choice == "pierre")
            {
                await ReplyAsync("Feuille. Tu as **perdu**. Comme c'est dommage.");
                Money.RetrieveMoney(Context.User, Config._INSTANCE.GuildConfigs[Context.Guild.Id].ChifoumiWinAmount, Context);
            }
            else if (number == 1 && choice == "feuille")
            {
                await ReplyAsync("Feuille. **Égalité**.");
            }
            else if (number == 1 && choice == "ciseaux")
            {
                await ReplyAsync("Feuille. MAIS, TU AS **GAGNÉ** ?! COMMENT EST-CE POSSIBLE ?! (Ce développeur de mes deux a vraiment mal fait son boulot...)");
                Money.AddMoney(Context.User, Config._INSTANCE.GuildConfigs[Context.Guild.Id].ChifoumiWinAmount, Context);
            }

            else if (number == 2 && choice == "pierre")
            {
                await ReplyAsync("Ciseaux. MAIS, TU AS **GAGNÉ** ?! COMMENT EST-CE POSSIBLE ?! (Ce développeur de mes deux a vraiment mal fait son boulot...)");
                Money.AddMoney(Context.User, Config._INSTANCE.GuildConfigs[Context.Guild.Id].ChifoumiWinAmount, Context);
            }
            else if (number == 2 && choice == "feuille")
            {
                await ReplyAsync("Ciseaux. Tu as **perdu**. Comme c'est dommage.");
                Money.RetrieveMoney(Context.User, Config._INSTANCE.GuildConfigs[Context.Guild.Id].ChifoumiWinAmount, Context);
            }
            else if (number == 2 && choice == "ciseaux")
            {
                await ReplyAsync("Ciseaux. **Égalité**.");
            }
        }

    }
}
