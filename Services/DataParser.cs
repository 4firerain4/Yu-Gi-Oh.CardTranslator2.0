using Yu_Gi_Oh.CardTranslator.Model;
using System.Text.Json.Nodes;
using HtmlAgilityPack;
using System.Web;


namespace Yu_Gi_Oh.CardTranslator.Services
{
    public static class DataParser
    {
        public static async Task<Card> ParseData(string name)
        {
            Card card = new();
            HtmlDocument doc = new();
            string rightName;
            var sss = await LoadPage($"https://yugipedia.com/wiki/{name}");
            doc.LoadHtml(sss);
            HtmlNode nameHtml = doc.DocumentNode.SelectSingleNode("//div[@class = 'heading']");
            rightName = HttpUtility.UrlEncode(HttpUtility.HtmlDecode(nameHtml.InnerText));
            
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get, 
                $"https://db.ygoprodeck.com/api/v7/cardinfo.php?name={rightName}");
                        
            var response = await client.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                Console.WriteLine($"Не удалось спарсить {name}");
            }

            JsonNode node = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            var data = node["data"]?[0];

            try
            {
                card.Name = data["name"].ToString();
            }

            catch
            {
                Console.WriteLine($"Не удалось спарсить {name}");
            }
            card.Code = data["id"].ToString();
            card.Type = data["type"].ToString();
            card.Text = data["desc"].ToString();

            if (data["type"].ToString().Contains("Monster"))
            {
                card.Types = string.Join(" / ", ParseJsonMassive(data["typeline"].AsArray()));
                card.ATK = data["atk"].ToString();
                card.DEF = data["def"].ToString();
                card.Level = data["level"].ToString();
                card.Attribute = data["attribute"].ToString();
            }
            else
            {
                card.Level = data["race"].ToString();
            }

            Console.WriteLine($"Card {card.Name} parsed");

            try
            {
                await ImgDownloader.DoSome(card);
            }
            catch
            {
                Console.WriteLine($"Не скачалась фотка((( {card.Name}");
            }
            return card;
            }
        private static string[] ParseJsonMassive(JsonArray? json)
        {
            if (json is null) return Array.Empty<string>();

            List<string> result = new();

            foreach (var item in json)
            {
                result.Add(item.ToString());
            }

            return result.ToArray();
        }

        private static async Task<string> LoadPage(string link)
        {
            string htmlContent = string.Empty;

            using HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(link);

            if (response.IsSuccessStatusCode)
                htmlContent = await response.Content.ReadAsStringAsync();

            return htmlContent;
        }

    }
}