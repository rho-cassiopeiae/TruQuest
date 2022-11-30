using Domain.Base;

namespace Domain.Aggregates;

public interface ITaskRepository : IRepository<DeferredTask>
{
    void Create(DeferredTask task);
    Task<List<DeferredTask>> FindAllWithScheduledBlockNumber(long leBlockNumber);
}