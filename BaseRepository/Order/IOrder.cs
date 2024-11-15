using System.Linq;

namespace GenericRepository.BaseRepository
{
    public interface IOrder<T>
    {
        IOrderedQueryable<T> Apply(IQueryable<T> queryable);
    }
}