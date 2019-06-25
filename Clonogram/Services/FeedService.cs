using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clonogram.Models;
using Clonogram.Repositories;

namespace Clonogram.Services
{
    public class FeedService : IFeedService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly IPhotosRepository _photosRepository;

        public FeedService(IRedisRepository redisRepository, IPhotosRepository photosRepository, IUsersRepository usersRepository)
        {
            _redisRepository = redisRepository;
            _photosRepository = photosRepository;
            _usersRepository = usersRepository;
        }

        public async Task AddPhotoToFeed(Guid userId, Photo photo)
        {
            var subscribers = await _usersRepository.GetAllSubscribers(userId);
            var redisPhoto = new RedisPhoto
            {
                Id = photo.Id,
                Time = photo.DateCreated
            };
            await Task.WhenAll(subscribers.Select(x => _redisRepository.AddFeedPhoto(x, redisPhoto)));
        }

        public async Task AddStoryToFeed(Guid userId, Story story)
        {
            var redisPhoto = new RedisPhoto
            {
                Id = story.Id,
                Time = story.DateCreated
            };
            await _redisRepository.AddFeedPhoto(userId, redisPhoto);
        }

        public async Task DeletePhotoFromFeed(Guid userId, Photo photo)
        {
            var subscribers = await _usersRepository.GetAllSubscribers(userId);
            var redisPhoto = new RedisPhoto
            {
                Id = photo.Id,
                Time = photo.DateCreated
            };
            await Task.WhenAll(subscribers.Select(x => _redisRepository.RemovePhotoFromFeed(x, redisPhoto)));
        }

        public async Task DeleteStoryFromFeed(Guid userId, Story story)
        {
            var redisPhoto = new RedisPhoto
            {
                Id = story.Id,
                Time = story.DateCreated
            };
            await _redisRepository.RemovePhotoFromFeed(userId, redisPhoto);
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

        public async Task<IEnumerable<RedisPhoto>> GetFeed(Guid userId)
        {
            return await _redisRepository.GetFeed(userId);
        }

        public async Task<IEnumerable<Guid>> GetStoryFeed(Guid userId)
        {
            var subscriptions = await _usersRepository.GetAllSubscriptions(userId);
            var guids = new ConcurrentBag<Guid>();
            await Task.WhenAll(subscriptions.AsParallel().Select(async x => (await _redisRepository.GetStoryFeed(x)).ForEach(y => guids.Add(y))));

            return guids;
        }
    }
}
