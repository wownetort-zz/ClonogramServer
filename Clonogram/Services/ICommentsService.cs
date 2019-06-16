using System;
using System.Threading.Tasks;
using Clonogram.ViewModels;

namespace Clonogram.Services
{
    public interface ICommentsService
    {
        Task Create(CommentView commentView);
        Task<CommentView> GetById(Guid id);

        Task Update(CommentView commentView);

        Task DeleteMy(Guid userId, Guid commentId);

        Task DeleteOnMyPhoto(Guid userId, Guid commentId);
    }
}
