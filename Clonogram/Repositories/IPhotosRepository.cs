using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface IPhotosRepository
    {
        Task<Photo> GetById(Guid id);
        Task<List<Guid>> GetAllPhotos(Guid userId);
        Task Update(Photo photo);
        Task Upload(Photo photo);
        Task Delete(Guid id);
        Task Like(Guid userId, Guid photoId);
        Task RemoveLike(Guid userId, Guid photoId);
        Task<int> GetLikesCount(Guid photoId);
    }
}
