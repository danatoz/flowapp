using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace BusinessFlowApp.Models;

public class DeliveryResult
{
    public int EstimatedDays { get; set; }
    public decimal Cost { get; set; }
    public string MessageId { get; set; } = string.Empty;
}