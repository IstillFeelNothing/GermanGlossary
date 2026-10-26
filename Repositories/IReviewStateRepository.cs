using GermanyGlossary.Models;

namespace GermanyGlossary.Repositories
{
    public interface IReviewStateRepository
    {
        Task<ReviewState?> GetAsync(int wordId);
        Task<IReadOnlyList<ReviewState>> GetAllAsync();

        // States whose NextReviewAt is before the given moment (UTC), most overdue first.
        Task<IReadOnlyList<ReviewState>> GetDueAsync(DateTime dueBeforeUtc);

        // Insert or replace.
        Task SaveAsync(ReviewState state);
    }
}
