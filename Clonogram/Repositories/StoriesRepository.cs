using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Helpers;
using Clonogram.Models;
using Clonogram.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Clonogram.Repositories
{
    public class StoriesRepository : IStoriesRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;
        private readonly ConnectionStrings _connectionStrings;

        public StoriesRepository(IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings, IOptions<ConnectionStrings> connectionStrings)
        {
            _memoryCache = memoryCache;
            _connectionStrings = connectionStrings.Value;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<Story> GetById(Guid id)
        {
            return await _memoryCache.GetOrCreateAsync(id, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = _cacheSettings.Story;
                return await GetStoryById(id);
            });
        }

        private async Task<Story> GetStoryById(Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, user_id, image_path, image_size, date_created from stories where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);
            var reader = await cmd.ExecuteReaderAsync();

            var next = await reader.ReadAsync();
            return next ? DataReaderMappers.MapToStory(reader) : null;
        }

        public async Task<List<Guid>> GetAllStories(Guid userId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"select id from stories where user_id = @p_user_id and date_created > now() - interval '1' day order by date_created"
            };
            cmd.Parameters.AddWithValue("p_user_id", userId);
            var reader = await cmd.ExecuteReaderAsync();

            var stories = new List<Guid>();
            while (await reader.ReadAsync())
            {
                stories.Add(reader.GetGuid(0));
            }

            return stories;
        }

        public async Task Upload(Story story)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into stories (id, user_id, image_path, image_size, date_created)
                                        values(@p_id, @p_user_id, @p_image_path, @p_image_size, @p_date_created)"
            };
            cmd.Parameters.AddWithValue("p_id", story.Id);
            cmd.Parameters.AddWithValue("p_user_id", story.UserId);
            cmd.Parameters.AddWithValue("p_image_path", story.ImagePath);
            cmd.Parameters.AddWithValue("p_image_size", story.ImageSize);
            cmd.Parameters.AddWithValue("p_date_created", story.DateCreated);

            await cmd.ExecuteNonQueryAsync();
            _memoryCache.Set(story.Id, story, _cacheSettings.Story);
        }

        public async Task Delete(Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"delete from stories where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);

            await cmd.ExecuteNonQueryAsync();
            _memoryCache.Remove(id);
        }
    }
}
