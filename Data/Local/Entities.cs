using SQLite;

namespace BlackIOS.Data.Local.Entity
{
    [Table("categories")]
    public class CategoryEntity
    {
        [PrimaryKey]
        public string CategoryId { get; set; }
        
        public string CategoryName { get; set; }
        
        public int ParentId { get; set; }
        
        // "live", "movie", "series"
        public string Type { get; set; } 
    }

    [Table("live_streams")]
    public class LiveStreamEntity
    {
        [PrimaryKey]
        public int StreamId { get; set; }
        
        public int Num { get; set; }
        
        public string Name { get; set; }
        
        public string StreamType { get; set; }
        
        public string StreamIcon { get; set; }
        
        public string CategoryId { get; set; }
    }

    [Table("movies")]
    public class MovieEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed(Name = "UIX_Movie_StreamCat", Order = 1, Unique = true)]
        public int StreamId { get; set; }

        [Indexed(Name = "UIX_Movie_StreamCat", Order = 2, Unique = true)]
        public string CategoryId { get; set; }

        public int Num { get; set; }
        
        public string Name { get; set; }
        
        public string StreamIcon { get; set; }
        
        public string Rating { get; set; }
        
        public string ContainerExtension { get; set; }
        
        public long Added { get; set; }
    }

    [Table("series")]
    public class SeriesEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed(Name = "UIX_Series_StreamCat", Order = 1, Unique = true)]
        public int SeriesId { get; set; }

        [Indexed(Name = "UIX_Series_StreamCat", Order = 2, Unique = true)]
        public string CategoryId { get; set; }

        public int Num { get; set; }
        
        public string Name { get; set; }
        
        public string Cover { get; set; }
        
        public string Rating { get; set; }
        
        public long Added { get; set; }
    }

    [Table("favorites")]
    public class FavoriteEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int StreamId { get; set; }
        
        public string Type { get; set; } // "vod", "live", "series"
    }
}
