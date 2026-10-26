using GermanyGlossary.Models;

namespace GermanyGlossary.Services
{
    public interface ISpacedRepetitionAlgorithm
    {
        ReviewState Apply(ReviewState state, ReviewRating rating);
    }
}
