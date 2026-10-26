using GermanyGlossary.Models;

namespace GermanyGlossary.Repositories
{
    public interface IWordRepository
    {
        Task<IReadOnlyList<Word>> GetAllAsync();
        Task<Word?> GetByIdAsync(int id);
        Task AddAsync(Word word);
        Task<bool> UpdateAsync(Word word);
        Task<bool> DeleteAsync(int id);
    }
}
