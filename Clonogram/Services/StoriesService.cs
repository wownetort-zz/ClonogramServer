using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.Settings;
using Clonogram.ViewModels;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Clonogram.Services
{
    public class StoriesService : IStoriesService
    {
        private readonly IStoriesRepository _storiesRepository;
        private readonly IAmazonS3Repository _amazonS3Repository;
        private readonly IFeedService _feedService;
        private readonly IMapper _mapper;
        private readonly S3Settings _s3Settings;

        public StoriesService(IAmazonS3Repository amazonS3Repository, IMapper mapper, IStoriesRepository storiesRepository, IFeedService feedService, IOptions<S3Settings> s3Settings)
        {
            _amazonS3Repository = amazonS3Repository;
            _mapper = mapper;
            _storiesRepository = storiesRepository;
            _feedService = feedService;
            _s3Settings = s3Settings.Value;
        }

        public async Task Upload(IFormFile photo, StoryView storyView)
        {
            var storyId = NewId.Next().ToGuid();

            var storyModel = _mapper.Map<Story>(storyView);
            storyModel.Id = storyId;
            storyModel.ImagePath = $"{_s3Settings.ServiceURL}/{_s3Settings.BucketName}/{storyId.ToString()}";
            storyModel.DateCreated = DateTime.Now;

            await Task.WhenAll(_storiesRepository.Upload(storyModel),
                _feedService.AddStoryToFeed(storyModel.UserId, storyModel), 
                _amazonS3Repository.Upload(photo, storyId.ToString()));
        }

        public async Task Delete(Guid userId, Guid storyId)
        {
            var storyDB = await _storiesRepository.GetById(storyId);
            if (storyDB == null) throw new ArgumentException("Story not found");
            if (storyDB.UserId != userId) throw new ArgumentException("Story doesn't belong to user");

            await Task.WhenAll(_amazonS3Repository.Delete(storyId.ToString()), 
                _storiesRepository.Delete(storyId),
                _feedService.DeleteStoryFromFeed(userId, storyDB));
        }

        public async Task<StoryView> GetById(Guid id)
        {
            var story = await _storiesRepository.GetById(id);

            var storyView = _mapper.Map<StoryView>(story);
            return storyView;
        }

        public async Task<List<Guid>> GetAllStories(Guid userId)
        {
            return await _storiesRepository.GetAllStories(userId);
        }
    }
}
