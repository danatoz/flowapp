using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using BusinessFlowApp.Core;

namespace BusinessFlowApp.Extensions;

public static class MessageExtensions
{
    public static string ReadContent(this Message message)
    {
        message.Stream.Position = 0;
        using var reader = new StreamReader(message.Stream, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }
}
