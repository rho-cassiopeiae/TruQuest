namespace Messages.Responses;

internal class ArchiveSettlementProposalAttachmentsFailureResult
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required string ErrorMessage { get; init; }
}
