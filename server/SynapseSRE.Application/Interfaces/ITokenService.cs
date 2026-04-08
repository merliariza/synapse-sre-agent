using SynapseSRE.Domain.Entities;

namespace SynapseSRE.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}