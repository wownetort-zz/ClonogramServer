using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Helpers;
using Clonogram.Models;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace Clonogram.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly IMemoryCache _memoryCache;

        public UsersRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<User> GetUserByName(string username)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, avatar_path, username, email, password_hash, password_salt, first_name, last_name, description from users where username = @p_username"
            };
            cmd.Parameters.AddWithValue("p_username", username);
            var reader = await cmd.ExecuteReaderAsync();

            var next = await reader.ReadAsync();
            return next ? DataReaderMappers.MapToUser(reader) : null;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, avatar_path, username, email, password_hash, password_salt, first_name, last_name, description from users where email = @p_email"
            };
            cmd.Parameters.AddWithValue("p_email", email);
            var reader = await cmd.ExecuteReaderAsync();

            var next = await reader.ReadAsync();
            return next ? DataReaderMappers.MapToUser(reader) : null;
        }

        public async Task<List<Guid>> GetAllUsersByName(string name)
        {
            return await _memoryCache.GetOrCreateAsync($"{name} name", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.Users;
                return await GetUsersByName(name);
            });
        }

        private static async Task<List<Guid>> GetUsersByName(string name)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn, CommandText = @"select id from users where username like @p_name and deleted = false"
            };
            cmd.Parameters.AddWithValue("p_name", $"%{name}%");
            var reader = await cmd.ExecuteReaderAsync();

            var users = new List<Guid>();
            while (await reader.ReadAsync())
            {
                users.Add(reader.GetGuid(0));
            }

            return users;
        }

        public async Task<List<Guid>> GetAllSubscribers(Guid userId)
        {
            return await _memoryCache.GetOrCreateAsync($"{userId} subscribers", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.UserSubscribers;
                return await GetSubscribers(userId);
            });
        }

        private static async Task<List<Guid>> GetSubscribers(Guid userId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"select main_user_id from users_relations where secondary_user_id = @p_user_id and status = 1"
            };
            cmd.Parameters.AddWithValue("p_user_id", userId);
            var reader = await cmd.ExecuteReaderAsync();

            var users = new List<Guid>();
            while (await reader.ReadAsync())
            {
                users.Add(reader.GetGuid(0));
            }

            return users;
        }

        public async Task<List<Guid>> GetAllSubscriptions(Guid userId)
        {
            return await _memoryCache.GetOrCreateAsync($"{userId} subscriptions", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.UserSubscriptions;
                return await GetSubscriptions(userId);
            });
        }

        private static async Task<List<Guid>> GetSubscriptions(Guid userId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText = @"select secondary_user_id from users_relations where main_user_id = @p_user_id and status = 1"
            };
            cmd.Parameters.AddWithValue("p_user_id", userId);
            var reader = await cmd.ExecuteReaderAsync();

            var users = new List<Guid>();
            while (await reader.ReadAsync())
            {
                users.Add(reader.GetGuid(0));
            }

            return users;
        }

        public async Task<User> GetUserById(Guid id)
        {
            return await _memoryCache.GetOrCreateAsync(id, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.User;
                return await GetUserByGuid(id);
            });
        }

        private static async Task<User> GetUserByGuid(Guid id)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"select id, avatar_path, username, email, password_hash, password_salt, first_name, last_name, description from users where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);
            var reader = await cmd.ExecuteReaderAsync();

            var next = await reader.ReadAsync();
            return next ? DataReaderMappers.MapToUser(reader) : null;
        }

        public async Task AddUser(User user)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into users (id, avatar_path, username, email, password_hash, password_salt, first_name, last_name, description, date_updated, date_created)
                                        values(@p_id, @p_avatar_path, @p_username, @p_email, @p_password_hash, @p_password_salt, @p_first_name, @p_last_name, @p_description, @p_date_updated, @p_date_created)"
            };
            cmd.Parameters.AddWithValue("p_id", user.Id);
            cmd.Parameters.AddWithValue("p_avatar_path", user.AvatarPath);
            cmd.Parameters.AddWithValue("p_username", user.Username);
            cmd.Parameters.AddWithValue("p_password_hash", user.PasswordHash);
            cmd.Parameters.AddWithValue("p_password_salt", user.PasswordSalt);
            cmd.Parameters.AddWithValue("p_email", user.Email);
            cmd.Parameters.AddWithValue("p_first_name", user.FirstName);
            cmd.Parameters.AddWithValue("p_last_name", user.LastName);
            cmd.Parameters.AddWithValue("p_description", user.Description);
            cmd.Parameters.AddWithValue("p_date_updated", DateTime.Now);
            cmd.Parameters.AddWithValue("p_date_created", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();
            _memoryCache.Set(user.Id, user, Cache.User);
        }

        public async Task UpdateUser(User user)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"update users set username = @p_username, avatar_path = @p_avatar_path, email = @p_email, password_hash = @p_password_hash, password_salt = @p_password_salt, 
                        first_name = @p_first_name, last_name = @p_last_name, description = @p_description, date_updated = @p_date_updated
                        where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", user.Id);
            cmd.Parameters.AddWithValue("p_avatar_path", user.AvatarPath);
            cmd.Parameters.AddWithValue("p_username", user.Username);
            cmd.Parameters.AddWithValue("p_password_hash", user.PasswordHash);
            cmd.Parameters.AddWithValue("p_password_salt", user.PasswordSalt);
            cmd.Parameters.AddWithValue("p_email", user.Email);
            cmd.Parameters.AddWithValue("p_first_name", user.FirstName);
            cmd.Parameters.AddWithValue("p_last_name", user.LastName);
            cmd.Parameters.AddWithValue("p_description", user.Description);
            cmd.Parameters.AddWithValue("p_date_updated", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();
            _memoryCache.Set(user.Id, user, Cache.User);
        }

        public async Task DeleteUserById(Guid id)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn, CommandText = @"update users set deleted = true where id = @p_id"
            };
            cmd.Parameters.AddWithValue("p_id", id);

            await cmd.ExecuteNonQueryAsync();
            _memoryCache.Remove(id);
        }

        public async Task Subscribe(Guid userId, Guid secondaryUserId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"insert into users_relations (main_user_id, secondary_user_id, status) values(@p_main_user_id, @p_secondary_user_id, @p_status)"
            };
            cmd.Parameters.AddWithValue("p_main_user_id", userId);
            cmd.Parameters.AddWithValue("p_secondary_user_id", secondaryUserId);
            cmd.Parameters.AddWithValue("p_status", 1);

            await cmd.ExecuteNonQueryAsync();

            var subscribersKey = $"{secondaryUserId} subscribers";
            if (_memoryCache.TryGetValue(subscribersKey, out List<Guid> subscribers))
            {
                subscribers.Add(userId);
                _memoryCache.Set(subscribersKey, subscribers, Cache.UserSubscribers);
            }
            var subscriptionsKey = $"{userId} subscriptions";
            if (_memoryCache.TryGetValue(subscriptionsKey, out List<Guid> subscriptions))
            {
                subscriptions.Add(secondaryUserId);
                _memoryCache.Set(subscriptionsKey, subscriptions, Cache.UserSubscriptions);
            }
        }

        public async Task Unsubscribe(Guid userId, Guid secondaryUserId)
        {
            using var conn = new NpgsqlConnection(Constants.PostgresConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand
            {
                Connection = conn,
                CommandText =
                    @"delete from users_relations where main_user_id = @p_main_user_id and secondary_user_id = @p_secondary_user_id"
            };
            cmd.Parameters.AddWithValue("p_main_user_id", userId);
            cmd.Parameters.AddWithValue("p_secondary_user_id", secondaryUserId);

            await cmd.ExecuteNonQueryAsync();

            var subscribersKey = $"{secondaryUserId} subscribers";
            if (_memoryCache.TryGetValue(subscribersKey, out List<Guid> subscribers))
            {
                subscribers.Remove(userId);
                _memoryCache.Set(subscribersKey, subscribers, Cache.UserSubscribers);
            }
            var subscriptionsKey = $"{userId} subscriptions";
            if (_memoryCache.TryGetValue(subscriptionsKey, out List<Guid> subscriptions))
            {
                subscriptions.Remove(secondaryUserId);
                _memoryCache.Set(subscriptionsKey, subscriptions, Cache.UserSubscriptions);
            }
        }
    }
}
