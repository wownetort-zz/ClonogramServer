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
        Task<StoryView> GetById(Guid id);
        Task<List<Guid>> GetAllStories(Guid userId);
    }
}
