using MediatR;

using Domain.Aggregates;
using Domain.Results;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Settlement.Commands.PrepareForAssessmentPoll;

[ExecuteInTxn]
public class PrepareForAssessmentPollCommand : IRequest<VoidResult>
{
    public required long AssessmentPollInitBlockNumber { get; init; }
    public required int AssessmentPollInitTxnIndex { get; init; }
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required decimal Nonce { get; init; }
    public required List<string> ClaimantIds { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class PrepareForAssessmentPollCommandHandler : IRequestHandler<PrepareForAssessmentPollCommand, VoidResult>
{
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IJoinedThingAssessmentVerifierLotteryEventRepository _joinedThingAssessmentVerifierLotteryEventRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public PrepareForAssessmentPollCommandHandler(
        ISettlementProposalRepository settlementProposalRepository,
        ITaskRepository taskRepository,
        IJoinedThingAssessmentVerifierLotteryEventRepository joinedThingAssessmentVerifierLotteryEventRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IWatchedItemRepository watchedItemRepository,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _settlementProposalRepository = settlementProposalRepository;
        _taskRepository = taskRepository;
        _joinedThingAssessmentVerifierLotteryEventRepository = joinedThingAssessmentVerifierLotteryEventRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(PrepareForAssessmentPollCommand command, CancellationToken ct)
    {
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State == SettlementProposalState.FundedAndVerifierLotteryInitiated)
        {
            proposal.SetState(SettlementProposalState.VerifiersSelectedAndPollInitiated);
            proposal.AddVerifiers(command.ClaimantIds.Concat(command.WinnerIds));

            int pollDurationBlocks = await _contractStorageQueryable.GetAssessmentPollDurationBlocks();

            var task = new DeferredTask(
                type: TaskType.CloseThingSettlementProposalAssessmentPoll,
                scheduledBlockNumber: command.AssessmentPollInitBlockNumber + pollDurationBlocks
            );
            task.SetPayload(new()
            {
                ["thingId"] = proposal.ThingId,
                ["settlementProposalId"] = proposal.Id
            });
            _taskRepository.Create(task);

            var joinedThingAssessmentVerifierLotteryEvent = new JoinedThingAssessmentVerifierLotteryEvent(
                blockNumber: command.AssessmentPollInitBlockNumber,
                txnIndex: command.AssessmentPollInitTxnIndex,
                thingId: command.ThingId,
                settlementProposalId: command.SettlementProposalId,
                userId: command.Orchestrator,
                nonce: command.Nonce
            );
            _joinedThingAssessmentVerifierLotteryEventRepository.Create(joinedThingAssessmentVerifierLotteryEvent);

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            _watchedItemRepository.Add(
                command.WinnerIds
                    .Select(userId => new WatchedItem(
                        userId: userId,
                        itemType: WatchedItemType.SettlementProposal,
                        itemId: proposal.Id,
                        itemUpdateCategory: (int)SettlementProposalUpdateCategory.Special,
                        lastSeenUpdateTimestamp: now
                    ))
                    .ToArray()
            );

            await _settlementProposalUpdateRepository.AddOrUpdate(
                new SettlementProposalUpdate(
                    settlementProposalId: proposal.Id,
                    category: SettlementProposalUpdateCategory.General,
                    updateTimestamp: now + 10,
                    title: "Verifier lottery completed",
                    details: "Assessment poll initiated"
                ),
                new SettlementProposalUpdate(
                    settlementProposalId: proposal.Id,
                    category: SettlementProposalUpdateCategory.Special,
                    updateTimestamp: now + 10,
                    title: "You've been selected as a verifier!",
                    details: null
                )
            );

            await _settlementProposalRepository.SaveChanges();
            await _taskRepository.SaveChanges();
            await _joinedThingAssessmentVerifierLotteryEventRepository.SaveChanges();
            await _watchedItemRepository.SaveChanges();
            await _settlementProposalUpdateRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}