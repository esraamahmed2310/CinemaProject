using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CinemaProject.Repositories.IRepositories
{
    public interface IRepository<T> where T: class
    {
         Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);


         void Update(T entity);

         void Delete(T entity);

         Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);


        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);



         Task<int> CommitAsync(CancellationToken cancellationToken = default);

    }
}
