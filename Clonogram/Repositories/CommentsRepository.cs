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
    public class CommentsRepository : ICommentsRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;
        private readonly ConnectionStrings _connectionStrings;

        public CommentsRepository(IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings, IOptions<ConnectionStrings> connectionStrings)
        {
            _memoryCache = memoryCache;
            _connectionStrings = connectionStrings.Value;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task Create(Comment comment)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into comments (id, user_id, photo_id, text, date_updated, date_created)
                                        values(@p_id, @p_user_id, @p_photo_id, @p_text, @p_date_updated, @p_date_created)"
            };
            cmd.Parameters.AddWithValue("p_id", comment.Id);
            cmd.Parameters.AddWithValue("p_user_id", comment.UserId);
            cmd.Parameters.AddWithValue("p_photo_id", comment.PhotoId);
            cmd.Parameters.AddWithValue("p_text", comment.Text);
            cmd.Parameters.AddWithValue("p_date_updated", DateTime.Now);
            cmd.Parameters.AddWithValue("p_date_created", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();

            _memoryCache.Set(comment.Id, comment, _cacheSettings.Comment);

            var commentsKey = $"{comment.PhotoId.ToString()} Comments";
            if (_memoryCache.TryGetValue(commentsKey, out List<Guid> comments))
            {
                comments.Add(comment.Id);
                _memoryCache.Set(commentsKey, comments, _cacheSettings.Comments);
            }
        }

        public async Task<Comment> GetById(Guid id)
        {
            return await _memoryCache.GetOrCreateAsync(id, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = _cacheSettings.Comment;
                return await GetCommentById(id);
            });
        }

        private async Task<Comment> GetCommentById(Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, user_id, photo_id, text, date_created from comments where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);
            var reader = await cmd.ExecuteReaderAsync();

            var next = await reader.ReadAsync();
            return next ? DataReaderMappers.MapToComment(reader) : null;
        }

        public async Task<List<Guid>> GetAllPhotosComments(Guid photoId)
        {
            return await _memoryCache.GetOrCreateAsync($"{photoId.ToString()} Comments", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = _cacheSettings.Comments;
                return await GetPhotosComments(photoId);
            });
        }

        private async Task<List<Guid>> GetPhotosComments(Guid photoId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"select id from comments where photo_id = @p_photo_id order by date_created"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);
            var reader = await cmd.ExecuteReaderAsync();

            var stories = new List<Guid>();
            while (await reader.ReadAsync())
            {
                stories.Add(reader.GetGuid(0));
            }

            return stories;
        }

        public async Task Update(Comment comment)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"update comments set text = @p_text, date_updated = @p_date_updated where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", comment.Id);
            cmd.Parameters.AddWithValue("p_text", comment.Text);
            cmd.Parameters.AddWithValue("p_date_updated", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();
            _memoryCache.Set(comment.Id, comment, _cacheSettings.Comment);
        }

        public async Task Delete(Comment comment)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"delete from comments where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", comment.Id);

            await cmd.ExecuteNonQueryAsync();

            _memoryCache.Remove(comment.Id);
            var commentsKey = $"{comment.PhotoId.ToString()} Comments";
            if (_memoryCache.TryGetValue(commentsKey, out List<Guid> comments))
            {
                comments.Remove(comment.Id);
                _memoryCache.Set(commentsKey, comments, _cacheSettings.Comments);
            }
        }
    }
}
