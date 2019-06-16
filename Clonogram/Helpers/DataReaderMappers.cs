using System.Data.Common;
using Clonogram.Models;
using NpgsqlTypes;

namespace Clonogram.Helpers
{
    public static class DataReaderMappers
    {
        public static User MapToUser(DbDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Username = reader.GetString(reader.GetOrdinal("username")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PasswordHash = (byte[])reader["password_hash"],
                PasswordSalt = (byte[])reader["password_salt"],
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                AvatarPath = reader.GetString(reader.GetOrdinal("avatar_path")),
            };
        }

        public static Photo MapToPhoto(DbDataReader reader)
        {
            return new Photo
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Geo = (NpgsqlPoint)reader["geo"],
                ImageSize = reader.GetInt32(reader.GetOrdinal("image_size")),
                ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                DateCreated = reader.GetDateTime(reader.GetOrdinal("date_created")),
            };
        }

        public static Story MapToStory(DbDataReader reader)
        {
            return new Story
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                ImageSize = reader.GetInt32(reader.GetOrdinal("image_size")),
                ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                DateCreated = reader.GetDateTime(reader.GetOrdinal("date_created")),
            };
        }

        public static Comment MapToComment(DbDataReader reader)
        {
            return new Comment()
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                PhotoId = reader.GetGuid(reader.GetOrdinal("photo_id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Text = reader.GetString(reader.GetOrdinal("text")),
                DateCreated = reader.GetDateTime(reader.GetOrdinal("date_created")),
            };
        }
    }
}
