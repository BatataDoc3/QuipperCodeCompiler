using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

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

        // Generate file from the code if needed
        if (!GenerateFile(request.Language, request.Code))
        {
            return StatusCode(500, "Failed to generate file.");
        }

        var output = ExecuteCode(request.Language, request.Code);

        return Ok(new { Output = output ?? "No output generated." });
    }

    private string ExecuteCode(string language, string code)
    {
        return $"Executed {language} code: {code}";
    }

    private bool GenerateFile(string language, string code)
    {
        try
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "CodeFiles/generated_code.hs");

            using (FileStream fs = System.IO.File.Create(filePath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(code);
                fs.Write(info, 0, info.Length);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file.");
            return false;
        }
    }
}

public class CodeRequest
{
    public string Language { get; set; }
    public string Code { get; set; }
}
