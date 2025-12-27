using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CodeExecutionController : ControllerBase
{
    Random rand = new Random();
    string defaultAlgorithmsDirectory = "DefaultAlgorithms/"; 

    private readonly ILogger<CodeExecutionController> _logger;

    public CodeExecutionController(ILogger<CodeExecutionController> logger)
    {
        _logger = logger;
    }

    [HttpGet("getCodeExamples")]
    public async Task<IActionResult> GetCodeExamples()
    {
        _logger.LogInformation("Getting code files");
        
        if (!Directory.Exists(defaultAlgorithmsDirectory))
        {
            return Ok(new Dictionary<string, string>());
        }

        var files = Directory.GetFiles(defaultAlgorithmsDirectory, "*.hs");
        var result = new Dictionary<string, string>();
        
        foreach (var file in files)
        {
            result[Path.GetFileName(file)] = await System.IO.File.ReadAllTextAsync(file);
        }

        return Ok(result);
    }

    [HttpPost("execute")]
    public IActionResult ExecuteCode([FromBody] CodeRequest request)
    {
        var id = Math.Abs(rand.Next());
        
        // Use Path.Combine to ensure forward slashes on Linux
        string folderName = id.ToString();
        string baseDir = Path.Combine(Directory.GetCurrentDirectory(), "CodeFiles");
        string path = Path.Combine(baseDir, folderName);
        
        _logger.LogInformation("Path to be created: " + path);
        Directory.CreateDirectory(path);

        string filePath = Path.Combine(path, "generated_code.hs");
        string hsFilePath = Path.Combine("CodeFiles", folderName, "generated_code.hs");
        string exeFilePath = Path.Combine("CodeFiles", folderName, "generated_code"); // Removed .exe for Linux
        string epsFilePath = Path.Combine("CodeFiles", folderName, "output.eps");
        string jpegFilePath = Path.Combine("CodeFiles", folderName, "output.jpeg");

        _logger.LogInformation("Executing code in {Language}", request.Language);

        if (!GenerateFile(request.Language, request.Code, filePath))
        {
            return StatusCode(500, "Failed to generate file.");
        }

        var output = ExecuteFile(hsFilePath, exeFilePath, epsFilePath, jpegFilePath);

        if (System.IO.File.Exists(jpegFilePath))
        {
            var imageBytes = System.IO.File.ReadAllBytes(jpegFilePath);
            try { Directory.Delete(path, true); } catch { }
            return File(imageBytes, "image/jpeg");
        }
        
        try { Directory.Delete(path, true); } catch { }
        return StatusCode(500, $"Failed to generate image. Output: {output}");
    }

    private bool GenerateFile(string language, string code, string filePath)
    {
        try
        {
            _logger.LogInformation("Creating path: " + filePath);
            System.IO.File.WriteAllText(filePath, code, new UTF8Encoding(false));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file");
            return false;
        }
    }

    private string ExecuteFile(string hsFilePath, string exeFilePath, string epsFilePath, string jpegFilePath)
    {
        _logger.LogInformation("Executing File");
        
        // Step 1: Compile with GHC
        // Note: -o specifies output binary name
        string compileCommand = $"ghc -package quipper-language {hsFilePath} -o {exeFilePath}";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{compileCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            error = FilterGhcOutput(error);

            if (!string.IsNullOrEmpty(error) && error.Contains("error:"))
            {
                Console.WriteLine("Compilation Error:\n" + error);
                return error;
            }
        }

        // Step 2: Run the compiled Haskell program
        // On Linux, we use ./path/to/exe and redirect to EPS
        string runCommand = $"./{exeFilePath} > {epsFilePath}";

        psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{runCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Execution Error:\n" + error);
                // Not returning here because sometimes Quipper warns on stderr but still produces EPS
            }
        }

        // Step 3: Convert EPS to JPEG using Ghostscript (gs)
        // Linux Ghostscript command is usually 'gs'
        string gsCommand = $"gs -dNOPAUSE -dBATCH -dEPSCrop -r300 -sDEVICE=jpeg -sOutputFile={jpegFilePath} {epsFilePath}";

        psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{gsCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }

    private string FilterGhcOutput(string output)
    {
        if (string.IsNullOrEmpty(output)) return "";
        return string.Join("\n", output.Split('\n')
            .Where(line => !line.Contains("Loaded package environment from"))
            .ToArray()).Trim();
    }
}

public class CodeRequest
{
    public string Language { get; set; }
    public string Code { get; set; }
}