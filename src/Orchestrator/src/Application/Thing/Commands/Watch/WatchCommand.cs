using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.Watch;

[RequireAuthorization]
public class WatchCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required bool MarkedAsWatched { get; init; }
}

internal class WatchCommandHandler : IRequestHandler<WatchCommand, VoidResult>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public WatchCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _currentPrincipal = currentPrincipal;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task<VoidResult> Handle(WatchCommand command, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (command.MarkedAsWatched)
        {
            foreach (var category in Enum.GetValues<ThingUpdateCategory>())
            {
                _watchedItemRepository.Add(new WatchedItem(
                    userId: _currentPrincipal.Id!,
                    itemType: WatchedItemType.Thing,
                    itemId: command.ThingId,
                    itemUpdateCategory: (int)category,
                    lastSeenUpdateTimestamp: now
                ));
            }
        }
        else
        {
            foreach (var category in Enum.GetValues<ThingUpdateCategory>())
            {
                _watchedItemRepository.Remove(new WatchedItem(
                    userId: _currentPrincipal.Id!,
                    itemType: WatchedItemType.Thing,
                    itemId: command.ThingId,
                    itemUpdateCategory: (int)category
                ));
            }
        }

        await _watchedItemRepository.SaveChanges();

        return VoidResult.Instance;
    }
}