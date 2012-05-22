using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace churn_sharp
{
    /// <summary>
    ///   Command line functionality.
    /// </summary>
    public static class CommandLine
    {
        /// <summary>
        ///   Executes the specified working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="date">The date.</param>
        /// <returns>Enumerable of commits.</returns>
        public static IEnumerable<Commit> Execute(string workingDirectory, DateTime date)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "hg";
            proc.StartInfo.Arguments = string.Format("churn -d \"{0}\"", date.ToShortDateString());
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.WorkingDirectory = workingDirectory;

            if (proc.Start())
            {
                var stdOutput = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                foreach (var line in stdOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    var regex = new Regex(@"[a-zA-z0-9\@\.\-]*\w", RegexOptions.Multiline);
                    var matches = regex.Matches(line);

                    for (int i = 0; i < matches.Count; i += 2)
                    {
                        Commit commit = null;

                        try
                        {
                            int temp;

                            commit = new Commit()
                            {
                                Date = date,
                                Author = matches[i].ToString(),
                                LinesOfChange = int.TryParse(matches[i + 1].ToString(), out temp) ? temp : 0
                            };
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                        if (commit != null)
                        {
                            yield return commit;
                        }
                    }
                }
            }
        }
    }
}
