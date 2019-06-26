using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.ViewModels;

namespace Clonogram.Services
{
    public interface ICommentsService
    {
        Task Create(CommentView commentView);
        Task<CommentView> GetById(Guid id);
        Task<List<Guid>> GetAllPhotosComments(Guid photoId);
        Task Update(CommentView commentView);
        Task DeleteMy(Guid userId, Guid commentId);
        Task DeleteOnMyPhoto(Guid userId, Guid commentId);
    }
}
