namespace BusinessFlowApp.Flows;

using BusinessFlowApp.Core;
using BusinessFlowApp.Extensions;
using BusinessFlowApp.Models;

public class ExpressDeliveryFlow : BusinessFlowBase<DeliveryResult>
{
    public override FlowType FlowType => FlowType.Delivery;
    public override string Group => "Express";

    public ExpressDeliveryFlow(IBusinessFlowHandler handler)
        : base(handler)
    {
    }

    protected override async Task<DeliveryResult> ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        Console.WriteLine("Executing Express Delivery Flow");
        var content = message.ReadContent();
        Console.WriteLine($"  Request: [Delivery Express] {content}");

        await Task.Delay(10, cancellationToken);

        return new DeliveryResult
        {
            EstimatedDays = 1,
            Cost = 1500,
            MessageId = content
        };
    }
}