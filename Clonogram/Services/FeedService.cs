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
        private readonly IPhotosRepository _photosRepository;

        public FeedService(IUsersService usersService, IRedisRepository redisRepository, IPhotosRepository photosRepository)
        {
            _usersService = usersService;
            _redisRepository = redisRepository;
            _photosRepository = photosRepository;
        }

        public async Task AddPhotoToFeed(Guid userId, Guid photoId, DateTime created)
        {
            var subscribers = await _usersService.GetAllSubscribers(userId);
            await Task.WhenAll(subscribers.Select(x => _redisRepository.AddFeedPhoto(x, photoId, created)));
        }

        public async Task AddStoryToFeed(Guid userId, Guid photoId, DateTime time)
        {
            await _redisRepository.AddFeedPhoto(userId, photoId, time);
        }

        public async Task DeletePhotoFromFeed(Guid userId, Guid photoId, DateTime created)
        {
            var subscribers = await _usersService.GetAllSubscribers(userId);
            await Task.WhenAll(subscribers.Select(x => _redisRepository.RemovePhotoFromFeed(x, photoId, created)));
        }

        public async Task DeleteStoryFromFeed(Guid userId, Guid photoId, DateTime created)
        {
            await _redisRepository.RemovePhotoFromFeed(userId, photoId, created);
        }

        public async Task AddAllUsersPhotoToFeed(Guid userId, Guid subscriptionId)
        {
            var photos = await _photosRepository.GetAllPhotos(subscriptionId);
            await _redisRepository.AddAllUsersPhotoToFeed(userId, photos);
        }

        public async Task RemoveAllUsersPhotoFromFeed(Guid userId, Guid subscriptionId)
        {
            var photos = await _photosRepository.GetAllPhotos(subscriptionId);
            await _redisRepository.RemoveAllUsersPhotoFromFeed(userId, photos);
        }

        public async Task<IEnumerable<Tuple<Guid, DateTime>>> GetFeed(Guid userId)
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
