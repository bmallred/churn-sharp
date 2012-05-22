using System;

namespace churn_sharp
{
    /// <summary>
    ///   Commit class.
    /// </summary>
    public class Commit
    {
        /// <summary>
        ///   Gets or sets the date.
        /// </summary>
        /// <value>
        ///   The date.
        /// </value>
        public DateTime Date { get; set; }
        
        /// <summary>
        ///   Gets or sets the author.
        /// </summary>
        /// <value>
        ///   The author.
        /// </value>
        public string Author { get; set; }
        
        /// <summary>
        ///   Gets or sets the lines of change.
        /// </summary>
        /// <value>
        ///   The lines of change.
        /// </value>
        public int LinesOfChange { get; set; }
    }
}
