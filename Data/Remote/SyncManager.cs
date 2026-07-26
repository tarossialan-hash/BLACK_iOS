using System;
using System.Linq;
using System.Threading.Tasks;
using BlackIOS.Data.Model;
using BlackIOS.Data.Local.Entity;

namespace BlackIOS.Data.Remote
{
    public class SyncManager
    {
        private readonly IptvApiClient _api;
        private readonly Local.DatabaseRepository _db;

        public SyncManager(IptvApiClient api, Local.DatabaseRepository db)
        {
            _api = api;
            _db = db;
        }

        public async Task SyncWithServerAsync(
            string username, 
            string password, 
            Action<float, string> onProgress,
            Action<string> onError,
            Action onComplete)
        {
            try
            {
                await _db.InitAsync();
                await _db.ClearAll();

                // 1. Live
                onProgress(0.05f, "Sincronizando canais...");
                var liveCats = await _api.GetLiveCategoriesAsync(username, password);
                if (liveCats != null)
                {
                    await _db.InsertCategories(liveCats.Select(c => new CategoryEntity { CategoryId = c.CategoryId, CategoryName = c.CategoryName, ParentId = c.ParentId, Type = "live" }));
                }

                onProgress(0.15f, "Sincronizando canais...");
                var liveStreams = await _api.GetLiveStreamsAsync(username, password);
                if (liveStreams != null)
                {
                    await _db.InsertLiveStreams(liveStreams.Select(s => new LiveStreamEntity { StreamId = s.StreamId, Num = s.Num, Name = s.Name, StreamType = s.StreamType, StreamIcon = s.StreamIcon, CategoryId = s.CategoryId }));
                }

                // 2. VOD
                onProgress(0.30f, "Sincronizando filmes...");
                var vodCats = await _api.GetVodCategoriesAsync(username, password);
                if (vodCats != null)
                {
                    await _db.InsertCategories(vodCats.Select(c => new CategoryEntity { CategoryId = c.CategoryId, CategoryName = c.CategoryName, ParentId = c.ParentId, Type = "movie" }));
                }

                onProgress(0.40f, "Sincronizando filmes...");
                var movies = await _api.GetVodStreamsAsync(username, password);
                if (movies != null)
                {
                    await _db.InsertMovies(movies.Where(m => !string.IsNullOrEmpty(m.CategoryId)).Select(m => new MovieEntity { StreamId = m.StreamId, Num = m.Num, Name = m.Name, StreamIcon = m.StreamIcon, Rating = m.Rating, CategoryId = m.CategoryId, ContainerExtension = m.ContainerExtension, Added = long.TryParse(m.Added, out long added) ? added : 0 }));
                }

                // 3. Series
                onProgress(0.55f, "Sincronizando séries...");
                var seriesCats = await _api.GetSeriesCategoriesAsync(username, password);
                if (seriesCats != null)
                {
                    await _db.InsertCategories(seriesCats.Select(c => new CategoryEntity { CategoryId = c.CategoryId, CategoryName = c.CategoryName, ParentId = c.ParentId, Type = "series" }));
                }

                onProgress(0.65f, "Sincronizando séries...");
                var series = await _api.GetSeriesAsync(username, password);
                if (series != null)
                {
                    await _db.InsertSeries(series.Where(s => !string.IsNullOrEmpty(s.CategoryId)).Select(s => new SeriesEntity { SeriesId = s.SeriesId, Num = s.Num, Name = s.Name, Cover = s.Cover, Rating = s.Rating, CategoryId = s.CategoryId, Added = long.TryParse(s.LastModified, out long added) ? added : 0 }));
                }

                onProgress(1.0f, "Concluído!");
                onComplete();
            }
            catch (Exception ex)
            {
                onError(ex.Message);
            }
        }
    }
}
