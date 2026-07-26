using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlackIOS.Data.Local.Entity;
using SQLite;

namespace BlackIOS.Data.Local
{
    public class DatabaseRepository
    {
        private SQLiteAsyncConnection _db;

        public DatabaseRepository()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "black_iptv.db3");
            _db = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitAsync()
        {
            await _db.CreateTableAsync<CategoryEntity>();
            await _db.CreateTableAsync<LiveStreamEntity>();
            await _db.CreateTableAsync<MovieEntity>();
            await _db.CreateTableAsync<SeriesEntity>();
            await _db.CreateTableAsync<FavoriteEntity>();
        }

        public Task InsertCategories(IEnumerable<CategoryEntity> categories)
        {
            return _db.InsertAllAsync(categories, runInTransaction: true);
        }

        public Task<List<CategoryEntity>> GetCategoriesByType(string type)
        {
            return _db.Table<CategoryEntity>().Where(c => c.Type == type).ToListAsync();
        }

        public Task ClearCategories()
        {
            return _db.DeleteAllAsync<CategoryEntity>();
        }

        public Task InsertLiveStreams(IEnumerable<LiveStreamEntity> streams)
        {
            return _db.InsertAllAsync(streams, runInTransaction: true);
        }

        public Task<List<LiveStreamEntity>> GetLiveStreamsByCategory(string categoryId)
        {
            return _db.Table<LiveStreamEntity>().Where(s => s.CategoryId == categoryId).ToListAsync();
        }

        public Task ClearLiveStreams()
        {
            return _db.DeleteAllAsync<LiveStreamEntity>();
        }

        public async Task InsertMovies(IEnumerable<MovieEntity> movies)
        {
            // O SQLite-net não tem UPSERT fácil para chaves compostas geradas via Index.
            // A forma mais eficiente de "Replace" em lote sem PK simples é deletar e inserir,
            // ou rodar um DeleteAll antes já que o Sync recria. 
            // O SyncViewModel do Kotlin faz ClearAll() antes do download inicial.
            await _db.InsertAllAsync(movies, runInTransaction: true);
        }

        public Task<List<MovieEntity>> GetMoviesByCategory(string categoryId)
        {
            return _db.Table<MovieEntity>().Where(m => m.CategoryId == categoryId).ToListAsync();
        }

        public Task<List<MovieEntity>> GetRecentMovies()
        {
            return _db.Table<MovieEntity>().OrderByDescending(m => m.StreamId).Take(20).ToListAsync();
        }

        public Task<List<MovieEntity>> SearchMovies(string query)
        {
            return _db.Table<MovieEntity>().Where(m => m.Name.Contains(query)).OrderBy(m => m.Name).Take(50).ToListAsync();
        }

        public Task ClearMovies()
        {
            return _db.DeleteAllAsync<MovieEntity>();
        }

        public Task InsertSeries(IEnumerable<SeriesEntity> series)
        {
            return _db.InsertAllAsync(series, runInTransaction: true);
        }

        public Task<List<SeriesEntity>> GetSeriesByCategory(string categoryId)
        {
            return _db.Table<SeriesEntity>().Where(s => s.CategoryId == categoryId).ToListAsync();
        }

        public Task<List<SeriesEntity>> GetRecentSeries()
        {
            return _db.Table<SeriesEntity>().OrderByDescending(s => s.SeriesId).Take(20).ToListAsync();
        }

        public Task<List<SeriesEntity>> SearchSeries(string query)
        {
            return _db.Table<SeriesEntity>().Where(s => s.Name.Contains(query)).OrderBy(s => s.Name).Take(50).ToListAsync();
        }

        public Task ClearSeries()
        {
            return _db.DeleteAllAsync<SeriesEntity>();
        }

        public async Task ClearAll()
        {
            await ClearCategories();
            await ClearLiveStreams();
            await ClearMovies();
            await ClearSeries();
        }
        
        public Task<List<FavoriteEntity>> GetAllFavorites()
        {
            return _db.Table<FavoriteEntity>().ToListAsync();
        }

        public async Task ToggleFavorite(int streamId, string type)
        {
            var fav = await _db.Table<FavoriteEntity>().Where(f => f.StreamId == streamId && f.Type == type).FirstOrDefaultAsync();
            if (fav != null)
                await _db.DeleteAsync(fav);
            else
                await _db.InsertAsync(new FavoriteEntity { StreamId = streamId, Type = type });
        }
    }
}
