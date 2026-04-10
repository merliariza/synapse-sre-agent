using Microsoft.EntityFrameworkCore;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Infrastructure.Persistence;

namespace SynapseSRE.Infrastructure.Repositories;

public class IncidentRepository : GenericRepository<Incident>, IIncidentRepository
{
    private readonly ApplicationDbContext _ctx;

    public IncidentRepository(ApplicationDbContext context) : base(context)
    {
        _ctx = context;
    }

    public override async Task<Incident?> GetByIdAsync(Guid id)
    {
        return await _ctx.Incidents
            .Include(i => i.TriageAnalysis)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public override async Task<IEnumerable<Incident>> GetAllAsync()
    {
        return await _ctx.Incidents
            .Include(i => i.TriageAnalysis)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
}