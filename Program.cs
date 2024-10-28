using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Yu_Gi_Oh.CardTranslator.DAL;
using Yu_Gi_Oh.CardTranslator.Model;
using Yu_Gi_Oh.CardTranslator.Services;

namespace Yu_Gi_Oh.CardTranslator;

static class Program
{
    static async Task Main(string[] args)
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

        Console.WriteLine("ГатовА");
        Console.ReadKey();
    }

    static List<string> GetNames()
    {
        var rawCardsNames = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "cards.txt").ToList();
        var CardsNames = rawCardsNames.Select(x => x.Trim()).ToList();
        return CardsNames;
    }
}
