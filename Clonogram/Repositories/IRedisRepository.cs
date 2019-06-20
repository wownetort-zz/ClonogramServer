using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clonogram.Repositories
{
    public interface IRedisRepository
    {
        Task AddFeedPhoto(Guid userId, Guid photoId);
        Task AddStoryFeedPhoto(Guid userId, Guid photoId, DateTime created);
        Task RemovePhotoFromFeed(Guid userId, Guid photoId);
        Task RemoveStoryFromFeed(Guid userId, Guid photoId, DateTime created);
        Task<IEnumerable<Guid>> GetFeed(Guid userId);
        Task<List<Guid>> GetStoryFeed(Guid userId);
    }
}
