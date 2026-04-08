using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Infrastructure.Persistence;
using SynapseSRE.Infrastructure.Repositories;

public class AttachmentRepository : GenericRepository<IncidentAttachment>, IAttachmentRepository 
{ public AttachmentRepository(ApplicationDbContext context) : base(context) { } }