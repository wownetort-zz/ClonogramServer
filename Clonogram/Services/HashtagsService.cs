using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Clonogram.Repositories;
using MassTransit;

namespace Clonogram.Services
{
    public class HashtagsService : IHashtagsService
    {
        private readonly IHashtagsRepository _hashtagsRepository;
        private readonly Regex _regex;

        public HashtagsService(IHashtagsRepository hashtagsRepository)
        {
            _hashtagsRepository = hashtagsRepository;
            _regex = new Regex(@"(?<=#)\w+");
        }

        public async Task<List<Guid>> GetPhotos(string hashtag)
        {
            var hashtagId = await _hashtagsRepository.GetId(hashtag);
            if (hashtagId == null) return new List<Guid>();

            return await _hashtagsRepository.GetPhotos(hashtagId.Value);
        }

        public async Task AddNewHashtags(Guid photoId, string text)
        {
            await RemoveAll(photoId);

            await Task.WhenAll(Parse(text).Select(x => Add(photoId, x)));
        }

        public IEnumerable<string> Parse(string text)
        {
            var matches = _regex.Matches(text);

            foreach (Match m in matches)
            {
                yield return m.Value;
            }
        }

        public async Task Add(Guid photoId, string hashTag)
        {
            var hashtagId = await _hashtagsRepository.GetId(hashTag);
            if (hashtagId == null)
            {
                hashtagId = NewId.Next().ToGuid();
                await _hashtagsRepository.Add(hashTag, hashtagId.Value);
                hashtagId = await _hashtagsRepository.GetId(hashTag);
            }

            await _hashtagsRepository.AddToPhoto(photoId, hashtagId.Value);
        }

        public async Task RemoveAll(Guid photoId)
        {
            await _hashtagsRepository.RemoveAll(photoId);
        }
    }
}
