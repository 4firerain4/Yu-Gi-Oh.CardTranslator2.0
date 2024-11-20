using Yu_Gi_Oh.CardTranslator.Model;

namespace Yu_Gi_Oh.CardTranslator.DAL
{
    static public class DataPorcessor
    {
        public static readonly string CacheDir = AppDomain.CurrentDomain.BaseDirectory + "/Cache";
        public static void WriteData(List<Card> data)
        {
            var query = data.Select(x => $"{x.Code} \t {x.Name} \t {x.TranslatedName} \t {x.Type} \t {x.Types} \t {x.TranslatedTypes} \t {x.ATK} \t {x.DEF} \t {x.Attribute} \t {x.Level} \t {x.Text} \t {x.TranslatedText}");

            File.WriteAllLines(CacheDir + "/data.csv", query);
        }

        public static async Task<List<Card>> ReadDataAsync()
        {
            var lines = File.ReadAllLines(CacheDir + "/data.csv");
            var cards = lines.Select(line =>
            {
                var columns = line.Split('\t').Select(column => column.Trim('\"').Trim().Replace('^', '\n').Replace('ё', 'е')).ToArray();
                return new Card
                {
                    Code = columns[0],
                    Name = columns[1],
                    TranslatedName = columns[2],
                    Type = columns[3],
                    Types = columns[4],
                    TranslatedTypes = columns[5],
                    ATK = columns[6],
                    DEF = columns[7],
                    Attribute = columns[8],
                    Level = columns[9],
                    Text = columns[10],
                    TranslatedText = columns[11]
                };
            }).ToList();
            await Task.CompletedTask;
            return cards;
        }
    }
}