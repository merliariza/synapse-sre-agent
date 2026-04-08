using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;

namespace SynapseSRE.Domain.Interfaces 
{
    public interface IActivityLogRepository : IRepository<ActivityLog>
    {
    }
}