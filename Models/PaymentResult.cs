using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace BusinessFlowApp.Models;

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
}
