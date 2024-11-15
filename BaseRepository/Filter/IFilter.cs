using System.Linq;

namespace GenericRepository.BaseRepository
{
    public interface IFilter<T>
    {
        IQueryable<T> Apply(IQueryable<T> items);
    }
}