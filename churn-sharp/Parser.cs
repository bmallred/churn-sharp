using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace churn_sharp
{
    /// <summary>
    ///   Parser class.
    /// </summary>
    public class Parser
    {
        private List<TextBlock> _blocks;
        private FileInfo _templateFile;

        /// <summary>
        ///   Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        public Parser()
        {
            this._blocks = new List<TextBlock>();
            this._templateFile = null;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="TemplateFile">The template file.</param>
        public Parser(string TemplateFile)
            : this(new FileInfo(TemplateFile))
        {
            // Stubble.
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="TemplateFile">The template file.</param>
        public Parser(FileInfo TemplateFile)
        {
            this._blocks = new List<TextBlock>();
            this._templateFile = TemplateFile;
        }

        /// <summary>
        ///   Read template.
        /// </summary>
        public void ReadTemplate()
        {
            // Clear previous text blocks stored.
            this._blocks.Clear();

            if (this._templateFile == null)
            {
                return;
            }

            if (this._templateFile.Exists)
            {
                using (TextReader reader = new StreamReader(this._templateFile.OpenRead()))
                {
                    StringBuilder newBlock = new StringBuilder();

                    // Check if there is more content to read.
                    while (reader.Peek() >= 0)
                    {
                        // Read the latest line.
                        string line = reader.ReadLine();

                        // Determine if there are any tokens that need to be processed.
                        if (line.ToLower().Contains("{authors}"))
                        {
                            this._blocks.Add(new TextBlock(newBlock.ToString()));
                            newBlock.Clear();
                        }
                        else if (line.ToLower().Contains("{/authors}"))
                        {
                            int idx = line.IndexOf("{/authors}", 0, StringComparison.CurrentCultureIgnoreCase);
                            int trashLength = idx + "{/authors}".Length;
                            string postText = line.Substring(trashLength, line.Length - trashLength);

                            this._blocks.Last(x => x.Type == BlockType.Author).InnerBlocks.Add(new TextBlock(newBlock.ToString(), BlockType.Plain, string.Empty));
                            this._blocks.Last(x => x.Type == BlockType.Author).PostText = postText;
                            newBlock.Clear();
                        }
                        else if (line.ToLower().Contains("{dates}"))
                        {
                            this._blocks.Add(new TextBlock(newBlock.ToString(), BlockType.Author));
                            newBlock.Clear();
                        }
                        else if (line.ToLower().Contains("{/dates}"))
                        {
                            int idx = line.IndexOf("{/dates}", 0, StringComparison.CurrentCultureIgnoreCase);
                            int trashLength = idx + "{/dates}".Length;
                            string postText = line.Substring(trashLength, line.Length - trashLength);

                            this._blocks.Last().InnerBlocks.Add(new TextBlock(newBlock.ToString(), BlockType.Date, postText));
                            newBlock.Clear();
                        }
                        else
                        {
                            newBlock.AppendLine(line);
                        }
                    }

                    // If there is anything still in the read buffer add it as a new block.
                    if (newBlock.Length > 0)
                    {
                        this._blocks.Add(new TextBlock(newBlock.ToString()));
                        newBlock.Clear();
                    }
                }
            }
        }

        /// <summary>
        ///   Read template.
        /// </summary>
        /// <param name="TemplateFile">The template file.</param>
        public void ReadTemplate(string TemplateFile)
        {
            this.ReadTemplate(new FileInfo(TemplateFile));
        }

        /// <summary>
        ///   Read template.
        /// </summary>
        /// <param name="TemplateFile">The template file.</param>
        public void ReadTemplate(FileInfo TemplateFile)
        {
            this._templateFile = TemplateFile;
            this.ReadTemplate();
        }

        /// <summary>
        ///   To stream.
        /// </summary>
        /// <param name="commiters">The commiters.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <returns>A stream containing markup contents.</returns>
        public Stream ToStream(Commit[] commiters, Stream outputStream = null)
        {
            MemoryStream memory = new MemoryStream();
            StreamWriter writer;

            if (outputStream == null)
            {
                writer = new StreamWriter(memory);
            }
            else
            {
                writer = new StreamWriter(outputStream);
            }

            foreach (TextBlock block in this._blocks)
            {
                switch (block.Type)
                {
                    case BlockType.Author:

                        var uniqueAuthors = commiters.Select(x => x.Author).Distinct();

                        foreach (var author in uniqueAuthors)
                        {
                            this.WriteAuthors(
                                    writer,
                                    block,
                                    commiters.Where(x => x.Author.Equals(author)).ToArray());

                            if (!string.IsNullOrEmpty(block.PostText))
                            {
                                var last = uniqueAuthors.Last();

                                // Add the separator if it is not the last route.
                                if (last != author)
                                {
                                    writer.Write(block.PostText);
                                    writer.Flush();
                                }
                            }
                        }

                        break;

                    case BlockType.Date:

                        // Stub.

                        break;

                    default:

                        int idx;

                        // Search for y-min tokens.
                        while ((idx = block.Text.IndexOf("{ymin}", StringComparison.CurrentCultureIgnoreCase)) > -1)
                        {
                            block.Text = block.Text.Replace(
                                block.Text.Substring(idx, "{ymin}".Length), 
                                commiters.Min(x => x.LinesOfChange).ToString());
                        }

                        // Search for y-max tokens.
                        while ((idx = block.Text.IndexOf("{ymax}", StringComparison.CurrentCultureIgnoreCase)) > -1)
                        {
                            block.Text = block.Text.Replace(
                                block.Text.Substring(idx, "{ymax}".Length),
                                commiters.Max(x => x.LinesOfChange).ToString());
                        }

                        // Search for y-min tokens.
                        while ((idx = block.Text.IndexOf("{xmin}", StringComparison.CurrentCultureIgnoreCase)) > -1)
                        {
                            block.Text = block.Text.Replace(
                                block.Text.Substring(idx, "{xmin}".Length),
                                ConvertToJavascriptTimestamp(commiters.Min(x => x.Date)).ToString());
                        }

                        // Search for y-max tokens.
                        while ((idx = block.Text.IndexOf("{xmax}", StringComparison.CurrentCultureIgnoreCase)) > -1)
                        {
                            block.Text = block.Text.Replace(
                                block.Text.Substring(idx, "{xmax}".Length),
                                ConvertToJavascriptTimestamp(commiters.Max(x => x.Date)).ToString());
                        }

                        // Nothing to see here.
                        writer.Write(block.Text);
                        writer.Flush();

                        break;
                }
            }

            writer.Flush();
            writer.Close();
            memory.Seek(0, SeekOrigin.Begin);
            return memory;
        }

        /// <summary>
        ///   Writes the specified target file.
        /// </summary>
        /// <param name="targetFile">The target file.</param>
        /// <param name="commits">The commits.</param>
        /// <param name="append">  If set to <c>true</c> [append].</param>
        public void Write(string targetFile, Commit[] commits, bool append = false)
        {
            this.Write(new FileInfo(targetFile), commits, append);
        }

        /// <summary>
        ///   Writes the specified target file.
        /// </summary>
        /// <param name="targetFile">The target file.</param>
        /// <param name="commits">The commits.</param>
        /// <param name="append">  If set to <c>true</c> [append].</param>
        public void Write(FileInfo targetFile, Commit[] commits, bool append = false)
        {
            this.ToStream(commits, targetFile.Open(append ? FileMode.Append : FileMode.Create));
        }

        /// <summary>
        ///   Convert standard Unix time stamp to JavaScript time stamp which is in milliseconds vice seconds.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>Javascript timestamp.</returns>
        private static double ConvertToJavascriptTimestamp(DateTime dateTime)
        {
            return GetUnixEpoch(dateTime) * 1000;
        }

        /// <summary>
        ///   Get unix epoch.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>Unix time stamp.</returns>
        private static double GetUnixEpoch(DateTime dateTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc) - epoch;

            return unixTime.TotalSeconds;
        }

        /// <summary>
        ///   Write authors.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="block">The block.</param>
        /// <param name="commits">The commits.</param>
        private void WriteAuthors(StreamWriter writer, TextBlock block, Commit[] commits)
        {
            var token = "{author}";

            // Work with the copy dummy.
            string line = block.Text;
            int idx;

            // Search for author tokens.
            while ((idx = line.IndexOf(token, StringComparison.CurrentCultureIgnoreCase)) > -1)
            {
                line = line.Replace(line.Substring(idx, token.Length), commits.First().Author);
            }

            writer.Write(line);
            writer.Flush();

            // Dig through any inner blocks.
            foreach (var innerBlock in block.InnerBlocks)
            {
                if (innerBlock.Type == BlockType.Date)
                {
                    this.WriteDates(writer, innerBlock, commits);
                }
                else if (innerBlock.Type == BlockType.Plain)
                {
                    writer.Write(innerBlock.Text);
                }
            }
        }

        /// <summary>
        ///   Write dates.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="block">The block.</param>
        /// <param name="commits">The commits.</param>
        private void WriteDates(StreamWriter writer, TextBlock block, Commit[] commits)
        {
            var token = "{date}";
            var lineToken = "{lines}";

            foreach (var commit in commits)
            {
                // Work with the copy dummy.
                string line = block.Text;
                int idx;

                // Search for date tokens.
                while ((idx = line.IndexOf(token, StringComparison.CurrentCultureIgnoreCase)) > -1)
                {
                    line = line.Replace(line.Substring(idx, token.Length), ConvertToJavascriptTimestamp(commit.Date).ToString());
                }

                // Search for line tokens.
                while ((idx = line.IndexOf(lineToken, StringComparison.CurrentCultureIgnoreCase)) > -1)
                {
                    line = line.Replace(line.Substring(idx, lineToken.Length), commit.LinesOfChange.ToString());
                }

                if (!string.IsNullOrEmpty(block.PostText))
                {
                    var last = commits.Last();

                    // Add the separator if it is not the last route.
                    if (last != commit)
                    {
                        line += block.PostText;
                    }
                }

                // Finally...
                writer.Write(line);
                writer.Flush();
            }
        }
    }
}
