using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;

namespace SynapseSRE.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}