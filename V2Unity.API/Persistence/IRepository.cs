using System.Linq.Expressions;
using V2Unity.Model;

namespace V2Unity.API.Persistence
{
    public interface IRepository<T> where T : Entity
    {
        public T? FindOne(Guid id);
        public T? FindOne(Expression<Func<T, bool>> expression);
        public IEnumerable<T> FindAll();
        public Guid Add(T entity);
        public bool Update(T entity);
        public bool Delete(Guid id);
    }
}
