using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Clonogram.Repositories
{
    public class HashtagsRepository : IHashtagsRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;
        private readonly ConnectionStrings _connectionStrings;

        public HashtagsRepository(IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings, IOptions<ConnectionStrings> connectionStrings)
        {
            _memoryCache = memoryCache;
            _connectionStrings = connectionStrings.Value;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<List<Guid>> GetPhotos(Guid hashtagId)
        {
            return await _memoryCache.GetOrCreateAsync(hashtagId, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = _cacheSettings.Hashtags;
                return await GetPhotosByHashtag(hashtagId);
            });
        }

        private async Task<List<Guid>> GetPhotosByHashtag(Guid hashtagId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"select photo_id from photos_hashtags where hashtag_id = @p_hashtag_id"
            };
            cmd.Parameters.AddWithValue("p_hashtag_id", hashtagId);
            var reader = await cmd.ExecuteReaderAsync();

            var photos = new List<Guid>();
            while (await reader.ReadAsync())
            {
                photos.Add(reader.GetGuid(0));
            }

            return photos;
        }

        public async Task<Guid?> GetId(string hashtag)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id from hashtags where hashtag = @p_hashtag"
            };
            cmd.Parameters.AddWithValue("p_hashtag", hashtag);

            var id = await cmd.ExecuteScalarAsync();
            return (Guid?) id;
        }

        public async Task Add(string hashtag, Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into hashtags (id, hashtag) values(@p_id, @p_hashtag) ON CONFLICT DO NOTHING"
            };
            cmd.Parameters.AddWithValue("p_id", id);
            cmd.Parameters.AddWithValue("p_hashtag", hashtag);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddToPhoto(Guid photoId, Guid hashtagId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into photos_hashtags (photo_id, hashtag_id) values(@p_photo_id, @p_hashtag_id)"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);
            cmd.Parameters.AddWithValue("p_hashtag_id", hashtagId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveAll(Guid photoId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"delete from photos_hashtags where photo_id = @p_photo_id"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
