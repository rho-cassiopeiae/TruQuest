namespace Application.User.Commands.SignInWithEthereum;

public class SignInWithEthereumResultVm
{
    public required string UserId { get; init; }
    public required string SignerAddress { get; init; }
    public required string WalletAddress { get; init; }
    public required string Token { get; init; }
}
