using TPI_API.Dtos.User;

namespace TPI_API.Interfaces
{
    public interface IUserService
    {
        Task<UserWithRolesDto> GetUserWithRoles(string email);
        Task<UserWithRolesDto> GetUserById(string id);
    }
}
