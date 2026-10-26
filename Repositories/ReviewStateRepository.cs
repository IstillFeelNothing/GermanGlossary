using GermanyGlossary.Data;
using GermanyGlossary.Models;

namespace GermanyGlossary.Repositories
{
    public class ReviewStateRepository : IReviewStateRepository
    {
        private readonly AppDatabase _database;

        public ReviewStateRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task<ReviewState?> GetAsync(int wordId)
        {
            var connection = await _database.GetConnectionAsync();
            var entity = await connection.FindAsync<ReviewStateEntity>(wordId);

            return entity is null ? null : ToModel(entity);
        }

        public async Task<IReadOnlyList<ReviewState>> GetAllAsync()
        {
            var connection = await _database.GetConnectionAsync();
            var entities = await connection.Table<ReviewStateEntity>().ToListAsync();

            return entities.Select(ToModel).ToList();
        }

        public async Task<IReadOnlyList<ReviewState>> GetDueAsync(DateTime dueBeforeUtc)
        {
            var ticks = dueBeforeUtc.ToUniversalTime().Ticks;

            var connection = await _database.GetConnectionAsync();
            var entities = await connection.Table<ReviewStateEntity>()
                .Where(e => e.NextReviewAtUtcTicks < ticks)
                .OrderBy(e => e.NextReviewAtUtcTicks)
                .ToListAsync();

            return entities.Select(ToModel).ToList();
        }

        public async Task SaveAsync(ReviewState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            var connection = await _database.GetConnectionAsync();
            await connection.InsertOrReplaceAsync(ToEntity(state));
        }

        private static ReviewStateEntity ToEntity(ReviewState state) => new()
        {
            WordId = state.WordId,
            Interval = state.Interval,
            EaseFactor = state.EaseFactor,
            Repetitions = state.Repetitions,
            NextReviewAtUtcTicks = state.NextReviewAt.ToUniversalTime().Ticks,
            LastReviewedAtUtcTicks = state.LastReviewedAt?.ToUniversalTime().Ticks
        };

        private static ReviewState ToModel(ReviewStateEntity entity) => new()
        {
            WordId = entity.WordId,
            Interval = entity.Interval,
            EaseFactor = entity.EaseFactor,
            Repetitions = entity.Repetitions,
            NextReviewAt = new DateTime(entity.NextReviewAtUtcTicks, DateTimeKind.Utc),
            LastReviewedAt = entity.LastReviewedAtUtcTicks is long ticks
                ? new DateTime(ticks, DateTimeKind.Utc)
                : null
        };
    }
}
