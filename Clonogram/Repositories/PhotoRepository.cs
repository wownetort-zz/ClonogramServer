using System;
using System.Threading.Tasks;
using Clonogram.Helpers;
using Clonogram.Models;
using Npgsql;

namespace Clonogram.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        public async Task<Photo> GetById(Guid id)
        {
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, user_id, description, geo, image_path, image_size from photos where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);
            var reader = await cmd.ExecuteReaderAsync();

            var next = await reader.ReadAsync();
            return next ? DataReaderMappers.MapToPhoto(reader) : null;
        }

        public async Task Upload(Photo photo)
        {
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
            cmd.Parameters.AddWithValue("p_date_updated", DateTime.Now);
            cmd.Parameters.AddWithValue("p_date_created", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(Photo photo)
        {
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
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
            using var conn = new NpgsqlConnection(Constants.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"update photos set deleted = true where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
