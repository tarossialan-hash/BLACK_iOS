using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BlackIOS.Data.Model;
using Newtonsoft.Json;

namespace BlackIOS.Data.Remote
{
    public class IptvApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://bkpac.cc/";

        public IptvApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public Task<LoginResponse> LoginAsync(string username, string password)
        {
            return GetAsync<LoginResponse>($"{_baseUrl}player_api.php?username={username}&password={password}");
        }

        public Task<List<Category>> GetLiveCategoriesAsync(string username, string password)
        {
            return GetAsync<List<Category>>($"{_baseUrl}player_api.php?username={username}&password={password}&action=get_live_categories");
        }

        public Task<List<Category>> GetVodCategoriesAsync(string username, string password)
        {
            return GetAsync<List<Category>>($"{_baseUrl}player_api.php?username={username}&password={password}&action=get_vod_categories");
        }

        public Task<List<Category>> GetSeriesCategoriesAsync(string username, string password)
        {
            return GetAsync<List<Category>>($"{_baseUrl}player_api.php?username={username}&password={password}&action=get_series_categories");
        }

        public Task<List<LiveStream>> GetLiveStreamsAsync(string username, string password, string categoryId = null)
        {
            var url = $"{_baseUrl}player_api.php?username={username}&password={password}&action=get_live_streams";
            if (!string.IsNullOrEmpty(categoryId))
                url += $"&category_id={categoryId}";
            return GetAsync<List<LiveStream>>(url);
        }

        public Task<List<Movie>> GetVodStreamsAsync(string username, string password, string categoryId = null)
        {
            var url = $"{_baseUrl}player_api.php?username={username}&password={password}&action=get_vod_streams";
            if (!string.IsNullOrEmpty(categoryId))
                url += $"&category_id={categoryId}";
            return GetAsync<List<Movie>>(url);
        }

        public Task<List<Series>> GetSeriesAsync(string username, string password, string categoryId = null)
        {
            var url = $"{_baseUrl}player_api.php?username={username}&password={password}&action=get_series";
            if (!string.IsNullOrEmpty(categoryId))
                url += $"&category_id={categoryId}";
            return GetAsync<List<Series>>(url);
        }

        public Task<SeriesInfoResponse> GetSeriesInfoAsync(string username, string password, int seriesId)
        {
            return GetAsync<SeriesInfoResponse>($"{_baseUrl}player_api.php?username={username}&password={password}&action=get_series_info&series_id={seriesId}");
        }

        public Task<EpgResponse> GetShortEpgAsync(string username, string password, int streamId)
        {
            return GetAsync<EpgResponse>($"{_baseUrl}player_api.php?username={username}&password={password}&action=get_short_epg&stream_id={streamId}");
        }
    }
}
