using MediatR;

using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.ThingFunded;

public class ThingFundedEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required string TxnHash { get; init; }
    public required byte[] ThingId { get; init; }
    public required string UserId { get; init; }
    public required decimal Stake { get; init; }
}

internal class ThingFundedEventHandler : INotificationHandler<ThingFundedEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public ThingFundedEventHandler(IActionableThingRelatedEventRepository actionableThingRelatedEventRepository)
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(ThingFundedEvent @event, CancellationToken ct)
    {
        var thingFundedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.Funded
        );
        thingFundedEvent.SetPayload(new()
        {
            ["userId"] = @event.UserId,
            ["stake"] = @event.Stake
        });
        _actionableThingRelatedEventRepository.Create(thingFundedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
