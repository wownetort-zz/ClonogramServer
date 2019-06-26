using System;

namespace Clonogram
{
    public static class Constants
    {
        public static int ExpirationDate = 30;
        public static string Secret = "Very very very very very very very very very very very very very very very very very very very long MySecret";
        public static string PostgresConnectionString = "Host=rc1b-ee0teiok1tzaipkt.mdb.yandexcloud.net;Port=6432;Username=nstarichenko;Password=Wfhmljns-2;Database=Anyly;SSL Mode=Require;Trust Server Certificate=true";
        public static string RedisConnectionString = "127.0.0.1:6379";

        public static string AccessKey = "c7cS25L80Inr1OueEeUn";
        public static string SecretKey = "zAQAnjAq6dAMZ8xKobsqe9O5Hriskcpp0LXxDp2O";
        public static string BucketName = "clonogram-photos";
        public static string ServiceURL = "https://storage.yandexcloud.net";
    }

    public static class Cache
    {
        public static TimeSpan Comment = TimeSpan.FromDays(365);
        public static TimeSpan Comments = TimeSpan.FromDays(365);
        public static TimeSpan Hashtags = TimeSpan.FromMinutes(30);
        public static TimeSpan Likes = TimeSpan.FromDays(365);
        public static TimeSpan Photo = TimeSpan.FromDays(365);
        public static TimeSpan UserPhotos = TimeSpan.FromDays(365);
        public static TimeSpan Story = TimeSpan.FromDays(365);
        public static TimeSpan User = TimeSpan.FromDays(365);
        public static TimeSpan Users = TimeSpan.FromMinutes(30);
        public static TimeSpan UserSubscribers = TimeSpan.FromDays(365);
        public static TimeSpan UserSubscriptions = TimeSpan.FromDays(365);
    }
}
