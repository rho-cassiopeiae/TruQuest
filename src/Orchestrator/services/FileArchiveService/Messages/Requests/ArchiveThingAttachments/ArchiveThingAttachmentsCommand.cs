using KafkaFlow;
using KafkaFlow.TypedHandler;

using Services;
using Messages.Responses;

namespace Messages.Requests;

internal class ArchiveThingAttachmentsCommand
{
    public string SubmitterId { get; set; }
    public Guid ThingId { get; set; }
    public NewThingIm Input { get; set; }
}

internal class ArchiveThingAttachmentsCommandHandler : IMessageHandler<ArchiveThingAttachmentsCommand>
{
    private readonly ILogger<ArchiveThingAttachmentsCommandHandler> _logger;
    private readonly IFileArchiver _fileArchiver;
    private readonly IResponseDispatcher _responseDispatcher;

    public ArchiveThingAttachmentsCommandHandler(
        ILogger<ArchiveThingAttachmentsCommandHandler> logger,
        IFileArchiver fileArchiver,
        IResponseDispatcher responseDispatcher
    )
    {
        _logger = logger;
        _fileArchiver = fileArchiver;
        _responseDispatcher = responseDispatcher;
    }

    public async Task Handle(IMessageContext context, ArchiveThingAttachmentsCommand message)
    {
        var progress = new Progress<int>(percent =>
        {
            _logger.LogInformation($"******************* Archive Progress: {percent}%");
        });

        var error = await _fileArchiver.ArchiveAllAttachments(message.Input, progress);
        object response;
        if (error != null)
        {
            response = new ArchiveThingAttachmentsFailureResult
            {
                ErrorMessage = error.ToString()
            };
        }
        else
        {
            response = new ArchiveThingAttachmentsSuccessResult
            {
                SubmitterId = message.SubmitterId,
                ThingId = message.ThingId,
                Input = message.Input
            };
        }

        await _responseDispatcher.Send(response, key: message.ThingId.ToString());
    }
}