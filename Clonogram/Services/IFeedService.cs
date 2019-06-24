using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clonogram.Services
{
    public interface IFeedService
    {
        Task AddPhotoToFeed(Guid userId, Guid photoId, DateTime created);
        Task AddStoryToFeed(Guid userId, Guid photoId, DateTime time);
        Task DeletePhotoFromFeed(Guid userId, Guid photoId, DateTime created);
        Task DeleteStoryFromFeed(Guid userId, Guid photoId, DateTime created);
        Task AddAllUsersPhotoToFeed(Guid userId, Guid subscriptionId);
        Task RemoveAllUsersPhotoFromFeed(Guid userId, Guid subscriptionId);
        Task<IEnumerable<Tuple<Guid, DateTime>>> GetFeed(Guid userId);
        Task<IEnumerable<Guid>> GetStoryFeed(Guid userId);
    }
}
