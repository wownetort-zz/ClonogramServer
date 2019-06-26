using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.ViewModels;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Clonogram.Services
{
    public class PhotosService : IPhotosService
    {
        private readonly IPhotosRepository _photosRepository;
        private readonly IAmazonS3Repository _amazonS3Repository;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFeedService _feedService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public PhotosService(IPhotosRepository photosRepository, IAmazonS3Repository amazonS3Repository, IHashtagsService hashtagsService, IMapper mapper, IFeedService feedService, IMemoryCache memoryCache)
        {
            _photosRepository = photosRepository;
            _amazonS3Repository = amazonS3Repository;
            _hashtagsService = hashtagsService;
            _mapper = mapper;
            _feedService = feedService;
            _memoryCache = memoryCache;
        }

        public async Task Upload(IFormFile photo, PhotoView photoView)
        {
            var photoId = NewId.Next().ToGuid();
            await _amazonS3Repository.Upload(photo, photoId.ToString());

            var photoModel = _mapper.Map<Photo>(photoView);
            photoModel.Id = photoId;
            photoModel.ImagePath = $"{Constants.ServiceURL}/{Constants.BucketName}/{photoId.ToString()}";
            photoModel.DateCreated = DateTime.Now;

            await Task.WhenAll(_photosRepository.Upload(photoModel),
                _hashtagsService.AddNewHashtags(photoId, photoModel.Description),
                _feedService.AddPhotoToFeed(photoModel.UserId, photoModel));
            _memoryCache.Set(photoId, photoModel, Cache.Photo);

            var photosKey = $"{photoModel.UserId} Photos";
            if (_memoryCache.TryGetValue(photosKey, out List<RedisPhoto> photos))
            {
                photos.Insert(0, new RedisPhoto
                {
                    Id = photoModel .Id,
                    Time = photoModel .DateCreated
                });
                _memoryCache.Set(photosKey, photos, Cache.UserPhotos);
            }
        }

        public async Task Delete(Guid userId, Guid photoId)
        {
            var photoDB = await _photosRepository.GetById(photoId);
            if (photoDB == null) throw new ArgumentException("Photo not found");
            if (photoDB.UserId != userId) throw new ArgumentException("Photo doesn't belong to user");

            await Task.WhenAll(_photosRepository.Delete(photoId),
                _feedService.DeletePhotoFromFeed(userId, photoDB));

            _memoryCache.Remove(photoId);
            var photosKey = $"{userId} Photos";
            if (_memoryCache.TryGetValue(photosKey, out List<RedisPhoto> photos))
            {
                photos.RemoveAt(photos.FindIndex(x => x.Id == photoId));
                _memoryCache.Set(photosKey, photos, Cache.UserPhotos);
            }
        }

        public async Task<PhotoView> GetById(Guid id)
        {
            var photo = await _memoryCache.GetOrCreateAsync(id, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.Photo;
                return await _photosRepository.GetById(id);
            });

            var photoView = _mapper.Map<PhotoView>(photo);
            return photoView;
        }

        public async Task<List<RedisPhoto>> GetAllPhotos(Guid userId)
        {
            return await _memoryCache.GetOrCreateAsync($"{userId} Photos", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.UserPhotos;
                return await _photosRepository.GetAllPhotos(userId);
            });
        }

        public async Task Update(PhotoView photoView)
        {
            var photo = _mapper.Map<Photo>(photoView);

            var photoDB = await _photosRepository.GetById(photo.Id);
            if (photoDB == null) throw new ArgumentException("Photo not found");

            if (photoDB.UserId != photo.UserId)
            {
                throw new ArgumentException("Photo doesn't belong to user");
            }

            photoDB.Description = photo.Description;

            await Task.WhenAll(_photosRepository.Update(photoDB),
                _hashtagsService.AddNewHashtags(photoDB.Id, photoDB.Description));
            _memoryCache.Set(photoDB.Id, photoDB, Cache.Photo);
        }

        public async Task Like(Guid userId, Guid photoId)
        {
            await _photosRepository.Like(userId, photoId);
            var likesKey = $"{photoId} Likes";
            if (_memoryCache.TryGetValue(likesKey, out int likes))
            {
                likes++;
                _memoryCache.Set(likesKey, likes, Cache.Likes);
            }
        }

        public async Task RemoveLike(Guid userId, Guid photoId)
        {
            await _photosRepository.RemoveLike(userId, photoId);
            var likesKey = $"{photoId} Likes";
            if (_memoryCache.TryGetValue(likesKey, out int likes))
            {
                likes--;
                _memoryCache.Set(likesKey, likes, Cache.Likes);
            }
        }

        public async Task<int> GetLikesCount(Guid photoId)
        {
            return await _memoryCache.GetOrCreateAsync($"{photoId} Likes", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.Likes;
                return await _photosRepository.GetLikesCount(photoId);
            });
        }
    }
}
