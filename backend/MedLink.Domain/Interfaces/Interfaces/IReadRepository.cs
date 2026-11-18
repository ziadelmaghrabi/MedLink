using MedLink.Domain.Common;

namespace MedLink.Domain.Interfaces
{
     
    public interface IReadRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
    }
}
