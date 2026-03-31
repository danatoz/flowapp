namespace BusinessFlowApp.Flows;

using BusinessFlowApp.Core;
using BusinessFlowApp.Extensions;
using BusinessFlowApp.Models;

public class RefundPaymentFlow : BusinessFlowBase<PaymentResult>
{
    public override FlowType FlowType => FlowType.Payments;
    public override string Group => "Refund";

    public RefundPaymentFlow(IBusinessFlowHandler handler)
        : base(handler)
    {
    }

    protected override async Task<PaymentResult> ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        Console.WriteLine("Executing Refund Payment Flow");
        var content = message.ReadContent();
        Console.WriteLine($"  Request: [Payment Refund] {content}");

        await Task.Delay(10, cancellationToken);

        return new PaymentResult
        {
            Success = true,
            TransactionId = "REF-" + Guid.NewGuid().ToString()[..8],
            MessageId = content
        };
    }
}