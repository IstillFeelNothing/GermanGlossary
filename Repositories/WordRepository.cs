using GermanyGlossary.Data;
using GermanyGlossary.Models;

namespace GermanyGlossary.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly AppDatabase _database;

        public WordRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task<IReadOnlyList<Word>> GetAllAsync()
        {
            var connection = await _database.GetConnectionAsync();
            var entities = await connection.Table<WordEntity>()
                .OrderBy(e => e.Text)
                .ToListAsync();

            return entities.Select(ToModel).ToList();
        }

        public async Task<Word?> GetByIdAsync(int id)
        {
            var connection = await _database.GetConnectionAsync();
            var entity = await connection.FindAsync<WordEntity>(id);

            return entity is null ? null : ToModel(entity);
        }

        public async Task AddAsync(Word word)
        {
            ArgumentNullException.ThrowIfNull(word);

            var connection = await _database.GetConnectionAsync();
            var entity = ToEntity(word);
            entity.Id = 0;

            await connection.InsertAsync(entity);
            word.Id = entity.Id;
        }

        public async Task<bool> UpdateAsync(Word word)
        {
            ArgumentNullException.ThrowIfNull(word);

            var connection = await _database.GetConnectionAsync();
            var updated = await connection.UpdateAsync(ToEntity(word));

            return updated > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var connection = await _database.GetConnectionAsync();
            var deleted = 0;

            // The review progress belongs to the word, so it goes together with it.
            await connection.RunInTransactionAsync(tran =>
            {
                tran.Delete<ReviewStateEntity>(id);
                deleted = tran.Delete<WordEntity>(id);
            });

            return deleted > 0;
        }

        private static WordEntity ToEntity(Word word)
        {
            var entity = new WordEntity
            {
                Id = word.Id,
                Text = word.Text,
                Translation = word.Translation,
                Example = word.Example,
                DeckId = word.DeckId,
                CreatedAtUtcTicks = word.CreatedAt.ToUniversalTime().Ticks
            };

            switch (word)
            {
                case Noun noun:
                    entity.Kind = WordKind.Noun;
                    entity.Article = (int)noun.Article;
                    entity.Plural = noun.Plural;
                    break;
                case Verb verb:
                    entity.Kind = WordKind.Verb;
                    entity.Praeteritum = verb.Praeteritum;
                    entity.PartizipII = verb.PartizipII;
                    entity.Auxiliary = (int)verb.Auxiliary;
                    entity.IsSeparable = verb.IsSeparable;
                    break;
                case Adjective adjective:
                    entity.Kind = WordKind.Adjective;
                    entity.Comparative = adjective.Comparative;
                    entity.Superlative = adjective.Superlative;
                    break;
                case Phrase:
                    entity.Kind = WordKind.Phrase;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported word type: {word.GetType().Name}");
            }

            return entity;
        }

        private static Word ToModel(WordEntity entity)
        {
            Word word = entity.Kind switch
            {
                WordKind.Noun => new Noun
                {
                    Article = (Article)(entity.Article ?? 0),
                    Plural = entity.Plural
                },
                WordKind.Verb => new Verb
                {
                    Praeteritum = entity.Praeteritum,
                    PartizipII = entity.PartizipII,
                    Auxiliary = (AuxiliaryVerb)(entity.Auxiliary ?? 0),
                    IsSeparable = entity.IsSeparable ?? false
                },
                WordKind.Adjective => new Adjective
                {
                    Comparative = entity.Comparative,
                    Superlative = entity.Superlative
                },
                WordKind.Phrase => new Phrase(),
                _ => throw new NotSupportedException($"Unknown word kind: {entity.Kind}")
            };

            word.Id = entity.Id;
            word.Text = entity.Text;
            word.Translation = entity.Translation;
            word.Example = entity.Example;
            word.DeckId = entity.DeckId;
            word.CreatedAt = new DateTime(entity.CreatedAtUtcTicks, DateTimeKind.Utc);

            return word;
        }
    }
}
