using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;

namespace Application.Ethereum.Events.ThingAssessmentVerifierLottery.LotteryClosedWithSuccess;

public class LotteryClosedWithSuccessEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required string TxnHash { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required byte[] Data { get; init; }
    public required byte[] UserXorData { get; init; }
    public required byte[] HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> ClaimantIds { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class LotteryClosedWithSuccessEventHandler : INotificationHandler<LotteryClosedWithSuccessEvent>
{
    private readonly IActionableThingRelatedEventRepository _actionableThingRelatedEventRepository;

    public LotteryClosedWithSuccessEventHandler(
        IActionableThingRelatedEventRepository actionableThingRelatedEventRepository
    )
    {
        _actionableThingRelatedEventRepository = actionableThingRelatedEventRepository;
    }

    public async Task Handle(LotteryClosedWithSuccessEvent @event, CancellationToken ct)
    {
        var lotteryClosedEvent = new ActionableThingRelatedEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            thingId: new Guid(@event.ThingId),
            type: ThingEventType.SettlementProposalAssessmentVerifierLotteryClosedWithSuccess
        );
        lotteryClosedEvent.SetPayload(new()
        {
            ["settlementProposalId"] = new Guid(@event.SettlementProposalId),
            ["orchestrator"] = @event.Orchestrator,
            ["data"] = @event.Data.ToHex(prefix: true),
            ["userXorData"] = @event.UserXorData.ToHex(prefix: true),
            ["hashOfL1EndBlock"] = @event.HashOfL1EndBlock.ToHex(prefix: true),
            ["nonce"] = @event.Nonce,
            ["claimantIds"] = @event.ClaimantIds,
            ["winnerIds"] = @event.WinnerIds
        });
        _actionableThingRelatedEventRepository.Create(lotteryClosedEvent);

        await _actionableThingRelatedEventRepository.SaveChanges();
    }
}
