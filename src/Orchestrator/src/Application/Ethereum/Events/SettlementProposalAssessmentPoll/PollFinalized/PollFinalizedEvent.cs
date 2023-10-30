using MediatR;

using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalAssessmentPoll.PollFinalized;

public class PollFinalizedEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public required List<string> RewardedVerifiers { get; init; }
    public required List<string> SlashedVerifiers { get; init; }
}

internal class PollFinalizedEventHandler : INotificationHandler<PollFinalizedEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public PollFinalizedEventHandler(IActionableThingRelatedEventRepository actionableThingRelatedEventRepository)
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(PollFinalizedEvent @event, CancellationToken ct)
    {
        var pollFinalizedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.SettlementProposalAssessmentPollFinalized
        );

        var payload = new Dictionary<string, object>()
        {
            ["settlementProposalId"] = new Guid(@event.SettlementProposalId),
            ["decision"] = @event.Decision,
            ["voteAggIpfsCid"] = @event.VoteAggIpfsCid,
            ["rewardedVerifiers"] = @event.RewardedVerifiers,
            ["slashedVerifiers"] = @event.SlashedVerifiers,
        };

        Telemetry.CurrentActivity!.AddTraceparentTo(payload);
        pollFinalizedEvent.SetPayload(payload);

        _actionableThingRelatedEventRepository.Create(pollFinalizedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}