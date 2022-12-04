using System.Numerics;

namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<byte[]> ComputeHash(byte[] data);
    Task<long> InitVerifierLottery(string thingId, byte[] dataHash);
    Task<BigInteger> ComputeNonce(string thingId, byte[] data);
    Task CloseVerifierLotteryWithSuccess(string thingId, byte[] data, IList<ulong> winnerIndices);
}