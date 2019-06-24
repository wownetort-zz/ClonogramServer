using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clonogram.Repositories
{
    public interface IRedisRepository
    {
        Task AddFeedPhoto(Guid userId, Guid photoId, DateTime created);
        Task AddAllUsersPhotoToFeed(Guid userId, List<Tuple<Guid, DateTime>> photos);
        Task RemoveAllUsersPhotoFromFeed(Guid userId, List<Tuple<Guid, DateTime>> photos);
        Task RemovePhotoFromFeed(Guid userId, Guid photoId, DateTime created);
        Task<IEnumerable<Tuple<Guid, DateTime>>> GetFeed(Guid userId);
        Task<List<Guid>> GetStoryFeed(Guid userId);
    }
}
