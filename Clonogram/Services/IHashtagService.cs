using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clonogram.Services
{
    public interface IHashtagService
    {
        Task<List<Guid>> GetPhotos(string hashtag);
        Task AddNewHashtags(Guid photoId, string text);
        IEnumerable<string> Parse(string text);
        Task Add(Guid photoId, string hashTag);
        Task RemoveAll(Guid photoId);
    }
}
