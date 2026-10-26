namespace GermanyGlossary.Models
{
    public class Noun : Word
    {
        public Article Article { get; set; }
        public string? Plural { get; set; }
    }
}
