using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clonogram.Services
{
    public interface IHashtagsService
    {
        //30min
        Task<List<Guid>> GetPhotos(string hashtag);
        Task AddNewHashtags(Guid photoId, string text);
        IEnumerable<string> Parse(string text);
        Task Add(Guid photoId, string hashTag);
        Task RemoveAll(Guid photoId);
    }
}
