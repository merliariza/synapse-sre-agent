using Microsoft.EntityFrameworkCore;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Infrastructure.Persistence;
using SynapseSRE.Infrastructure.Repositories;

public class IncidentRepository : GenericRepository<Incident>, IIncidentRepository
{
    public IncidentRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<IEnumerable<Incident>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Incidents
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
}