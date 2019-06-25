using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.ViewModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace Clonogram.Services
{
    public class CommentsService : ICommentsService
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IMapper _mapper;
        private readonly IPhotosRepository _photosRepository;
        private readonly IMemoryCache _memoryCache;

        public CommentsService(ICommentsRepository commentsRepository, IMapper mapper, IPhotosRepository photosRepository, IMemoryCache memoryCache)
        {
            _commentsRepository = commentsRepository;
            _mapper = mapper;
            _photosRepository = photosRepository;
            _memoryCache = memoryCache;
        }

        public async Task Create(CommentView commentView)
        {
            if (string.IsNullOrWhiteSpace(commentView.Text)) throw new ArgumentException("Comment is empty");
            var commentId = NewId.Next().ToGuid();

            var comment = _mapper.Map<Comment>(commentView);
            comment.Id = commentId;
            await _commentsRepository.Create(comment);

            _memoryCache.Set(comment.Id, comment, Cache.Comment);

            var commentsKey = $"{comment.PhotoId.ToString()} Comments";
            if (_memoryCache.TryGetValue(commentsKey, out List<Guid> comments))
            {
                comments.Add(commentId);
                _memoryCache.Set(commentsKey, comments, Cache.Comments);
            }
        }

        public async Task<CommentView> GetById(Guid id)
        {
            var cacheComment = await _memoryCache.GetOrCreateAsync(id, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.Comment;
                return await _commentsRepository.GetById(id);
            });

            return _mapper.Map<CommentView>(cacheComment);
        }

        public async Task<List<Guid>> GetAllPhotosComments(Guid photoId)
        {
            return await _memoryCache.GetOrCreateAsync($"{photoId.ToString()} Comments", async x =>
            {
                x.AbsoluteExpirationRelativeToNow = Cache.Comments;
                return await _commentsRepository.GetAllPhotosComments(photoId);
            });
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
            _memoryCache.Set(commentDB.Id, commentDB, Cache.Comment);
        }

        public async Task DeleteMy(Guid userId, Guid commentId)
        {
            var commentDB = await _commentsRepository.GetById(commentId);
            if (commentDB == null) throw new ArgumentException("Comment not found");
            if (commentDB.UserId != userId) throw new ArgumentException("Comment doesn't belong to user");
            await _commentsRepository.Delete(commentId);

            RemoveCommentFromCache(commentDB);
        }

        public async Task DeleteOnMyPhoto(Guid userId, Guid commentId)
        {
            var commentDB = await _commentsRepository.GetById(commentId);
            if (commentDB == null) throw new ArgumentException("Comment not found");

            var photo = await _photosRepository.GetById(commentDB.PhotoId);
            if (photo == null) throw new ArgumentException("Photo not found");
            if (photo.UserId != userId) throw new ArgumentException("Photo doesn't belong to user");

            await _commentsRepository.Delete(commentId);

            RemoveCommentFromCache(commentDB);
        }

        private void RemoveCommentFromCache(Comment commentDB)
        {
            _memoryCache.Remove(commentDB.Id);
            var commentsKey = $"{commentDB.PhotoId.ToString()} Comments";
            if (_memoryCache.TryGetValue(commentsKey, out List<Guid> comments))
            {
                comments.Remove(commentDB.Id);
                _memoryCache.Set(commentsKey, comments, Cache.Comments);
            }
        }
    }
}
