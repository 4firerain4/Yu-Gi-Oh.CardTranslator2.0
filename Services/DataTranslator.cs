using System.Text.Json.Nodes;
using Yu_Gi_Oh.CardTranslator.Model;
using Yu_Gi_Oh.CardTranslator.DAL;
using System.Text;
using System.Text.Json;

namespace Yu_Gi_Oh.CardTranslator.Services
{
    public class DataTranslator
    {
        static readonly HttpClient _client = new HttpClient();
        public static async Task Translate()
        {
            List<Card> cards = await DataPorcessor.ReadDataAsync();

            foreach (var card in cards)
            {
                var translation = await SendRequest(card.Name + "\n" + card.Types + "\n" + card.Text);
                card.TranslatedName = translation[0];
                card.TranslatedTypes = translation[1];
                card.TranslatedText = translation[2];
                Console.WriteLine("Card done");
            }

            DataPorcessor.WriteData(cards);
        }

        private async static Task<string[]> SendRequest(string text)
        {
            var content = new JsonObject
            {
                {"q" , $"{text}"},
                {"source" , "en"},
                {"target" , "ru"},
                {"format" , "text"}
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://127.0.0.1:5000/translate");
            request.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var a = JsonData(await response.Content.ReadAsStringAsync()).Split('\n');
            return a;
        }

        private static string JsonData(string text)
        {

            JsonNode data = JsonNode.Parse(text);
            var d = data["translatedText"].ToString();
            return d;
        }
    }
}