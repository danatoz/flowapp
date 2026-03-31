namespace BusinessFlowApp.Flows;

using BusinessFlowApp.Core;
using BusinessFlowApp.Extensions;
using BusinessFlowApp.Models;

public class CreatePaymentFlow : BusinessFlowBase<PaymentResult>
{
    public override FlowType FlowType => FlowType.Payments;
    public override string Group => "Create";

    public CreatePaymentFlow(IBusinessFlowHandler handler)
        : base(handler)
    {
    }

    protected override async Task<PaymentResult> ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        Console.WriteLine("Executing Create Payment Flow");
        var content = message.ReadContent();
        Console.WriteLine($"  Request: [Payment Create] {content}");

        await Task.Delay(10, cancellationToken);

        return new PaymentResult
        {
            Success = true,
            TransactionId = "TXN-" + Guid.NewGuid().ToString()[..8],
            MessageId = content
        };
    }
}