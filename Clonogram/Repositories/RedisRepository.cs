using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clonogram.Models;
using Clonogram.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Clonogram.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly ConnectionMultiplexer _connection;
        public RedisRepository(IOptions<ConnectionStrings> connectionStrings)
        {
            _connection = ConnectionMultiplexer.Connect(connectionStrings.Value.Redis);
        }

        public async Task AddFeedPhoto(Guid userId, RedisPhoto photo)
        {
            var conn = _connection.GetDatabase();
            var jsonPhoto = JsonConvert.SerializeObject(photo);
            await conn.ListLeftPushAsync(userId.ToString(), jsonPhoto);
        }

        public async Task AddAllUsersPhotoToFeed(Guid userId, List<RedisPhoto> photos)
        {
            var conn = _connection.GetDatabase();
            var feedPhotos = (await GetFeed(userId)).ToList();

            int j = 0;
            foreach (var tuplePhoto in photos)
            {
                while (j < feedPhotos.Count && tuplePhoto.Time < feedPhotos[j].Time)
                {
                    j++;
                }
                var newPhoto = JsonConvert.SerializeObject(tuplePhoto);
                if (j == feedPhotos.Count)
                {
                    await conn.ListRightPushAsync(userId.ToString(), newPhoto);
                }
                else
                {
                    var photo = JsonConvert.SerializeObject(feedPhotos);
                    await conn.ListInsertBeforeAsync(userId.ToString(), photo, newPhoto);
                }
            }
        }

        public async Task RemoveAllUsersPhotoFromFeed(Guid userId, List<RedisPhoto> photos)
        {
            await Task.WhenAll(photos.Select(x => RemovePhotoFromFeed(userId, x)));
        }

        public async Task RemovePhotoFromFeed(Guid userId, RedisPhoto photo)
        {
            var conn = _connection.GetDatabase();
            var jsonPhoto = JsonConvert.SerializeObject(photo);
            await conn.ListRemoveAsync(userId.ToString(), jsonPhoto);
        }

        public async Task<IEnumerable<RedisPhoto>> GetFeed(Guid userId)
        {
            var conn = _connection.GetDatabase();
            var photos = await conn.ListRangeAsync(userId.ToString());

            return photos.Select(x => JsonConvert.DeserializeObject<RedisPhoto>(x));
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
                var story = JsonConvert.DeserializeObject<RedisPhoto>(stories[iterator]);
                if (story.Time > timeMax)
                {
                    guids.Add(story.Id);
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
