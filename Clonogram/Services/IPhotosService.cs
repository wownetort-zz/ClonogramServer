using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Models;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Services
{
    public interface IPhotosService
    {
        Task Upload(IFormFile photo, PhotoView photoView);
        Task Delete(Guid userId, Guid photoId);
        //1min | push
        Task<PhotoView> GetById(Guid id);
        //1min | push
        Task<List<RedisPhoto>> GetAllPhotos(Guid userId);
        Task Update(PhotoView photoView);
        Task Like(Guid userId, Guid photoId);
        Task RemoveLike(Guid userId, Guid photoId);
        //1min
        Task<int> GetLikesCount(Guid photoId);
    }
}
