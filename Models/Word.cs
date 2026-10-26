namespace GermanyGlossary.Models
{
    public abstract class Word
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string? Example { get; set; }
        public int DeckId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
