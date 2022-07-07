namespace RenderExpressConnectResponse
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

#nullable enable

    internal static class ProcessWrapper
    {
        public static async Task<string> GetCommandLineProcessResultAsync(string exename, string workingdirectory, string? arguments)
        {            
            string? eOut = null;
            using Process p = CreateProcess(exename, workingdirectory, arguments);

            p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => eOut += e.Data);
            _ = p.Start();

            // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
            p.BeginErrorReadLine();
            string output = p.StandardOutput.ReadToEnd();
            await p.WaitForExitAsync();

            return !string.IsNullOrWhiteSpace(eOut) && eOut.Contains("error", StringComparison.CurrentCultureIgnoreCase) ? throw new Exception($"The process returned following error message: {eOut}") : output;
        }

        public static string GetCommandLineProcessResult(string exename, string workingdirectory, string? arguments)
        {
            string? eOut = null;
            using Process p = CreateProcess(exename, workingdirectory, arguments);

            p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => eOut += e.Data);
            _ = p.Start();

            // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
            p.BeginErrorReadLine();
            string output = p.StandardOutput.ReadToEnd();
            _ = p.WaitForExit(1000);

            return !string.IsNullOrWhiteSpace(eOut) && eOut.Contains("error", StringComparison.CurrentCultureIgnoreCase) ? throw new Exception($"The process returned following error message: {eOut}") : output;
        }

        private static Process CreateProcess(string exename, string workingdirectory, string? arguments)
        {
            ProcessStartInfo startInfo = new();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = exename;
            if (!string.IsNullOrWhiteSpace(arguments)) startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workingdirectory;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process p = new();
            p.StartInfo = startInfo;
            return p;
        }
    }
}
