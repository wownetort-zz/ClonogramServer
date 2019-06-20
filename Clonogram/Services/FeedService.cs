using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clonogram.Repositories;

namespace Clonogram.Services
{
    public class FeedService : IFeedService
    {
        private readonly IUsersService _usersService;
        private readonly IRedisRepository _redisRepository;

        public FeedService(IUsersService usersService, IRedisRepository redisRepository)
        {
            _usersService = usersService;
            _redisRepository = redisRepository;
        }

        public async Task AddPhotoToFeed(Guid userId, Guid photoId)
        {
            var subscribers = await _usersService.GetAllSubscribers(userId);
            await Task.WhenAll(subscribers.Select(x => _redisRepository.AddFeedPhoto(x, photoId)));
        }

        public async Task AddStoryToFeed(Guid userId, Guid photoId, DateTime time)
        {
            await _redisRepository.AddStoryFeedPhoto(userId, photoId, time);
        }

        public async Task<IEnumerable<Guid>> GetFeed(Guid userId)
        {
            return await _redisRepository.GetFeed(userId);
        }

        public async Task<IEnumerable<Guid>> GetStoryFeed(Guid userId)
        {
            var subscriptions = await _usersService.GetAllSubscriptions(userId);
            var guids = new ConcurrentBag<Guid>();
            await Task.WhenAll(subscriptions.AsParallel().Select(async x => (await _redisRepository.GetStoryFeed(x)).ForEach(y => guids.Add(y))));

            return guids;
        }
    }
}
