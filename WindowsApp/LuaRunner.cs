using System.Diagnostics;

namespace WowWtfSync.WindowsApp
{
    /*
     * This class runs Lua code by utilizing the Process library to run lua54.exe.
     */

    public static class LuaRunner
    {
        /*
         * This method runs lua54.exe with the given arguments and opens a message
         * box indicating success or error.
         */
        public static void Run(List<string> arguments)
        {
            try
            {
                using (Process process = new Process())
                {
                    string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = workingDirectory + @"\LuaApp\lua-5.4.2\lua54.exe";
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.Arguments = String.Join(' ', arguments.ToArray());
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    // Synchronously read the standard error/output of the spawned process.
                    StreamReader stdoutReader = process.StandardOutput;
                    string stdout = stdoutReader.ReadToEnd();
                    StreamReader stderrReader = process.StandardError;
                    string stderr = stderrReader.ReadToEnd();

                    if (stderr != "")
                    {
                        MessageBox.Show(
                            stderr,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            stdout,
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
