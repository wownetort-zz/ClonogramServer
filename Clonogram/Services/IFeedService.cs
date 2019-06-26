using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Services
{
    public interface IFeedService
    {
        Task AddPhotoToFeed(Guid userId, Photo photo);
        Task AddStoryToFeed(Guid userId, Story story);
        Task DeletePhotoFromFeed(Guid userId, Photo photo);
        Task DeleteStoryFromFeed(Guid userId, Story story);
        Task AddAllUsersPhotoToFeed(Guid userId, Guid subscriptionId);
        Task RemoveAllUsersPhotoFromFeed(Guid userId, Guid subscriptionId);
        Task<IEnumerable<RedisPhoto>> GetFeed(Guid userId);
        Task<IEnumerable<Guid>> GetStoryFeed(Guid userId);
    }
}
