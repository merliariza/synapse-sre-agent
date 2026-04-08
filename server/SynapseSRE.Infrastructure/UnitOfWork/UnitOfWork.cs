using SynapseSRE.Application.Interfaces;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Infrastructure.Repositories;

namespace SynapseSRE.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    
    private IUserRepository? _users;
    private IIncidentRepository? _incidents;
    private IActivityLogRepository? _logs;
    private ITriageAnalysisRepository? _triage;
    private IAttachmentRepository? _attachments;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IIncidentRepository Incidents => _incidents ??= new IncidentRepository(_context);
    public IActivityLogRepository Logs => _logs ??= new ActivityLogRepository(_context);
    public ITriageAnalysisRepository TriageAnalyses => _triage ??= new TriageAnalysisRepository(_context);
    public IAttachmentRepository Attachments => _attachments ??= new AttachmentRepository(_context);

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}