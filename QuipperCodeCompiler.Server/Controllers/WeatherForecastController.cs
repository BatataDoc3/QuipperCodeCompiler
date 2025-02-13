using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CodeExecutionController : ControllerBase
{
    private readonly ILogger<CodeExecutionController> _logger;

    public CodeExecutionController(ILogger<CodeExecutionController> logger)
    {
        _logger = logger;
    }

    [HttpPost("execute")]
    public IActionResult ExecuteCode([FromBody] CodeRequest request)
    {
        _logger.LogInformation("Executing code in {Language}", request.Language);


        var output = ExecuteCode(request.Language, request.Code);

        return Ok(new { Output = output });
    }

    private string ExecuteCode(string language, string code)
    {
        return "Hello, World!";  
    }
}

public class CodeRequest
{
    public string Language { get; set; }
    public string Code { get; set; }
}
