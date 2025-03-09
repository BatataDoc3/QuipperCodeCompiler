using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Diagnostics;



[ApiController]
[Route("api/[controller]")]
public class CodeExecutionController : ControllerBase
{
    string defaultAlgorithmsDirectory = "DefaultAlgorithms/"; 

    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "CodeFiles/generated_code.hs");
    string hsFilePath = "CodeFiles/generated_code.hs";
    string exeFilePath = "CodeFiles/generated_code.exe";
    string objFilePath = "CodeFiles/generated_code.o";
    string hiFilePath = "CodeFiles/generated_code.hi";
    string epsFilePath = "CodeFiles/output.eps";
    string pngFilePath = "CodeFiles/output.png";
    private readonly ILogger<CodeExecutionController> _logger;

    public CodeExecutionController(ILogger<CodeExecutionController> logger)
    {
        _logger = logger;
    }


    [HttpGet("getCodeExamples")]
    public async Task<IActionResult> GetCodeExamples()
    {
        _logger.LogInformation("Getting code files");
        var files = Directory.GetFiles(defaultAlgorithmsDirectory, "*.hs")
                                .ToDictionary(Path.GetFileName, async file => await System.IO.File.ReadAllTextAsync(file));
        var result = new Dictionary<string, string>();
        foreach (var file in files)
        {
            result[file.Key] = await file.Value;
        }

        return Ok(result);
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


        var output = ExecuteFile();
        try
        {
            if (System.IO.File.Exists(hsFilePath)) System.IO.File.Delete(hsFilePath);
            if (System.IO.File.Exists(exeFilePath)) System.IO.File.Delete(exeFilePath);
            if (System.IO.File.Exists(objFilePath)) System.IO.File.Delete(objFilePath);
            if (System.IO.File.Exists(hiFilePath)) System.IO.File.Delete(hiFilePath);
            if (System.IO.File.Exists(epsFilePath)) System.IO.File.Delete(epsFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("File Cleanup Error: " + ex.Message);
        }


        if (System.IO.File.Exists(pngFilePath))
        {
            var imageBytes = System.IO.File.ReadAllBytes(pngFilePath);
            return File(imageBytes, "image/png");

        }

        return StatusCode(500, "Failed to generate image.");
    }


    private bool GenerateFile(string language, string code)
    {
        try
        {

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

    private string ExecuteFile()
    {
        string command = "ghc -package quipper-language CodeFiles/generated_code.hs > CodeFiles/output.pdf";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {command}",
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

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Compilation Error:\n" + error);
                return error; // Return the error message if compilation fails
            }
        }

        // Step 2: Run the compiled Haskell program
        string exeName = "chcp 65001 && CodeFiles\\generated_code.exe > CodeFiles\\output.eps"; // On Windows
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            exeName = "./CodeFiles/generated_code | Out-File .\\thing.ps -Encoding default"; // On Linux/macOS
        }

        psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {exeName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            output = FilterGhcOutput(output);
            error = FilterGhcOutput(error);

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Execution Error:\n" + error);
                return error; // Return execution errors if they occur
            }

            Console.WriteLine("Program Output:\n" + output);
        }

        psi = new ProcessStartInfo
        {
            FileName = "gswin64c",
            Arguments = "-dNOPAUSE -dBATCH -dEPSCrop -r300 -sDEVICE=pngalpha -sOutputFile=\"CodeFiles\\output.png\" CodeFiles\\output.eps",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }


    private string FilterGhcOutput(string output)
    {
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
