namespace Services;

internal interface IResponseDispatcher
{
    Task ReplyTo(string requestId, object message);
    Task Send(object message, string? key = null);
}