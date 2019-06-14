using System;
using System.Threading.Tasks;
using Npgsql;

namespace Clonogram.Repositories
{
    public class HashtagRepository : IHashtagRepository
    {
        public async Task<Guid?> GetId(string hashtag)
        {
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
