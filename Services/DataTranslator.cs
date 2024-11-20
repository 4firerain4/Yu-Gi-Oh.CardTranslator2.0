using System.Text.Json.Nodes;
using Yu_Gi_Oh.CardTranslator.Model;
using Yu_Gi_Oh.CardTranslator.DAL;
using System.Text;

namespace Yu_Gi_Oh.CardTranslator.Services
{
    static public class DataTranslator
    {

        public static async Task Translate()
        {
            List<Card> cards = await DataPorcessor.ReadDataAsync();
            int i = 0;
            bool needNewClient = true;
            HttpClient client = new();
            List<Card> test = new();

            foreach (var card in cards)
            {
                string[] translation;
                do
                {
                    if (needNewClient)
                    {
                        client.Dispose();
                        client = new();
                    }
                    translation = await SendRequest(client, card.Name + " \n " + card.Types + " \n " + card.Text.Replace("FLIP:", ""));

                    if (translation.Length != 3)
                        needNewClient = true;
                }
                while (translation.Length != 3);

                needNewClient = false;
                card.TranslatedName = translation[0];
                card.TranslatedTypes = translation[1];
                card.TranslatedText = translation[2];

                if (card.Types.Contains("Flip"))
                {
                    card.TranslatedText = card.TranslatedText.Insert(0, "ПЕРЕВОРОТ: ");
                }

                Console.WriteLine($"Card {card.Name} done");
                await Task.Delay(1000);
                i++;
                test.Add(card);
                if (i == 10)
                    Console.ReadKey();
            }

            DataPorcessor.WriteData(cards);
        }

        private async static Task<string[]> SendRequest(HttpClient client, string text)
        {
            var content = new JsonObject
            {

                { "model", "claude-3.5-sonnet" },
                { "messages", new JsonArray
                    {
                        new JsonObject
                        {
                            { "role", "system" },
                            { "content", "Ты обязан писать ИСКЛЮЧИТЕЛЬНО ПЕРЕВОД текста карточки Yu-Gi-Oh, котрый тебе отправляют. Перевод выполняй на русский язык и учитывай, что третья строка - текст игровой карточки. Твой ответ обязан состоять только из перевода полученного текста. Обязательно из одной строки. И Обязательно выглядить следующим образом: первая переведённая строка \n вторая переведённая строка \n третья переведённая строка. Соблюдай этим правила безукоризненно, и НЕ ПИШИ АБСОЛЮТНО НИЧЕГО КРОМЕ ПЕРЕВОДА."}
                        },
                        new JsonObject
                        {
                            { "role", "user" },
                            { "content", $"Ты обязан писать ИСКЛЮЧИТЕЛЬНО ПЕРЕВОД текста карточки, котрый тебе отправляют. Перевод выполняй на русский язык. Твой ответ обязан состоять только из перевода полученного текста. Обязательно из одной строки. И Обязательно выглядить следующим образом: первая переведённая строка \n вторая переведённая строка \n третья переведённая строка. Соблюдай этим правила безукоризненно, и НЕ ПИШИ АБСОЛЮТНО НИЧЕГО КРОМЕ ПЕРЕВОДА. Вот текст для перевода: {text}"}
                        }
                    }
                },
                { "temperature", 0}
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://127.0.0.1:1337/v1/chat/completions");
            request.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var a = JsonData(await response.Content.ReadAsStringAsync()).Split('\n');
            return a;
        }

        private static string JsonData(string text)
        {
            var jsonObject = JsonNode.Parse(text);
            var choices = jsonObject["choices"].AsArray();
            var content = choices[0]["message"]["content"].ToString();
            return content.ToString();
        }
    }
}