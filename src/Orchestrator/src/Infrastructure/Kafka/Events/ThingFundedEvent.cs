using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Thing.Commands.InitVerifierLottery;

namespace Infrastructure.Kafka.Events;

internal class ThingFundedEvent : TraceableEvent
{
    public required string WalletAddress { get; init; }
    public required decimal Stake { get; init; }
}

internal class ThingFundedEventHandler : IMessageHandler<ThingFundedEvent>
{
    private readonly SenderWrapper _sender;

    public ThingFundedEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingFundedEvent @event) =>
        _sender.Send(
            new InitVerifierLotteryCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))
            },
            addToAdditionalSinks: true
        );
}
