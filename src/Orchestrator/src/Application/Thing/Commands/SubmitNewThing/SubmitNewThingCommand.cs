using System.Reflection;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using ThingDM = Domain.Aggregates.Thing;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.SubmitNewThing;

[RequireAuthorization]
public class SubmitNewThingCommand : IRequest<HandleResult<SubmitNewThingResultVM>>
{
    public NewThingIM Input { get; set; }
    public string Signature { get; set; }
}

internal class SubmitNewThingCommandHandler : IRequestHandler<SubmitNewThingCommand, HandleResult<SubmitNewThingResultVM>>
{
    private readonly ILogger<SubmitNewThingCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IFileFetcher _fileFetcher;
    private readonly IFileStorage _fileStorage;
    private readonly IThingRepository _thingRepository;

    public SubmitNewThingCommandHandler(
        ILogger<SubmitNewThingCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileFetcher fileFetcher,
        IFileStorage fileStorage,
        IThingRepository thingRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileFetcher = fileFetcher;
        _fileStorage = fileStorage;
        _thingRepository = thingRepository;
    }

    public async Task<HandleResult<SubmitNewThingResultVM>> Handle(
        SubmitNewThingCommand command, CancellationToken ct
    )
    {
        var result = _signer.RecoverFromNewThingMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        // check that result.Data == _currentPrincipal.Id

        await foreach (var (filePath, obj, prop) in _fileFetcher.FetchAll(command.Input, _currentPrincipal.Id))
        {
            _logger.LogDebug("File saved to " + filePath);

            var uploadResult = await _fileStorage.Upload(filePath);
            if (uploadResult.IsError)
            {
                return new()
                {
                    Error = uploadResult.Error
                };
            }

            _logger.LogDebug("File cid is " + uploadResult.Data);

            var attr = prop.GetCustomAttribute<FileURLAttribute>()!;
            if (attr.KeepOriginURL)
            {
                prop.SetValue(obj, $"{prop.GetValue(obj)}\t{uploadResult.Data}");
            }
            else
            {
                prop.SetValue(obj, uploadResult.Data);
            }
        }

        var thing = new ThingDM(
            title: command.Input.Title,
            details: command.Input.Details,
            imageURL: command.Input.ImageURL != string.Empty ? command.Input.ImageURL : null,
            submitterId: _currentPrincipal.Id,
            subjectId: command.Input.SubjectId
        );
        thing.AddEvidence(command.Input.Evidence.Select(e =>
        {
            var index = e.URL.LastIndexOf('\t');
            return new Evidence(
                originURL: e.URL.Substring(0, index),
                truURL: e.URL.Substring(index + 1)
            );
        }));
        thing.AddTags(command.Input.Tags.Select(t => t.Id));

        _thingRepository.Create(thing);

        await _thingRepository.SaveChanges();

        var thingVM = new ThingVM
        {
            Id = thing.Id!.Value.ToString()
        };

        return new()
        {
            Data = new()
            {
                Thing = thingVM,
                Signature = _signer.SignThing(thingVM)
            }
        };
    }
}