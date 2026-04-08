using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Infrastructure.Persistence;
using SynapseSRE.Infrastructure.Repositories;

public class TriageAnalysisRepository : GenericRepository<TriageAnalysis>, ITriageAnalysisRepository 
{ public TriageAnalysisRepository(ApplicationDbContext context) : base(context) { } }