using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlackIOS.Data.Model
{
    public class LoginResponse
    {
        [JsonProperty("user_info")]
        public UserInfo UserInfo { get; set; }

        [JsonProperty("server_info")]
        public ServerInfo ServerInfo { get; set; }
    }

    public class UserInfo
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("exp_date")]
        public string ExpDate { get; set; }

        [JsonProperty("is_trial")]
        public string IsTrial { get; set; }

        [JsonProperty("active_cons")]
        public string ActiveCons { get; set; }

        [JsonProperty("max_connections")]
        public string MaxConnections { get; set; }

        [JsonProperty("allowed_output_formats")]
        public List<string> AllowedOutputFormats { get; set; }
    }

    public class ServerInfo
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }

        [JsonProperty("https_port")]
        public string HttpsPort { get; set; }

        [JsonProperty("server_protocol")]
        public string ServerProtocol { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }

    public class Category
    {
        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("category_name")]
        public string CategoryName { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }
    }

    public class LiveStream
    {
        [JsonProperty("num")]
        public int Num { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("stream_type")]
        public string StreamType { get; set; }

        [JsonProperty("stream_id")]
        public int StreamId { get; set; }

        [JsonProperty("stream_icon")]
        public string StreamIcon { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }
    }

    public class Movie
    {
        [JsonProperty("num")]
        public int Num { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("stream_id")]
        public int StreamId { get; set; }

        [JsonProperty("stream_icon")]
        public string StreamIcon { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("container_extension")]
        public string ContainerExtension { get; set; }

        [JsonProperty("added")]
        public string Added { get; set; }
    }

    public class Series
    {
        [JsonProperty("num")]
        public int Num { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("series_id")]
        public int SeriesId { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("last_modified")]
        public string LastModified { get; set; }
    }

    public class EpgResponse
    {
        [JsonProperty("epg_listings")]
        public List<EpgListing> EpgListings { get; set; } = new List<EpgListing>();
    }

    public class EpgListing
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("epg_id")]
        public string EpgId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("start_timestamp")]
        public string StartTimestamp { get; set; }

        [JsonProperty("end_timestamp")]
        public string EndTimestamp { get; set; }
    }

    public class SeriesInfoResponse
    {
        [JsonProperty("episodes")]
        public Dictionary<string, List<IptvEpisode>> Episodes { get; set; }
    }

    public class IptvEpisode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("episode_num")]
        public int EpisodeNumber { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("container_extension")]
        public string ContainerExtension { get; set; }

        [JsonProperty("info")]
        public IptvEpisodeInfo Info { get; set; }
    }

    public class IptvEpisodeInfo
    {
        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("movie_image")]
        public string MovieImage { get; set; }
    }
}
