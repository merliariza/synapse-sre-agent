using Microsoft.EntityFrameworkCore;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Application.Interfaces;
using SynapseSRE.Infrastructure.Persistence;

namespace SynapseSRE.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username) => 
        await _context.Set<User>().FirstOrDefaultAsync(u => u.Username == username);
}