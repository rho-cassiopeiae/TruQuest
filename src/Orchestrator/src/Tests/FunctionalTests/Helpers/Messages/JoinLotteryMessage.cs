using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("joinLottery")]
public class JoinLotteryMessage : FunctionMessage
{
    [Parameter("string", "_thingId", 1)]
    public string ThingId { get; init; }
    [Parameter("bytes32", "_data", 2)]
    public byte[] Data { get; init; }
}