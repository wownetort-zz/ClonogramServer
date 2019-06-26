using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.ViewModels;
using MassTransit;

namespace Clonogram.Services
{
    public class CommentsService : ICommentsService
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IMapper _mapper;
        private readonly IPhotosRepository _photosRepository;

        public CommentsService(ICommentsRepository commentsRepository, IMapper mapper, IPhotosRepository photosRepository)
        {
            _commentsRepository = commentsRepository;
            _mapper = mapper;
            _photosRepository = photosRepository;
        }

        public async Task Create(CommentView commentView)
        {
            if (string.IsNullOrWhiteSpace(commentView.Text)) throw new ArgumentException("Comment is empty");
            var commentId = NewId.Next().ToGuid();

            var comment = _mapper.Map<Comment>(commentView);
            comment.Id = commentId;
            await _commentsRepository.Create(comment);
        }

        public async Task<CommentView> GetById(Guid id)
        {
            var cacheComment = await _commentsRepository.GetById(id);
            return _mapper.Map<CommentView>(cacheComment);
        }

        public async Task<List<Guid>> GetAllPhotosComments(Guid photoId)
        {
            return await _commentsRepository.GetAllPhotosComments(photoId);
        }

        public async Task Update(CommentView commentView)
        {
            if (string.IsNullOrWhiteSpace(commentView.Text)) throw new ArgumentException("Comment is empty");

            var comment = _mapper.Map<Comment>(commentView);

            var commentDB = await _commentsRepository.GetById(comment.Id);
            if (commentDB == null) throw new ArgumentException("Comment not found");

            if (commentDB.UserId != comment.UserId)
            {
                throw new ArgumentException("Comment doesn't belong to user");
            }

            commentDB.Text = comment.Text;
            await _commentsRepository.Update(commentDB);
        }

        public async Task DeleteMy(Guid userId, Guid commentId)
        {
            var commentDB = await _commentsRepository.GetById(commentId);
            if (commentDB == null) throw new ArgumentException("Comment not found");
            if (commentDB.UserId != userId) throw new ArgumentException("Comment doesn't belong to user");
            await _commentsRepository.Delete(commentDB);
        }

        public async Task DeleteOnMyPhoto(Guid userId, Guid commentId)
        {
            var commentDB = await _commentsRepository.GetById(commentId);
            if (commentDB == null) throw new ArgumentException("Comment not found");

            var photo = await _photosRepository.GetById(commentDB.PhotoId);
            if (photo == null) throw new ArgumentException("Photo not found");
            if (photo.UserId != userId) throw new ArgumentException("Photo doesn't belong to user");

            await _commentsRepository.Delete(commentDB);
        }
    }
}
