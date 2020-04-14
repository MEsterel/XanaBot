using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XanaBot.Modules
{
    public class RandomPic : ModuleBase<ICommandContext>
    {
        [Command("pic")]
        [Description("Affiche une image alétoire trouvée sur Google correspondant au terme précisé.", "x!pic <terme de la recherche>")]
        public async Task PicAsync([Remainder]string term)
        {
            string html = GetHtmlCode(term);
            List<string> urls = GetUrls(html);
            var rnd = new Random();

            int randomUrl = rnd.Next(0, urls.Count - 1);

            string luckyUrl = urls[randomUrl];

            EmbedBuilder embedbuilder = new EmbedBuilder()
            {
                Title = "X.A.N.A. - Résultat de la recherche '" + term + "'",
                Color = Color.Red,
                ImageUrl = luckyUrl,
                Footer = new EmbedFooterBuilder() { Text = "Demandé par " + Context.User.Username }
            };
            await Context.Channel.SendMessageAsync("", false, embedbuilder.Build());
            return;
        }

        [Command("william")]
        [Description("Invoque la phrase de séduction irrésistible de William.", "x!william")]
        public async Task WilliamAsync()
        {
            string html = GetHtmlCode("william coach en séduction");
            List<string> urls = GetUrls(html);
            var rnd = new Random();

            int randomUrl = rnd.Next(0, 27);

            string luckyUrl = urls[randomUrl];

            EmbedBuilder embedbuilder = new EmbedBuilder()
            {
                Title = "William, coach en séduction",
                Color = Color.Red,
                ImageUrl = luckyUrl,
                Description = "Bonjour, je m'appelle William, j'ai 32 ans, je réside à Arcachon, je travaille dans l'immobilier et donc je me recherche une copine.",
                Footer = new EmbedFooterBuilder() { Text = "Demandé par " + Context.User.Username }
            };
            await Context.Channel.SendMessageAsync("", false, embedbuilder.Build());
            return;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Ne pas supprimer d'objets plusieurs fois")]
        private string GetHtmlCode(string term)
        {
            string url = "https://www.google.com/search?q=" + term + "&tbm=isch";
            string data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            var response = (HttpWebResponse)request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return "";
                using (var sr = new StreamReader(dataStream))
                {
                    data = sr.ReadToEnd();
                }
            }
            return data;
        }

        private List<string> GetUrls(string html)
        {
            var urls = new List<string>();

            int ndx = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            while (ndx >= 0)
            {
                ndx = html.IndexOf("\"", ndx + 4, StringComparison.Ordinal);
                ndx++;
                int ndx2 = html.IndexOf("\"", ndx, StringComparison.Ordinal);
                string url = html.Substring(ndx, ndx2 - ndx);
                urls.Add(url);
                ndx = html.IndexOf("\"ou\"", ndx2, StringComparison.Ordinal);
            }
            return urls;
        }
    }
}
