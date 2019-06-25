using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface IRedisRepository
    {
        Task AddFeedPhoto(Guid userId, RedisPhoto photo);
        Task AddAllUsersPhotoToFeed(Guid userId, List<RedisPhoto> photos);
        Task RemoveAllUsersPhotoFromFeed(Guid userId, List<RedisPhoto> photos);
        Task RemovePhotoFromFeed(Guid userId, RedisPhoto photo);
        Task<IEnumerable<RedisPhoto>> GetFeed(Guid userId);
        Task<List<Guid>> GetStoryFeed(Guid userId);
    }
}
