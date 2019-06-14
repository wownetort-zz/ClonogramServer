using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Clonogram.Repositories;
using MassTransit;

namespace Clonogram.Services
{
    public class HashtagService : IHashtagService
    {
        private readonly IHashtagRepository _hashtagRepository;
        private readonly Regex _regex;

        public HashtagService(IHashtagRepository hashtagRepository)
        {
            _hashtagRepository = hashtagRepository;
            _regex = new Regex(@"(?<=#)\w+");
        }

        public async Task AddNewHashtags(Guid photoId, string text)
        {
            await RemoveAll(photoId);

            foreach (var hashtag in Parse(text))
            {
                await Add(photoId, hashtag);
            }
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
            var hashtagId = await _hashtagRepository.GetId(hashTag);
            if (hashtagId == null)
            {
                hashtagId = NewId.Next().ToGuid();
                await _hashtagRepository.Add(hashTag, hashtagId.Value);
                hashtagId = await _hashtagRepository.GetId(hashTag);
            }

            await _hashtagRepository.AddToPhoto(photoId, hashtagId.Value);
        }

        public async Task RemoveAll(Guid photoId)
        {
            await _hashtagRepository.RemoveAll(photoId);
        }
    }
}
