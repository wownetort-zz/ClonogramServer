﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clonogram.Models;

namespace Clonogram.Repositories
{
    public interface IUsersRepository
    {
        Task<User> GetUserByName(string username);
        Task<List<string>> GetAllUsernames();
        Task<User> GetUserById(Guid id);
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteUserById(Guid id);
    }
}