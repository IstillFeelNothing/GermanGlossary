namespace GermanyGlossary.Models
{
    public class Verb : Word
    {
        public string? Praeteritum { get; set; }
        public string? PartizipII { get; set; }
        public AuxiliaryVerb Auxiliary { get; set; }
        public bool IsSeparable { get; set; }
    }
}
