using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface ICommentsRepository
    {
        Task Create(Comment comment);
        Task<Comment> GetById(Guid id);
        Task<List<Guid>> GetAllPhotosComments(Guid photoId);

        Task Update(Comment comment);

        Task Delete(Guid id);
    }
}