using System;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.ViewModels;
using MassTransit;

namespace Clonogram.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoRepository _photoRepository;

        public CommentService(ICommentRepository commentRepository, IMapper mapper, IPhotoRepository photoRepository)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _photoRepository = photoRepository;
        }

        public async Task Create(CommentView commentView)
        {
            var commentId = NewId.Next().ToGuid();

            var comment = _mapper.Map<Comment>(commentView);
            comment.Id = commentId;
            await _commentRepository.Create(comment);
        }

        public async Task<CommentView> GetById(Guid id)
        {
            var comment = await _commentRepository.GetById(id);
            var commentView = _mapper.Map<CommentView>(comment);
            return commentView;
        }

        public async Task Update(CommentView commentView)
        {
            var comment = _mapper.Map<Comment>(commentView);

            var commentDB = await _commentRepository.GetById(comment.Id);
            if (commentDB == null) throw new ArgumentException("Comment not found");

            if (commentDB.UserId != comment.UserId)
            {
                throw new ArgumentException("Comment doesn't belong to user");
            }

            commentDB.Text = comment.Text;
            await _commentRepository.Update(commentDB);
        }

        public async Task DeleteMy(Guid userId, Guid commentId)
        {
            var commentDB = await _commentRepository.GetById(commentId);
            if (commentDB == null) throw new ArgumentException("Comment not found");
            if (commentDB.UserId != userId) throw new ArgumentException("Comment doesn't belong to user");
            await _commentRepository.Delete(commentId);
        }

        public async Task DeleteOnMyPhoto(Guid userId, Guid commentId)
        {
            var commentDB = await _commentRepository.GetById(commentId);
            if (commentDB == null) throw new ArgumentException("Comment not found");

            var photo = await _photoRepository.GetById(commentDB.PhotoId);
            if (photo == null) throw new ArgumentException("Photo not found");
            if (photo.UserId != userId) throw new ArgumentException("Photo doesn't belong to user");

            await _commentRepository.Delete(commentId);
        }
    }
}
