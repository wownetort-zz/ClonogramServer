using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Helpers;
using Clonogram.Models;
using Npgsql;

namespace Clonogram.Repositories
{
    public class CommentsRepository : ICommentsRepository
    {
        public async Task Create(Comment comment)
        {
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
        }

        public async Task<Comment> GetById(Guid id)
        {
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
        }

        public async Task Delete(Guid id)
        {
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"delete from comments where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
