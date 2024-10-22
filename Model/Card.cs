#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace Yu_Gi_Oh.CardTranslator.Model
{
    public class Card
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Property = "N/A";
        public string Attribute { get; set;}
        public string Types { get; set;}
        public string Level { get; set;}
        public string Stats { get; set;}
        public string Text { get; set; }
        public string TranslatedName { get; set; }
        public string TranslatedTypes { get; set; }
        public string TranslatedText { get; set; }
    }
}