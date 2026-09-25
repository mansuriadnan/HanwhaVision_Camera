using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class ExposeApiUserService : IExposeApiUserService
    {
        private readonly IExposeApiUserRepository _exposeApiUserRepository;

        public ExposeApiUserService(IExposeApiUserRepository exposeApiUserRepository)
        {
            _exposeApiUserRepository = exposeApiUserRepository;
        }

        public async Task<StandardAPIResponse<ExposeApiUserDto>> AddOrUpdateUserAsync(ExposeApiUserDto userDto, string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userDto.Username))
                {
                    return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, "Username is required.");
                }

                bool isUsernameExists = await _exposeApiUserRepository.IsUsernameExistsAsync(userDto.Username, userDto.Id);
                if (isUsernameExists)
                {
                    return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, "Username already exists.");
                }

                if (string.IsNullOrEmpty(userDto.Id)) // Add
                {
                    var newUser = new ExposeApiUser
                    {
                        Username = userDto.Username,
                        Password = userDto.Password,
                        Role = userDto.Role,
                        IsActive = userDto.IsActive,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = currentUserId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = currentUserId,
                        IsDeleted = false
                    };

                    userDto.Id = await _exposeApiUserRepository.InsertAsync(newUser);
                    return StandardAPIResponse<ExposeApiUserDto>.SuccessResponse(userDto, "User created successfully.");
                }
                else // Update
                {
                    var existingUser = await _exposeApiUserRepository.GetAsync(userDto.Id);
                    if (existingUser == null || existingUser.IsDeleted == true)
                    {
                        return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, "User not found.");
                    }

                    existingUser.Username = userDto.Username;
                    existingUser.Password = userDto.Password;
                    existingUser.Role = userDto.Role;
                    existingUser.IsActive = userDto.IsActive;
                    existingUser.UpdatedOn = DateTime.UtcNow;
                    existingUser.UpdatedBy = currentUserId;

                    bool updated = await _exposeApiUserRepository.UpdateAsync(existingUser);
                    if (updated)
                    {
                        return StandardAPIResponse<ExposeApiUserDto>.SuccessResponse(userDto, "User updated successfully.");
                    }

                    return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, "Failed to update user.");
                }
            }
            catch (Exception ex)
            {
                return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<StandardAPIResponse<bool>> DeleteUserAsync(string id, string currentUserId)
        {
            try
            {
                var existingUser = await _exposeApiUserRepository.GetAsync(id);
                if (existingUser == null || existingUser.IsDeleted == true)
                {
                    return StandardAPIResponse<bool>.ErrorResponse(false, "User not found.");
                }

                bool deleted = await _exposeApiUserRepository.SoftDeleteAsync(id, currentUserId);
                if (deleted)
                {
                    return StandardAPIResponse<bool>.SuccessResponse(true, "User deleted successfully.");
                }

                return StandardAPIResponse<bool>.ErrorResponse(false, "Failed to delete user.");
            }
            catch (Exception ex)
            {
                return StandardAPIResponse<bool>.ErrorResponse(false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<StandardAPIResponse<List<ExposeApiUserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _exposeApiUserRepository.GetAllAsync(includeDeleted: false);
                var userDtos = users.Select(u => new ExposeApiUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Password = u.Password,
                    Role = u.Role,
                    IsActive = u.IsActive
                }).ToList();

                return StandardAPIResponse<List<ExposeApiUserDto>>.SuccessResponse(userDtos, "Users retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StandardAPIResponse<List<ExposeApiUserDto>>.ErrorResponse(null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<StandardAPIResponse<ExposeApiUserDto>> GetUserByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, "Username is required.");
                }

                var user = await _exposeApiUserRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, "User not found.");
                }

                var userDto = new ExposeApiUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    Role = user.Role,
                    IsActive = user.IsActive
                };

                return StandardAPIResponse<ExposeApiUserDto>.SuccessResponse(userDto, "User retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StandardAPIResponse<ExposeApiUserDto>.ErrorResponse(null, $"An error occurred: {ex.Message}");
            }
        }
    }
}
