namespace ALttPRandomizer.Service {
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    public class ProcessService {
        public ProcessService(ILogger<ProcessService> logger) {
            this.Logger = logger;
        }

        public ILogger<ProcessService> Logger { get; }

        public Process StartProcess(string logPrefix, string workingDirectory, params string[] args) {
            var start = new ProcessStartInfo(args[0], args.Skip(1)) {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            this.Logger.LogInformation("{prefix} - executing command: {command}", logPrefix, string.Join(" ", args.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg)));

            var process = Process.Start(start) ?? throw new GenerationFailedException("{0} - Process failed to start.", logPrefix);
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (_, args) => {
                if (args.Data != null) {
                    Logger.LogInformation("{prefix} - STDOUT: {output}", logPrefix, args.Data);
                }
            };
            process.ErrorDataReceived += (_, args) => {
                if (args.Data != null) {
                    Logger.LogInformation("{prefix} STDERR: {output}", logPrefix, args.Data);
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }
    }
}
