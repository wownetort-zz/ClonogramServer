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

        public async Task AddFeedPhoto(Guid userId, Guid photoId, DateTime created)
        {
            var conn = _connection.GetDatabase();
            var photo = JsonConvert.SerializeObject(new { id = photoId.ToString(), time = created });
            await conn.ListLeftPushAsync(userId.ToString(), photo);
        }

        public async Task AddAllUsersPhotoToFeed(Guid userId, List<Tuple<Guid, DateTime>> photos)
        {
            var conn = _connection.GetDatabase();
            var feedPhotos = (await GetFeed(userId)).ToList();

            int j = 0;
            foreach (var tuplePhoto in photos)
            {
                while (j < feedPhotos.Count && tuplePhoto.Item2 < feedPhotos[j].Item2)
                {
                    j++;
                }
                var newPhoto = JsonConvert.SerializeObject(new { id = tuplePhoto.Item1, time = tuplePhoto.Item2 });
                if (j == feedPhotos.Count)
                {
                    await conn.ListRightPushAsync(userId.ToString(), newPhoto);
                }
                else
                {
                    var photo = JsonConvert.SerializeObject(new {id = feedPhotos[j].Item1, time = feedPhotos[j].Item2});
                    await conn.ListInsertBeforeAsync(userId.ToString(), photo, newPhoto);
                }
            }
        }

        public async Task RemoveAllUsersPhotoFromFeed(Guid userId, List<Tuple<Guid, DateTime>> photos)
        {
            await Task.WhenAll(photos.Select(x => RemovePhotoFromFeed(userId, x.Item1, x.Item2)));
        }

        public async Task RemovePhotoFromFeed(Guid userId, Guid photoId, DateTime created)
        {
            var conn = _connection.GetDatabase();
            var photo = JsonConvert.SerializeObject(new { id = photoId.ToString(), time = created });
            await conn.ListRemoveAsync(userId.ToString(), photo);
        }

        public async Task<IEnumerable<Tuple<Guid, DateTime>>> GetFeed(Guid userId)
        {
            var conn = _connection.GetDatabase();
            var photos = await conn.ListRangeAsync(userId.ToString());

            return photos.Select(x => JsonConvert.DeserializeObject<Tuple<Guid, DateTime>>(x));
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
