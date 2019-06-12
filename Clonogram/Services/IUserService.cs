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
        Task<UserView> Create(string username, string password);
        Task Update(UserView userView);
        Task Delete(Guid id);
    }
}