using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Yu_Gi_Oh.CardTranslator.DAL;
using Yu_Gi_Oh.CardTranslator.Model;
using Yu_Gi_Oh.CardTranslator.Services;

namespace Yu_Gi_Oh.CardTranslator;

class Program
{
    static async Task Main(string[] args)
    {
        int maxDegreeOfParallelism = 1;
        SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
        List<Card> cards = new();

        var a = GetNames().Distinct().Select(x =>
        {
            if (x.Contains("#")) x = x.Replace("#", "");
            return x;
        }).ToList();

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

        Console.ReadKey();
    }

    static List<string> GetNames()
    {
        var rawCardsNames = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "cards.txt").ToList();
        var CardsNames = rawCardsNames.Select(x =>
        {
            x = x.Substring(3, x.Length - 5);
            var c = Regex.Replace(x, @".*\|", "");
            return c;
        }).ToList();
        return CardsNames;
    }
}
