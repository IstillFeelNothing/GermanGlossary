namespace GermanyGlossary.Data
{
    // Discriminator for the single "Words" table (table-per-hierarchy).
    public enum WordKind
    {
        Noun = 1,
        Verb = 2,
        Adjective = 3,
        Phrase = 4
    }
}
