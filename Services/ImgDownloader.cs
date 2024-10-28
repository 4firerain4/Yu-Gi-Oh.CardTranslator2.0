using Yu_Gi_Oh.CardTranslator.Model;

namespace Yu_Gi_Oh.CardTranslator.DAL
{
    public static class ImgDownloader
    {
        public static readonly string CacheDir = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string DataDir = CacheDir + @"Cache\Data"; //Путь к папке Cache в директории программы
        public static readonly HttpClient _client = new();
        public async static Task DoSome(Card card)
        {
            string url = "https://images.ygoprodeck.com/images/cards/";
            string savePath = CacheDir + @"\Cache\SavedImages\";
            using var request = new HttpRequestMessage(HttpMethod.Get, url + card.Code + ".jpg");
            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            await File.WriteAllBytesAsync(savePath + card.Code + ".jpg", await response.Content.ReadAsByteArrayAsync());
        }
    }
}