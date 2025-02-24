using LiteDB;
using System.Linq.Expressions;
using V2Unity.Model;

namespace V2Unity.API.Persistence
{
    public class LiteDBRepository<T> : IRepository<T> where T:Entity
    {
        private readonly ILiteCollection<T> _collection;

        public LiteDBRepository(ILiteDatabase database)
        {
            _collection = database.GetCollection<T>(typeof(T).Name);
        }

        public T? FindOne(Guid id)
        {
            return _collection.FindOne(x => x.Id == id);
        }

        public T? FindOne(Expression<Func<T, bool>> expression)
        {
            return _collection.FindOne(expression);
        }

        public IEnumerable<T> FindAll()
        {
            return _collection.FindAll();
        }

        public Guid Add(T x)
        {            
            return _collection.Insert(x).AsGuid;
        }

        public bool Delete(Guid id)
        {
            return _collection.Delete(id);
        }

        public bool Update(T x)
        {
            return _collection.Update(x);
        }
    }
}
