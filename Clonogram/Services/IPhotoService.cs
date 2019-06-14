using System;
using System.Threading.Tasks;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Services
{
    public interface IPhotoService
    {
        Task Upload(IFormFile photo, PhotoView photoView);
        Task Delete(Guid userId, Guid photoId);
        Task<PhotoView> GetById(Guid id);
        Task Update(PhotoView photoView);
        Task Like(Guid userId, Guid photoId);
        Task RemoveLike(Guid userId, Guid photoId);
        Task<int> GetLikesCount(Guid photoId);
    }
}
