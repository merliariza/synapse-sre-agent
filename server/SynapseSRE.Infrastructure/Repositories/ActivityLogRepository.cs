using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Infrastructure.Persistence;
using SynapseSRE.Infrastructure.Repositories;

public class ActivityLogRepository : GenericRepository<ActivityLog>, IActivityLogRepository 
{ public ActivityLogRepository(ApplicationDbContext context) : base(context) { } }