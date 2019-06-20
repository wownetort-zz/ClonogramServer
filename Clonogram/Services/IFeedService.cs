using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clonogram.Services
{
    public interface IFeedService
    {
        Task AddPhotoToFeed(Guid userId, Guid photoId);
        Task AddStoryToFeed(Guid userId, Guid photoId, DateTime time);
        Task<IEnumerable<Guid>> GetFeed(Guid userId);
        Task<IEnumerable<Guid>> GetStoryFeed(Guid userId);
    }
}
