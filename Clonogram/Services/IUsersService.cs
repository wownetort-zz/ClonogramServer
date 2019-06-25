using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Services
{
    public interface IUsersService
    {
        Task<UserView> Authenticate(string username, string password);
        //1hour
        Task<IEnumerable<Guid>> GetAllUsersByName(string name);
        Task<List<Guid>> GetAllSubscribers(Guid userId);
        Task<List<Guid>> GetAllSubscriptions(Guid userId);
        //1min | push
        Task<UserView> GetById(Guid id);
        Task Create(UserView userView, IFormFile avatar = null);
        Task Update(UserView userView, IFormFile avatar = null);
        Task Delete(Guid id);
        Task Subscribe(Guid userId, Guid secondaryUserId);
        Task Unsubscribe(Guid userId, Guid secondaryUserId);
    }
}