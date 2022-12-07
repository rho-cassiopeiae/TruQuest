using System.Numerics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Nethereum.Web3;

using Application.Common.Interfaces;

using Infrastructure.Ethereum.Messages;

namespace Infrastructure.Ethereum;

internal class ContractCaller : IContractCaller
{
    private readonly ILogger<ContractCaller> _logger;
    private readonly Web3 _web3;
    private readonly string _verifierLotteryAddress;
    private readonly string _acceptancePollAddress;

    public ContractCaller(ILogger<ContractCaller> logger, IConfiguration configuration)
    {
        _logger = logger;

        var network = configuration["Ethereum:Network"]!;
        var orchestrator = new Nethereum.Web3.Accounts.Account(
            configuration[$"Ethereum:Accounts:{network}:Orchestrator:PrivateKey"]!,
            configuration.GetValue<int>($"Ethereum:Networks:{network}:ChainId")
        );
        _web3 = new Web3(orchestrator, configuration[$"Ethereum:Networks:{network}:URL"]);
        _verifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
    }

    public Task<byte[]> ComputeHash(byte[] data)
    {
        var queryDispatcher = _web3.Eth.GetContractQueryHandler<ComputeHashMessage>();
        return queryDispatcher.QueryAsync<byte[]>(_verifierLotteryAddress, new ComputeHashMessage { Data = data });
    }

    public async Task<long> InitVerifierLottery(string thingId, byte[] dataHash)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<InitVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _verifierLotteryAddress,
            new InitVerifierLotteryMessage
            {
                ThingId = thingId,
                DataHash = dataHash
            }
        );

        return (long)txnReceipt.BlockNumber.Value;
    }

    public Task<BigInteger> ComputeNonce(string thingId, byte[] data)
    {
        var queryDispatcher = _web3.Eth.GetContractQueryHandler<ComputeNonceMessage>();
        return queryDispatcher.QueryAsync<BigInteger>(
            _verifierLotteryAddress,
            new ComputeNonceMessage
            {
                ThingId = thingId,
                Data = data
            }
        );
    }

    public async Task CloseVerifierLotteryWithSuccess(string thingId, byte[] data, List<ulong> winnerIndices)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<CloseVerifierLotteryWithSuccessMessage>();
        try
        {
            var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                _verifierLotteryAddress,
                new CloseVerifierLotteryWithSuccessMessage
                {
                    ThingId = thingId,
                    Data = data,
                    WinnerIndices = winnerIndices
                }
            );

            _logger.LogInformation("Txn hash {TxnHash}", txnReceipt.TransactionHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task FinalizeAcceptancePollForThingAsAccepted(
        string thingId, string voteAggIpfsCid,
        List<string> verifiersToReward, List<string> verifiersToSlash
    )
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<FinalizeAcceptancePollForThingAsAcceptedMessage>();
        try
        {
            var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                _acceptancePollAddress,
                new FinalizeAcceptancePollForThingAsAcceptedMessage
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToReward = verifiersToReward,
                    VerifiersToSlash = verifiersToSlash
                }
            );

            _logger.LogInformation("Txn hash {TxnHash}", txnReceipt.TransactionHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}