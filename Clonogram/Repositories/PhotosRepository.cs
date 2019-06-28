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
    public class PhotosRepository : IPhotosRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;
        private readonly ConnectionStrings _connectionStrings;

        public PhotosRepository(IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings, IOptions<ConnectionStrings> connectionStrings)
        {
            _memoryCache = memoryCache;
            _connectionStrings = connectionStrings.Value;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<Photo> GetById(Guid id)
        {
            return await _memoryCache.GetOrCreateAsync(id, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = _cacheSettings.Photo;
                return await GetPhotoById(id);
            });
        }

        private async Task<Photo> GetPhotoById(Guid id)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
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

        public async Task<List<RedisPhoto>> GetAllPhotos(Guid userId)
        {
            return await _memoryCache.GetOrCreateAsync($"{userId} Photos", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = _cacheSettings.UserPhotos;
                return await GetAllUserPhotos(userId);
            });
        }

        private async Task<List<RedisPhoto>> GetAllUserPhotos(Guid userId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, date_created from photos where user_id = @p_user_id and deleted = false order by date_created desc"
            };
            cmd.Parameters.AddWithValue("p_user_id", userId);
            var reader = await cmd.ExecuteReaderAsync();

            var photos = new List<RedisPhoto>();
            while (await reader.ReadAsync())
            {
                var photo = new RedisPhoto
                {
                    Id = reader.GetGuid(0),
                    Time = reader.GetDateTime(1)
                };
                photos.Add(photo);
            }

            return photos;
        }

        public async Task Upload(Photo photo)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
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

            _memoryCache.Set(photo.Id, photo, _cacheSettings.Photo);

            var photosKey = $"{photo.UserId} Photos";
            if (_memoryCache.TryGetValue(photosKey, out List<RedisPhoto> photos))
            {
                photos.Insert(0, new RedisPhoto
                {
                    Id = photo.Id,
                    Time = photo.DateCreated
                });
                _memoryCache.Set(photosKey, photos, _cacheSettings.UserPhotos);
            }
        }

        public async Task Update(Photo photo)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
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
            _memoryCache.Set(photo.Id, photo, _cacheSettings.Photo);
        }

        public async Task Delete(Photo photo)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"update photos set deleted = true where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", photo.Id);

            await cmd.ExecuteNonQueryAsync();

            _memoryCache.Remove(photo.Id);
            var photosKey = $"{photo.UserId} Photos";
            if (_memoryCache.TryGetValue(photosKey, out List<RedisPhoto> photos))
            {
                photos.RemoveAt(photos.FindIndex(x => x.Id == photo.Id));
                _memoryCache.Set(photosKey, photos, _cacheSettings.UserPhotos);
            }
        }

        public async Task Like(Guid userId, Guid photoId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into photos_likes (photo_id, user_id) values(@p_photo_id, @p_user_id) ON CONFLICT DO NOTHING"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);
            cmd.Parameters.AddWithValue("p_user_id", userId);

            if(await cmd.ExecuteNonQueryAsync() == 0) return;

            var likesKey = $"{photoId} Likes";
            if (_memoryCache.TryGetValue(likesKey, out int likes))
            {
                likes++;
                _memoryCache.Set(likesKey, likes, _cacheSettings.Likes);
            }
        }

        public async Task RemoveLike(Guid userId, Guid photoId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"delete from photos_likes where photo_id = @p_photo_id and user_id = @p_user_id"
            };
            cmd.Parameters.AddWithValue("p_photo_id", photoId);
            cmd.Parameters.AddWithValue("p_user_id", userId);

            if (await cmd.ExecuteNonQueryAsync() == 0) return;

            var likesKey = $"{photoId} Likes";
            if (_memoryCache.TryGetValue(likesKey, out int likes))
            {
                likes--;
                _memoryCache.Set(likesKey, likes, _cacheSettings.Likes);
            }
        }

        public async Task<int> GetLikesCount(Guid photoId)
        {
            return await _memoryCache.GetOrCreateAsync($"{photoId} Likes", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = _cacheSettings.Likes;
                return await GetPhotoLikesCount(photoId);
            });
        }

        private async Task<int> GetPhotoLikesCount(Guid photoId)
        {
            using var conn = new NpgsqlConnection(_connectionStrings.Postgres);
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
