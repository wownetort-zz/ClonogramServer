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
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ICryptographyService _cryptographyService;
        private readonly IFeedService _feedService;
        private readonly IAmazonS3Repository _amazonS3Repository;
        private readonly IMapper _mapper;

        public UsersService(IUsersRepository usersRepository, ICryptographyService cryptographyService, IMapper mapper, IAmazonS3Repository amazonS3Repository, IFeedService feedService)
        {
            _usersRepository = usersRepository;
            _cryptographyService = cryptographyService;
            _mapper = mapper;
            _amazonS3Repository = amazonS3Repository;
            _feedService = feedService;
        }

        public async Task<UserView> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;

            var user = await _usersRepository.GetUserByName(username);
            if (user == null)
            {
                user = await _usersRepository.GetUserByEmail(username);
                if (user == null) return null;
            }

            if (!_cryptographyService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) return null;

            var userView = _mapper.Map<UserView>(user);
            return userView;
        }

        public async Task<IEnumerable<Guid>> GetAllUsersByName(string name)
        {
            if (name.Length < 3) throw new ArgumentException("Name length < 3");

            return await _usersRepository.GetAllUsersByName(name);
        }

        public async Task<List<Guid>> GetAllSubscribers(Guid userId)
        {
            return await _usersRepository.GetAllSubscribers(userId);
        }

        public async Task<List<Guid>> GetAllSubscriptions(Guid userId)
        {
            return await _usersRepository.GetAllSubscriptions(userId);
        }

        public async Task<UserView> GetById(Guid id)
        {
            var user = await _usersRepository.GetUserById(id);

            var userView = _mapper.Map<UserView>(user);
            return userView;
        }

        public async Task Create(UserView userView, IFormFile avatar = null)
        {
            if (string.IsNullOrWhiteSpace(userView.Password)) throw new ArgumentException("Password is required");
            if (userView.Username.Length < 3) throw new ArgumentException("Username length < 3");
            if (await _usersRepository.GetUserByName(userView.Username) != null) throw new ArgumentException("Username \"" + userView.Username + "\" is already taken");
            if (await _usersRepository.GetUserByEmail(userView.Email) != null) throw new ArgumentException("Email \"" + userView.Email + "\" is already taken");

            _cryptographyService.CreatePasswordHash(userView.Password, out var passwordHash, out var passwordSalt);

            userView.Id = NewId.Next().ToGuid().ToString();
            var user = _mapper.Map<User>(userView);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            if (avatar != null)
            {
                await _amazonS3Repository.Upload(avatar, userView.Id);
                user.AvatarPath = $"{Constants.ServiceURL}/{Constants.BucketName}/{userView.Id}";
            }

            await _usersRepository.AddUser(user);
        }

        public async Task Update(UserView userView, IFormFile avatar = null)
        {
            var user = _mapper.Map<User>(userView);
            var userDB = await _usersRepository.GetUserById(user.Id);

            if (userDB == null) throw new ArgumentException("User not found");

            if (userDB.Username != user.Username)
            {
                if (await _usersRepository.GetUserByName(user.Username) != null) throw new ArgumentException("Username \"" + user.Username + "\" is already taken");
            }
            if (userDB.Email != user.Email)
            {
                if (await _usersRepository.GetUserByEmail(user.Email) != null) throw new ArgumentException("Email \"" + user.Email + "\" is already taken");
            }

            if (!string.IsNullOrWhiteSpace(userView.FirstName)) userDB.FirstName = userView.FirstName;
            if (!string.IsNullOrWhiteSpace(userView.LastName)) userDB.LastName = userView.LastName;
            if (!string.IsNullOrWhiteSpace(userView.Username)) userDB.Username = userView.Username;
            userDB.Description = userView.Description;
            if (!string.IsNullOrWhiteSpace(userView.Email)) userDB.Email = userView.Email;

            if (!string.IsNullOrWhiteSpace(userView.Password))
            {
                _cryptographyService.CreatePasswordHash(userView.Password, out var passwordHash, out var passwordSalt);
                userDB.PasswordHash = passwordHash;
                userDB.PasswordSalt = passwordSalt;
            }

            if (avatar != null)
            {
                if (!string.IsNullOrWhiteSpace(userDB.AvatarPath))
                {
                    await _amazonS3Repository.Delete(userView.Id);
                }
                await _amazonS3Repository.Upload(avatar, userView.Id);
                userDB.AvatarPath = $"{Constants.ServiceURL}/{Constants.BucketName}/{userView.Id}";
            }

            await _usersRepository.UpdateUser(userDB);
        }

        public async Task Delete(Guid id)
        {
            await _usersRepository.DeleteUserById(id);
        }

        public async Task Subscribe(Guid userId, Guid secondaryUserId)
        {
            var secondaryUser = await _usersRepository.GetUserById(secondaryUserId);
            if (secondaryUser == null) throw new ArgumentException("User not found");

            await Task.WhenAll(_usersRepository.Subscribe(userId, secondaryUserId), 
                _feedService.AddAllUsersPhotoToFeed(userId, secondaryUserId));
        }

        public async Task Unsubscribe(Guid userId, Guid secondaryUserId)
        {
            var secondaryUser = await _usersRepository.GetUserById(secondaryUserId);
            if (secondaryUser == null) throw new ArgumentException("User not found");

            await Task.WhenAll(_usersRepository.Unsubscribe(userId, secondaryUserId),
                _feedService.RemoveAllUsersPhotoFromFeed(userId, secondaryUserId));
        }
    }
}