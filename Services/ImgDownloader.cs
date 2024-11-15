using Yu_Gi_Oh.CardTranslator.Model;

namespace Yu_Gi_Oh.CardTranslator.Services
{
    public static class ImgDownloader
    {
        public static readonly string CacheDir = AppDomain.CurrentDomain.BaseDirectory + @"/Cache";
        public static readonly string _savePath = CacheDir + @"/SavedImages/";
        public static readonly HttpClient _client = new();
        public async static Task Download(Card card)
        {
            if (!CheckFile(card))
            {
                string url = "https://images.ygoprodeck.com/images/cards/";

                using var request = new HttpRequestMessage(HttpMethod.Get, url + card.Code + ".jpg");
                using var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                await File.WriteAllBytesAsync(_savePath + card.Code + ".jpg", await response.Content.ReadAsByteArrayAsync());
            }
        }

        private static bool CheckFile(Card card)
        {
            if (!Directory.Exists(CacheDir))
            {
                Directory.CreateDirectory(_savePath);
                return false;
            }
            if (!File.Exists(_savePath + $"{card.Code}.jpg"))
                return false;
        
            return true;
        }
    }
}