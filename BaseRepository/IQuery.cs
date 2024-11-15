using System.Linq;

namespace GenericRepository.BaseRepository
{
    public interface IQuery<in TIn, out TOut>
    {
        IQueryable<TOut> Execute(IQueryable<TIn> items);
    }
}