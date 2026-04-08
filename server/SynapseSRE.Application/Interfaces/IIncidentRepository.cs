using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;

namespace SynapseSRE.Domain.Interfaces 
{
    public interface IIncidentRepository : IRepository<Incident>
    {
    }
}