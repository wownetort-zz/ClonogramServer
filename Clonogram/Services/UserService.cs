using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Models;
using Clonogram.Repositories;
using Clonogram.ViewModels;
using MassTransit;

namespace Clonogram.Services
{
    public class UserService : IUserService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ICryptographyService _cryptographyService;
        private readonly IMapper _mapper;

        public UserService(IUsersRepository usersRepository, ICryptographyService cryptographyService, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _cryptographyService = cryptographyService;
            _mapper = mapper;
        }

        public async Task<UserView> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;

            var user = await _usersRepository.GetUserByName(username);

            if (user == null) return null;

            if (!_cryptographyService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) return null;

            var userView = _mapper.Map<UserView>(user);
            return userView;
        }

        public async Task<IEnumerable<string>> GetAllUsernames()
        {
            return await _usersRepository.GetAllUsernames();
        }

        public async Task<UserView> GetById(Guid id)
        {
            var user = await _usersRepository.GetUserById(id);
            var userView = _mapper.Map<UserView>(user);
            return userView;
        }

        public async Task<UserView> Create(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required");
            if (await _usersRepository.GetUserByName(username) != null) throw new ArgumentException("Username \"" + username + "\" is already taken");

            _cryptographyService.CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Id = NewId.Next().ToGuid()
            };
            await _usersRepository.AddUser(user);

            var userView = _mapper.Map<UserView>(user);
            return userView;
        }

        public async Task Update(UserView userView)
        {
            var user = await _usersRepository.GetUserByName(userView.Username);

            if (user == null) throw new ArgumentException("User not found");

            if (userView.Username != user.Username)
            {
                if (await _usersRepository.GetUserByName(userView.Username) != null) throw new ArgumentException("Username \"" + user.Username + "\" is already taken");
            }

            user.FirstName = userView.FirstName;
            user.LastName = userView.LastName;
            user.Username = userView.Username;
            user.Description = userView.Description;
            user.Email = userView.Email;

            if (!string.IsNullOrWhiteSpace(userView.Password))
            {
                _cryptographyService.CreatePasswordHash(userView.Password, out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            await _usersRepository.UpdateUser(user);
        }

        public async Task Delete(Guid id)
        {
            await _usersRepository.DeleteUserById(id);
        }
    }
}