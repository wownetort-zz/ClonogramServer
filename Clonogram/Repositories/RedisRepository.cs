using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Clonogram.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly ConnectionMultiplexer _connection;
        public RedisRepository()
        {
            _connection = ConnectionMultiplexer.Connect(Constants.RedisConnectionString);
        }

        public async Task AddFeedPhoto(Guid userId, Guid photoId)
        {
            var conn = _connection.GetDatabase();
            await conn.ListLeftPushAsync(userId.ToString(), photoId.ToString());
        }

        public async Task AddStoryFeedPhoto(Guid userId, Guid photoId, DateTime created)
        {
            var conn = _connection.GetDatabase();
            var story = JsonConvert.SerializeObject(new {id = photoId.ToString(), time = created });
            await conn.ListLeftPushAsync(userId.ToString(), story);
        }

        public async Task RemovePhotoFromFeed(Guid userId, Guid photoId)
        {
            var conn = _connection.GetDatabase();
            await conn.ListRemoveAsync(userId.ToString(), photoId.ToString());
        }

        public async Task RemoveStoryFromFeed(Guid userId, Guid photoId, DateTime created)
        {
            var conn = _connection.GetDatabase();
            var story = JsonConvert.SerializeObject(new { id = photoId.ToString(), time = created });
            await conn.ListRemoveAsync(userId.ToString(), story);
        }

        public async Task<IEnumerable<Guid>> GetFeed(Guid userId)
        {
            var conn = _connection.GetDatabase();
            return (await conn.ListRangeAsync(userId.ToString())).Select(x => Guid.Parse(x.ToString()));
        }

        public async Task<List<Guid>> GetStoryFeed(Guid userId)
        {
            var conn = _connection.GetDatabase();
            var stories = await conn.ListRangeAsync(userId.ToString());
            var timeMax = DateTime.Now.AddDays(-1);
            var guids = new List<Guid>();

            var iterator = 0;
            for (;iterator < stories.Length; iterator++)
            {
                var (guid, time) = JsonConvert.DeserializeObject<Tuple<Guid, DateTime>>(stories[iterator]);
                if (time > timeMax)
                {
                    guids.Add(guid);
                }
                else
                {
                    break;
                }
            }

            if (iterator < stories.Length)
            {
                await conn.ListTrimAsync(userId.ToString(), 0, iterator);
            }

            return guids;
        }
    }
}
