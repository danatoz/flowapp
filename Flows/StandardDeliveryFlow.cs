namespace BusinessFlowApp.Flows;

using BusinessFlowApp.Core;
using BusinessFlowApp.Extensions;
using BusinessFlowApp.Models;

public class StandardDeliveryFlow : BusinessFlowBase<DeliveryResult>
{
    public override FlowType FlowType => FlowType.Delivery;
    public override string Group => "Standard";

    public StandardDeliveryFlow(IBusinessFlowHandler handler)
        : base(handler)
    {
    }

    protected override async Task<DeliveryResult> ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        Console.WriteLine("Executing Standard Delivery Flow");
        var content = message.ReadContent();
        Console.WriteLine($"  Request: [Delivery Standard] {content}");

        await Task.Delay(10, cancellationToken);

        return new DeliveryResult
        {
            EstimatedDays = 5,
            Cost = 500,
            MessageId = content
        };
    }
}
