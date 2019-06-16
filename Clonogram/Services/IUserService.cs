using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.ViewModels;

namespace Clonogram.Services
{
    public interface IUserService
    {
        Task<UserView> Authenticate(string username, string password);
        Task<IEnumerable<string>> GetAllUsernames();
        Task<UserView> GetById(Guid id);
        Task Create(UserView userView);
        Task Update(UserView userView);
        Task Delete(Guid id);
        Task Subscribe(Guid userId, Guid secondaryUserId);
    }
}