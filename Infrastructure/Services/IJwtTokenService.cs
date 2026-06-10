using Infrastructure.Auth.Dtos;
using Infrastructure.Entities;

namespace Infrastructure.Services;

public interface IJwtTokenService
{
    AuthResponse CreateToken(User user, IEnumerable<string> permissions);
}
