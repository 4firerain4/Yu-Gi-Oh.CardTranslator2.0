using Yu_Gi_Oh.CardTranslator.DAL;
using Yu_Gi_Oh.CardTranslator.Model;
using Yu_Gi_Oh.CardTranslator.Services;

namespace Yu_Gi_Oh.CardTranslator;

static class Program
{
    public static readonly string CacheDir = AppDomain.CurrentDomain.BaseDirectory + "Cache";
    static async Task Main(string[] args)
    {
        List<Card> cards;

        if (!File.Exists(CacheDir + "/data.csv"))
        {
            await GetCardsData();
            cards = await DataPorcessor.ReadDataAsync();
        }
        else
        {
            //await DataTranslator.Translate();
            cards = await DataPorcessor.ReadDataAsync();
        }

        foreach (var card in cards)
        {
            CardCleaner.Clean(card);
        }

        Console.ReadKey();
    }

    static List<string> GetNames()
    {
        var rawCardsNames = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "cards.txt").ToList();
        var CardsNames = rawCardsNames.Select(x => x.Trim()).ToList();
        return CardsNames;
    }

    private static async Task GetCardsData()
    {
        int maxDegreeOfParallelism = 8;
        SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
        List<Card> cards = new();

        var a = GetNames().Distinct().Select(x => x.Replace("#", "")).ToList();

        var tasks = a.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                var result = await DataParser.ParseData(item);
                lock (cards) // Синхронизация доступа к общей коллекции
                {
                    cards.Add(result);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        DataPorcessor.WriteData(cards);
    }
}
