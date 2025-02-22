namespace ALttPRandomizer {
    using ALttPRandomizer.Options;
    using Microsoft.Extensions.Options;
    using System.Diagnostics;

    public class Randomizer {
        public Randomizer(IdGenerator idGenerator, IOptionsMonitor<ServiceOptions> optionsMonitor) {
            this.IdGenerator = idGenerator;
            this.optionsMonitor = optionsMonitor;
        }

        private IOptionsMonitor<ServiceOptions> optionsMonitor;
        private IdGenerator IdGenerator;
        private ServiceOptions Configuration => optionsMonitor.CurrentValue;

        public string Randomize() {
            var start = new ProcessStartInfo() {
                FileName = Configuration.PythonPath,
                WorkingDirectory = Configuration.RandomizerPath,
                RedirectStandardOutput = true,
            };

            var args = start.ArgumentList;
            args.Add("DungeonRandomizer.py");
            args.Add("--rom=../alttp.sfc");
            args.Add("--bps");

            args.Add("--quickswap");

            var process = Process.Start(start) ?? throw new GenerationFailedException("Process failed to start.");
            process.EnableRaisingEvents = true;

            var id = IdGenerator.GenerateId();

            process.Exited += (sender, args) => {
                var exitcode = process.ExitCode;
                process.Dispose();

                if (exitcode != 0) {
                    this.GenerationFailed(id, exitcode);
                } else {
                    this.GenerationSucceeded(id);
                }
            };

            return id;
        }

        private void GenerationSucceeded(string id) {

        }

        private void GenerationFailed(string id, int exitcode) {
        }
    }
}
