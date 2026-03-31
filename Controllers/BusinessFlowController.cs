namespace BusinessFlowApp.Controllers;

using Microsoft.AspNetCore.Mvc;
using BusinessFlowApp.Core;

[ApiController]
[Route("api/[controller]")]
public class BusinessFlowController : ControllerBase
{
    private readonly IBusinessFlowExecutor _executor;

    public BusinessFlowController(IBusinessFlowExecutor executor)
    {
        _executor = executor;
    }

    /// <summary>
    /// Выполняет BusinessFlow на основе Mailbox и Folder
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteFlow([FromBody] ExecuteFlowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Mailbox))
        {
            return BadRequest("Mailbox is required");
        }

        if (string.IsNullOrWhiteSpace(request.Folder))
        {
            return BadRequest("Folder is required");
        }

        var message = new Message(new MemoryStream());
        var result = await _executor.ExecuteAsync(request.Mailbox, request.Folder, message, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Выполняет BusinessFlow с определением группы по XLSX файлу
    /// </summary>
    [HttpPost("xlsx")]
    public async Task<IActionResult> ProcessXlsx(
        [FromQuery] string mailbox,
        [FromQuery] string folder,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Файл не предоставлен");
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Только XLSX файлы");
        }

        await using var stream = file.OpenReadStream();
        var result = await _executor.ExecuteXlsxAsync(mailbox, folder, stream, cancellationToken);
        return Ok(result);
    }
}

public class ExecuteFlowRequest
{
    public string Mailbox { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
}
