using System;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface IPhotoRepository
    {
        Task<Photo> GetById(Guid id);
        Task Update(Photo photo);
        Task Upload(Photo photo);
        Task Delete(Guid id);
    }
}
