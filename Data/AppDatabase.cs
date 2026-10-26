using SQLite;

namespace GermanyGlossary.Data
{
    public class AppDatabase
    {
        private const string DatabaseFileName = "germanyglossary.db3";

        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private readonly string _databasePath;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private SQLiteAsyncConnection? _connection;

        public AppDatabase()
            : this(Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName))
        {
        }

        public AppDatabase(string databasePath)
        {
            _databasePath = databasePath;
        }

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_connection is not null)
                return _connection;

            await _initLock.WaitAsync();
            try
            {
                if (_connection is null)
                {
                    var connection = new SQLiteAsyncConnection(_databasePath, Flags);
                    await connection.CreateTableAsync<WordEntity>();
                    await connection.CreateTableAsync<ReviewStateEntity>();
                    _connection = connection;
                }

                return _connection;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}
