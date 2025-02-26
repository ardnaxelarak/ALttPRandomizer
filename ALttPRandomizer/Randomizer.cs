namespace ALttPRandomizer {
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Options;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class Randomizer {
        public Randomizer(
                IdGenerator idGenerator,
                AzureStorage azureStorage,
                IOptionsMonitor<ServiceOptions> optionsMonitor,
                ILogger<Randomizer> logger) {
            this.IdGenerator = idGenerator;
            this.AzureStorage = azureStorage;
            this.OptionsMonitor = optionsMonitor;
            this.Logger = logger;
        }

        private AzureStorage AzureStorage { get; }
        private IOptionsMonitor<ServiceOptions> OptionsMonitor { get; }
        private IdGenerator IdGenerator { get; }
        private ILogger<Randomizer> Logger { get; }
        private ServiceOptions Configuration => this.OptionsMonitor.CurrentValue;

        public string Randomize() {
            var start = new ProcessStartInfo() {
                FileName = Configuration.PythonPath,
                WorkingDirectory = Configuration.RandomizerPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var id = IdGenerator.GenerateId();

            var args = start.ArgumentList;
            args.Add("DungeonRandomizer.py");
            args.Add("--rom=../alttp.sfc");
            args.Add("--bps");

            args.Add("--outputpath");
            args.Add(Path.GetTempPath());

            args.Add("--outputname");
            args.Add(id);

            args.Add("--quickswap");

            var process = Process.Start(start) ?? throw new GenerationFailedException("Process failed to start.");
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (_, args) => this.Logger.LogInformation("Randomizer STDOUT: {output}", args.Data);
            process.ErrorDataReceived += (_, args) => this.Logger.LogInformation("Randomizer STDERR: {output}", args.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.Exited += async (sender, args) => {
                var exitcode = process.ExitCode;
                process.Dispose();

                if (exitcode != 0) {
                    this.GenerationFailed(id, exitcode);
                } else {
                    await this.GenerationSucceeded(id);
                }
            };

            return id;
        }

        private async Task GenerationSucceeded(string id) {
            var rom = Path.Join(Path.GetTempPath(), string.Format("DR_{0}.sfc", id));

            var bpsIn = Path.Join(Path.GetTempPath(), string.Format("DR_{0}.bps", id));
            var bpsOut = string.Format("{0}/patch.bps", id);

            var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("DR_{0}_Spoiler.txt", id));
            var spoilerOut = string.Format("{0}/spoiler.txt", id);

            var uploadPatch = this.AzureStorage.UploadFileAndDelete(bpsOut, bpsIn);
            var uploadSpoiler = this.AzureStorage.UploadFileAndDelete(spoilerOut, spoilerIn);

            await Task.WhenAll(uploadPatch, uploadSpoiler);

            this.Logger.LogDebug("Deleting file {filepath}", rom);
            File.Delete(rom);

            this.Logger.LogDebug("Finished uploading seed id {id}", id);
        }

        private void GenerationFailed(string id, int exitcode) {
        }
    }
}
