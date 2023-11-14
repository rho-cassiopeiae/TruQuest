using GoThataway;

using Domain.Results;

using Application.Common.Models.IM;
using Application.General.Commands.ArchiveDeadLetter;

namespace Application.Common.Middlewares.Request;

public class TracingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        string? traceparent = null;
        if (request is DeferredTaskCommand deferredTaskCommand) traceparent = deferredTaskCommand.Traceparent;
        else if (request is ArchiveDeadLetterCommand deadLetterCommand) traceparent = deadLetterCommand.Traceparent;

        // @@NOTE: Passing null 'traceparent' is the same as not passing it at all, that is,
        // parent gets set from Activity.Current if any.
        using var span = Telemetry.StartActivity(request.GetType().FullName!, traceparent: traceparent)!;
        return await next();
    }
}
