using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Tests.FunctionalTests.Helpers.Errors;

[Error("ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock")]
public class ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlockError
{
    [Parameter("bytes32", "thingProposalId", 1)]
    public byte[] ThingProposalId { get; set; }
}