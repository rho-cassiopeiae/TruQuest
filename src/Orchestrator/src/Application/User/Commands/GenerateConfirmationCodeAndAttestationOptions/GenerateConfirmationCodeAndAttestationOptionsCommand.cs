using System.Text;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Fido2NetLib;
using Fido2NetLib.Objects;
using MediatR;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.User.Commands.GenerateConfirmationCodeAndAttestationOptions;

public class GenerateConfirmationCodeAndAttestationOptionsCommand : IRequest<HandleResult<CredentialCreateOptions>>
{
    public required string Email { get; init; }
}

internal class GenerateConfirmationCodeAndAttestationOptionsCommandHandler : IRequestHandler<
    GenerateConfirmationCodeAndAttestationOptionsCommand,
    HandleResult<CredentialCreateOptions>
>
{
    private readonly ILogger<GenerateConfirmationCodeAndAttestationOptionsCommandHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly ITotpProvider _totpProvider;

    public GenerateConfirmationCodeAndAttestationOptionsCommandHandler(
        ILogger<GenerateConfirmationCodeAndAttestationOptionsCommandHandler> logger,
        IUserRepository userRepository,
        IFido2 fido2,
        IMemoryCache memoryCache,
        ITotpProvider totpProvider
    )
    {
        _logger = logger;
        _userRepository = userRepository;
        _fido2 = fido2;
        _memoryCache = memoryCache;
        _totpProvider = totpProvider;
    }

    public async Task<HandleResult<CredentialCreateOptions>> Handle(GenerateConfirmationCodeAndAttestationOptionsCommand command, CancellationToken ct)
    {
        var user = await _userRepository.FindByEmail(command.Email); // @@TODO: Use queryable instead.
        if (user != null)
        {
            return new()
            {
                Error = new UserError($"User with email {command.Email} already exists")
            };
        }

        var fido2User = new Fido2User
        {
            Id = Guid.NewGuid().ToByteArray(),
            Name = command.Email,
            DisplayName = command.Email
        };

        var authenticatorSelection = new AuthenticatorSelection
        {
            AuthenticatorAttachment = AuthenticatorAttachment.Platform,
            ResidentKey = ResidentKeyRequirement.Required,
            UserVerification = UserVerificationRequirement.Discouraged
        };

        // var exts = new AuthenticationExtensionsClientInputs()
        // {
        //     Extensions = true,
        //     PRF = new()
        //     {
        //         Eval = new()
        //         {
        //             First = Enumerable.Repeat<byte>(5, 32).ToArray()
        //         }
        //     }
        // };

        var options = _fido2.RequestNewCredential(
            fido2User,
            new(),
            authenticatorSelection,
            AttestationConveyancePreference.None
        );

        var challenge = Base64Url.Encode(options.Challenge);
        _memoryCache.Set($"fido2.attestationOptions.{challenge}", options.ToJson()); // @@TODO: Expiration.

        var totp = _totpProvider.GenerateTotpFor(Encoding.UTF8.GetBytes(command.Email));
        // @@TODO!!: Send the code.

        _logger.LogInformation($"******** Confirmation code: {totp}. Attestation Challenge: {challenge} ********");

        return new()
        {
            Data = options
        };
    }
}