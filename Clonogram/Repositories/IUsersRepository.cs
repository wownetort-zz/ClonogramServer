using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface IUsersRepository
    {
        Task<User> GetUserByName(string username);
        Task<User> GetUserByEmail(string email);
        Task<List<Guid>> GetAllUsersByName(string name);
        Task<List<Guid>> GetAllSubscribers(Guid userId);
        Task<List<Guid>> GetAllSubscriptions(Guid userId);
        Task<User> GetUserById(Guid id);
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteUserById(Guid id);
        Task Subscribe(Guid userId, Guid secondaryUserId);
        Task Unsubscribe(Guid userId, Guid secondaryUserId);
    }
}
