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

        var id = Math.Abs(rand.Next());

        string path = Path.Combine(Directory.GetCurrentDirectory(), "CodeFiles\\" + id.ToString());
        _logger.LogInformation("Path to be created: " + path);
        Directory.CreateDirectory(path);


        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "CodeFiles\\" + id + "\\generated_code.hs");
        string hsFilePath = "CodeFiles\\" + id + "\\generated_code.hs";
        string exeFilePath = "CodeFiles\\" + id + "\\generated_code.exe";
        string objFilePath = "CodeFiles\\" + id + "\\generated_code.o";
        string hiFilePath = "CodeFiles\\" + id + "\\generated_code.hi";
        string epsFilePath = "CodeFiles\\" + id + "\\output.eps";
        string pngFilePath = "CodeFiles\\" + id + "\\output.png";


        _logger.LogInformation("Executing code in {Language}", request.Code);

        // Generate file from the code if needed
        if (!GenerateFile(request.Language, request.Code, filePath))
        {
            return StatusCode(500, "Failed to generate file.");
        }


        var output = ExecuteFile(hsFilePath, exeFilePath, epsFilePath, pngFilePath);
        try
        {
            Directory.Delete("CodeFiles\\" + id);
        }
        catch (Exception ex)
        {
            Console.WriteLine("File Cleanup Error: " + ex.Message);
        }


        if (System.IO.File.Exists(pngFilePath))
        {
            var imageBytes = System.IO.File.ReadAllBytes(pngFilePath);
            Directory.Delete("CodeFiles\\" + id);
            return File(imageBytes, "image/png");
        }

        return StatusCode(500, "Failed to generate image.");
    }


    private bool GenerateFile(string language, string code, string filePath)
    {
        try
        {
            _logger.LogInformation("Creating path: " + filePath);
            using (FileStream fs = System.IO.File.Create(filePath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(code);
                fs.Write(info, 0, info.Length);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file");
            return false;
        }
    }

    private string ExecuteFile(string hsFilePath, string exeFilePath, string epsFilePath, string pngFilePath)
    {
        _logger.LogInformation("Executing File");
        string command = "ghc -package quipper-language " + hsFilePath ;

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
        string exeName = "chcp 65001 && " + exeFilePath + " > " + epsFilePath; // On Windows
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            exeName = "./" + exeFilePath + " | Out-File .\\thing.ps -Encoding default"; // On Linux/macOS
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
            Arguments = "-dNOPAUSE -dBATCH -dEPSCrop -r300 -sDEVICE=pngalpha -sOutputFile=" + pngFilePath + " " + epsFilePath,
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
