using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using Application.Common.Interfaces;

namespace Infrastructure.Misc;

internal class FrontendEnvFileProvider : IFrontendEnvFileProvider
{
    private readonly string _envFileContents;

    public FrontendEnvFileProvider(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        var network = configuration["Ethereum:Network"];
        var contractsConfig = configuration.GetSection($"Ethereum:Contracts:{network}")!;
        _envFileContents =
            $"ENVIRONMENT={hostEnvironment.EnvironmentName}\n" +
            $"ORCHESTRATOR_HOST={configuration["AdvertisedHost"]}\n" +
            $"ETHEREUM_RPC_URL={configuration[$"Ethereum:Networks:{network}:URL"]}\n" +
            $"ETHEREUM_L1_RPC_URL={configuration[$"Ethereum:Networks:{network}:SettlementNetwork:URL"]}\n" +
            $"ERC4337_BUNDLER_BASE_URL={configuration[$"Ethereum:Bundler:{network}:Host"]}\n" +
            $"IPFS_GATEWAY_URL={configuration["IPFS:GatewayHost"]}/ipfs\n" +
            $"EntryPointAddress={contractsConfig["EntryPoint:Address"]}\n" +
            $"SimpleAccountFactoryAddress={contractsConfig["SimpleAccountFactory:Address"]}\n" +
            $"TruthserumAddress={contractsConfig["Truthserum:Address"]}\n" +
            $"TruQuestAddress={contractsConfig["TruQuest:Address"]}\n" +
            $"ThingValidationVerifierLotteryAddress={contractsConfig["ThingValidationVerifierLottery:Address"]}\n" +
            $"ThingValidationPollAddress={contractsConfig["ThingValidationPoll:Address"]}\n" +
            $"SettlementProposalAssessmentVerifierLotteryAddress={contractsConfig["SettlementProposalAssessmentVerifierLottery:Address"]}\n" +
            $"SettlementProposalAssessmentPollAddress={contractsConfig["SettlementProposalAssessmentPoll:Address"]}\n";
    }

    public string GetContents() => _envFileContents;
}