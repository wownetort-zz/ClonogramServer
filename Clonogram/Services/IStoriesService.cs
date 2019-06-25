using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Services
{
    public interface IStoriesService
    {
        Task Upload(IFormFile photo, StoryView storyView);
        Task Delete(Guid userId, Guid storyId);
        //1day | push
        Task<StoryView> GetById(Guid id);
        //1min | push
        Task<List<Guid>> GetAllStories(Guid userId);
    }
}
