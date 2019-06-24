using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Helpers;
using Clonogram.Models;
using Npgsql;

namespace Clonogram.Repositories
{
    public class PhotosRepository : IPhotosRepository
    {
        public async Task<Photo> GetById(Guid id)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, user_id, description, geo, image_path, image_size, date_created from photos where id = @p_id and deleted = false"
            };
            cmd.Parameters.AddWithValue("p_id", id);
            var reader = await cmd.ExecuteReaderAsync();

            var next = await reader.ReadAsync();
            return next ? DataReaderMappers.MapToPhoto(reader) : null;
        }

        public async Task<List<Tuple<Guid, DateTime>>> GetAllPhotos(Guid userId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"select id, date_created from photos where user_id = @p_user_id and deleted = false order by date_created desc"
            };
            cmd.Parameters.AddWithValue("p_user_id", userId);
            var reader = await cmd.ExecuteReaderAsync();

            var photos = new List<Tuple<Guid, DateTime>>();
            while (await reader.ReadAsync())
            {
                var photo = new Tuple<Guid, DateTime>(reader.GetGuid(0), reader.GetDateTime(1));
                photos.Add(photo);
            }

            return photos;
        }

        public async Task Upload(Photo photo)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into photos (id, user_id, description, geo, image_path, image_size, date_updated, date_created)
                                        values(@p_id, @p_user_id, @p_description, @p_geo, @p_image_path, @p_image_size, @p_date_updated, @p_date_created)"
            };
            cmd.Parameters.AddWithValue("p_id", photo.Id);
            cmd.Parameters.AddWithValue("p_user_id", photo.UserId);
            cmd.Parameters.AddWithValue("p_description", photo.Description);
            cmd.Parameters.AddWithValue("p_geo", photo.Geo);
            cmd.Parameters.AddWithValue("p_image_path", photo.ImagePath);
            cmd.Parameters.AddWithValue("p_image_size", photo.ImageSize);
            cmd.Parameters.AddWithValue("p_date_updated", photo.DateCreated);
            cmd.Parameters.AddWithValue("p_date_created", photo.DateCreated);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(Photo photo)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"update photos set description = @p_description, date_updated = @p_date_updated where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", photo.Id);
            cmd.Parameters.AddWithValue("p_description", photo.Description);
            cmd.Parameters.AddWithValue("p_date_updated", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(Guid id)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"update photos set deleted = true where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Like(Guid userId, Guid photoId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into photos_likes (photo_id, user_id) values(@p_photo_id, @p_user_id) ON CONFLICT DO NOTHING"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);
            cmd.Parameters.AddWithValue("p_user_id", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveLike(Guid userId, Guid photoId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"delete from photos_likes where photo_id = @p_photo_id and user_id = @p_user_id"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);
            cmd.Parameters.AddWithValue("p_user_id", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetLikesCount(Guid photoId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select count(*) from photos_likes where photo_id = @p_photo_id"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);

            var count = await cmd.ExecuteScalarAsync();
            return int.Parse(count.ToString());
        }
    }
}
