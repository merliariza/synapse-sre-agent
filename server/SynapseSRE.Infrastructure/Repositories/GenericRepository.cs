using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Infrastructure.Persistence;

namespace SynapseSRE.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual void Add(T entity) => _context.Set<T>().Add(entity);

    public virtual void AddRange(IEnumerable<T> entities) => _context.Set<T>().AddRange(entities);

    public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate) 
        => _context.Set<T>().Where(predicate);

    public virtual async Task<IEnumerable<T>> GetAllAsync() 
        => await _context.Set<T>().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(Guid id) 
        => await _context.Set<T>().FindAsync(id);

    public virtual void Remove(T entity) => _context.Set<T>().Remove(entity);

    public virtual void RemoveRange(IEnumerable<T> entities) => _context.Set<T>().RemoveRange(entities);

    public virtual void Update(T entity) => _context.Set<T>().Update(entity);
}