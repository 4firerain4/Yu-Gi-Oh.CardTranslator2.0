using Yu_Gi_Oh.CardTranslator.Model;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Yu_Gi_Oh.CardTranslator.DAL;

namespace Yu_Gi_Oh.CardTranslator.Services
{
    public class DataParser
    {
        public static async Task<Card> ParseData(string name)
        {
            var link = await LoadPage(name);
            var document = new HtmlDocument();
            document.LoadHtml(link);
            HtmlNodeCollection text2 = document.DocumentNode.SelectNodes("//div[contains(@class, 'infocolumn')]//table/tbody");

            Card card = new();
            var block = text2[0].ChildNodes;

            foreach(var node in block)
            {
                if(node.InnerText.Contains("Monster"))
                    card = GetMonster(block);
                if(node.InnerText.Contains("Spell"))
                    card = GetSpell(block);
                if(node.InnerText.Contains("Trap"))
                    card = GetSpell(block);
            }
    
            card.Name = name;
            if (card.Code[0] == '0') card.Code = card.Code.TrimStart('0');
            // try
            // {
            //     await ImgDownloader.DoSome(card);
            // }
            // catch
            // {
            //     Console.WriteLine($"Не скачалась фотка((( {card.Name}");
            // }
            return card;
        }

        private static async Task<string> LoadPage(string name)
        {
            string htmlContent = string.Empty;

            using HttpClient client = new();
            name = name.Replace(" ", "_");
            using HttpResponseMessage response = await client.GetAsync("https://yugipedia.com/wiki/" + name.Replace(" ", "_"));

            if (response.IsSuccessStatusCode)
                htmlContent = await response.Content.ReadAsStringAsync();

            return htmlContent;
        }
        private static Card GetMonster(HtmlNodeCollection table)
        {
            Card result = new();
            foreach(var row in table)
            {
                if (row.Name =="#text") break;
                if (row.ChildNodes[0].InnerText == "Card type") result.Type = row.ChildNodes[2].InnerText.Trim('\n');
                if (row.ChildNodes[0].InnerText == "Attribute") result.Attribute = row.ChildNodes[2].InnerText.Trim('\n');
                if (row.ChildNodes[0].InnerText == "Types") result.Types = row.ChildNodes[2].InnerText.Trim('\n');
                if (row.ChildNodes[0].InnerText == "Level") result.Level = row.ChildNodes[2].InnerText.Trim('\n');
                if (row.ChildNodes[0].InnerText == "ATK / DEF") result.Stats = row.ChildNodes[2].InnerText.Trim('\n');
                if (row.ChildNodes[0].InnerText == "Password") result.Code = row.ChildNodes[2].InnerText.Trim('\n').TrimStart('0');
            }

            result.Text = table[table.Count()-2].InnerText.Trim('\n');
    
            return result;
        }

        private static Card GetSpell(HtmlNodeCollection table)
        {
            Card result = new();
            foreach(var row in table)
            {
                if (row.Name =="#text") break;
                if (row.ChildNodes[0].InnerText == "Card type") result.Type = row.ChildNodes[2].InnerText.Trim('\n');
                if (row.ChildNodes[0].InnerText == "Property") result.Property = row.ChildNodes[2].InnerText.Trim('\n');
                if (row.ChildNodes[0].InnerText == "Password") result.Code = row.ChildNodes[2].InnerText.Trim('\n').TrimStart('0');
            }

            result.Text = table[table.Count()-2].InnerText.Trim('\n');
    
            return result;
        }
    }
}