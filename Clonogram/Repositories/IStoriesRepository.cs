using System;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface IStoriesRepository
    {
        Task<Story> GetById(Guid id);
        Task Upload(Story story);
        Task Delete(Guid id);
    }
}
