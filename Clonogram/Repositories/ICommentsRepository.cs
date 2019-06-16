using System;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface ICommentsRepository
    {
        Task Create(Comment comment);
        Task<Comment> GetById(Guid id);

        Task Update(Comment comment);

        Task Delete(Guid id);
    }
}