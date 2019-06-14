using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clonogram.Repositories
{
    public interface IHashtagRepository
    {
        Task<List<Guid>> GetPhotos(Guid hashtagId);
        Task<Guid?> GetId(string hashtag);
        Task Add(string hashtag, Guid id);
        Task AddToPhoto(Guid photoId, Guid hashtagId);
        Task RemoveAll(Guid photoId);
    }
}
