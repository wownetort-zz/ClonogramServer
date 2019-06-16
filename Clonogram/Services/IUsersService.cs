using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.ViewModels;

namespace Clonogram.Services
{
    public interface IUsersService
    {
        Task<UserView> Authenticate(string username, string password);
        Task<IEnumerable<Guid>> GetAllUsersByName(string name);
        Task<List<Guid>> GetAllSubscribers(Guid userId);
        Task<List<Guid>> GetAllSubscriptions(Guid userId);
        Task<UserView> GetById(Guid id);
        Task Create(UserView userView);
        Task Update(UserView userView);
        Task Delete(Guid id);
        Task Subscribe(Guid userId, Guid secondaryUserId);
    }
}