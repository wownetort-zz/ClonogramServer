using System;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.ViewModels;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IAmazonS3Repository _amazonS3Repository;
        private readonly IHashtagService _hashtagService;
        private readonly IMapper _mapper;

        public PhotoService(IPhotoRepository photoRepository, IAmazonS3Repository amazonS3Repository, IHashtagService hashtagService, IMapper mapper)
        {
            _photoRepository = photoRepository;
            _amazonS3Repository = amazonS3Repository;
            _hashtagService = hashtagService;
            _mapper = mapper;
        }

        public async Task Upload(IFormFile photo, PhotoView photoView)
        {
            var photoId = NewId.Next().ToGuid();
            await _amazonS3Repository.Upload(photo, photoId.ToString());

            var photoModel = _mapper.Map<Photo>(photoView);
            photoModel.Id = photoId;
            photoModel.ImagePath = $"{Constants.ServiceURL}/{Constants.BucketName}/{photoId.ToString()}";
            await _photoRepository.Upload(photoModel);
            await _hashtagService.AddNewHashtags(photoId, photoModel.Description);
        }

        public async Task Delete(Guid userId, Guid photoId)
        {
            var photoDB = await _photoRepository.GetById(photoId);
            if (photoDB == null) throw new ArgumentException("Photo not found");
            if (photoDB.UserId != userId) throw new ArgumentException("Photo doesn't belong to user");
            await _photoRepository.Delete(photoId);
        }

        public async Task<PhotoView> GetById(Guid id)
        {
            var photo = await _photoRepository.GetById(id);
            var photoView = _mapper.Map<PhotoView>(photo);
            return photoView;
        }

        public async Task Update(PhotoView photoView)
        {
            var photo = _mapper.Map<Photo>(photoView);

            var photoDB = await _photoRepository.GetById(photo.Id);
            if (photoDB == null) throw new ArgumentException("Photo not found");

            if (photoDB.UserId != photo.UserId)
            {
                throw new ArgumentException("Photo doesn't belong to user");
            }

            photoDB.Description = photo.Description;
            await _photoRepository.Update(photoDB);
            await _hashtagService.AddNewHashtags(photoDB.Id, photoDB.Description);
        }
    }
}
