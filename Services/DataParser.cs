#pragma warning disable CS8602

using Yu_Gi_Oh.CardTranslator.Model;
using System.Text.Json.Nodes;
using HtmlAgilityPack;
using System.Web;



namespace Yu_Gi_Oh.CardTranslator.Services
{
    public static class DataParser
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<Card> ParseData(string name)
        {
            string rightName = await GetCurrectName(name);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://db.ygoprodeck.com/api/v7/cardinfo.php?name={rightName}");

            var response = await _httpClient.SendAsync(request);

            JsonNode node = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            Card card = MapCardData(node);

            //await ImgDownloader.Download(card);

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

            using HttpResponseMessage response = await _httpClient.GetAsync(link);

            if (response.IsSuccessStatusCode)
                htmlContent = await response.Content.ReadAsStringAsync();

            return htmlContent;
        }
        private static async Task<string> GetCurrectName(string name)
        {
            HtmlDocument doc = new();
            var rightNamePage = await LoadPage($"https://yugipedia.com/wiki/{name}");
            doc.LoadHtml(rightNamePage);
            HtmlNode nameHtml = doc.DocumentNode.SelectSingleNode("//div[@class = 'heading']");
            return HttpUtility.UrlEncode(HttpUtility.HtmlDecode(nameHtml.InnerText));
        }
        private static Card MapCardData(JsonNode node)
        {
            var card = new Card();
            var data = node["data"]?[0];

            card.Name = data["name"].ToString();
            card.Code = data["id"].ToString();
            card.Type = data["type"].ToString();
            card.Text = data["desc"].ToString().Replace("\n", " | ").Replace("\r", "");

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
                card.Types = data["race"].ToString();
            }

            Console.WriteLine($"Card {card.Name} parsed");
            return card;
        }

    }
}