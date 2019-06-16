using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.ViewModels;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Services
{
    public class StoriesService : IStoriesService
    {
        private readonly IStoriesRepository _storiesRepository;
        private readonly IAmazonS3Repository _amazonS3Repository;
        private readonly IMapper _mapper;

        public StoriesService(IAmazonS3Repository amazonS3Repository, IMapper mapper, IStoriesRepository storiesRepository)
        {
            _amazonS3Repository = amazonS3Repository;
            _mapper = mapper;
            _storiesRepository = storiesRepository;
        }

        public async Task Upload(IFormFile photo, StoryView storyView)
        {
            var storyId = NewId.Next().ToGuid();
            await _amazonS3Repository.Upload(photo, storyId.ToString());

            var storyModel = _mapper.Map<Story>(storyView);
            storyModel.Id = storyId;
            storyModel.ImagePath = $"{Constants.ServiceURL}/{Constants.BucketName}/{storyId.ToString()}";
            await _storiesRepository.Upload(storyModel);
        }

        public async Task Delete(Guid userId, Guid storyId)
        {
            var storyDB = await _storiesRepository.GetById(storyId);
            if (storyDB == null) throw new ArgumentException("Story not found");
            if (storyDB.UserId != userId) throw new ArgumentException("Story doesn't belong to user");

            await _amazonS3Repository.Delete(storyId.ToString());
            await _storiesRepository.Delete(storyId);
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
