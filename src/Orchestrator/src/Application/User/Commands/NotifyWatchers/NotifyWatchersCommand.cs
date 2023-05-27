using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Interfaces;
using Application.User.Common.Models.IM;

namespace Application.User.Commands.NotifyWatchers;

public class NotifyWatchersCommand : IRequest<VoidResult>
{
    public required WatchedItemTypeIm ItemType { get; init; }
    public required Guid ItemId { get; init; }
    public required int ItemUpdateCategory { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }
}

internal class NotifyWatchersCommandHandler : IRequestHandler<NotifyWatchersCommand, VoidResult>
{
    private readonly ILogger<NotifyWatchersCommandHandler> _logger;
    private readonly IWatchListQueryable _watchListQueryable;
    private readonly IClientNotifier _clientNotifier;

    public NotifyWatchersCommandHandler(
        ILogger<NotifyWatchersCommandHandler> logger,
        IWatchListQueryable watchListQueryable,
        IClientNotifier clientNotifier
    )
    {
        _logger = logger;
        _watchListQueryable = watchListQueryable;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(NotifyWatchersCommand command, CancellationToken ct)
    {
        if (command.ItemUpdateCategory % 100 == 0) // General update
        {
            var watcherIds = await _watchListQueryable.GetGeneralWatchersFor(
                (WatchedItemType)command.ItemType, command.ItemId
            );

            await _clientNotifier.NotifyUsersAboutItemUpdate(
                watcherIds, command.UpdateTimestamp, (WatchedItemType)command.ItemType,
                command.ItemId, command.ItemUpdateCategory, command.Title, command.Details
            );
        }
        else
        {
            var watcherIds = await _watchListQueryable.GetWatchersFor(
                (WatchedItemType)command.ItemType, command.ItemId, command.ItemUpdateCategory
            );

            await _clientNotifier.NotifyUsersAboutSpecialItemUpdate(
                watcherIds, command.UpdateTimestamp, (WatchedItemType)command.ItemType,
                command.ItemId, command.ItemUpdateCategory, command.Title, command.Details
            );
        }

        return VoidResult.Instance;
    }
}