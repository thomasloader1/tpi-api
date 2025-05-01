using Microsoft.AspNetCore.Identity;
using TPI_API.Dtos.User;
using TPI_API.Interfaces;
using TPI_API.Models;

namespace TPI_API.Services
{

    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;

        }

        public Task<UserWithRolesDto> GetUserById(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserWithRolesDto> GetUserWithRoles(string email)
        {
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new UserWithRolesDto
            {
                UserName = user.UserName,
                Roles = roles,
                Email = user.Email,
                FullName = user.FullName
            };
        
    }
    }
}
