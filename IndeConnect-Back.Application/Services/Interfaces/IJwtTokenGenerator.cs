using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}