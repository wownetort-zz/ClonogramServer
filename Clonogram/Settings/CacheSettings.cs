using System;

namespace Clonogram.Settings
{
    public class CacheSettings
    {
        public TimeSpan Comment { get; set; }
        public TimeSpan Comments { get; set; }
        public TimeSpan Hashtags { get; set; }
        public TimeSpan Likes { get; set; }
        public TimeSpan Photo { get; set; }
        public TimeSpan UserPhotos { get; set; }
        public TimeSpan Story { get; set; }
        public TimeSpan User { get; set; }
        public TimeSpan Users { get; set; }
        public TimeSpan UserSubscribers { get; set; }
        public TimeSpan UserSubscriptions { get; set; }
    }
}
