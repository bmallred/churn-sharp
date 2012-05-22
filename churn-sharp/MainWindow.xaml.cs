using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace churn_sharp
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        ///   Html file resource.
        /// </summary>
        private static FileInfo _fileInfo = new FileInfo(System.IO.Path.Combine("Resources", "history.html"));

        /// <summary>
        ///   Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.browser.Navigate(_fileInfo.FullName);
        }

        /// <summary>
        /// Handles the Click event of the graphIt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">  The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void graphIt_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button right quick.
            this.graphIt.IsEnabled = false;
            this.startDate.Background = Brushes.White;
            this.workingDirectory.Background = Brushes.White;

            // Check for valid inputs first.
            if (!string.IsNullOrWhiteSpace(this.workingDirectory.Text) && this.startDate.SelectedDate.HasValue)
            {
                var directory = this.workingDirectory.Text;
                var start = this.startDate.SelectedDate.Value;
                var stop = this.stopDate.SelectedDate.HasValue ? this.stopDate.SelectedDate.Value : DateTime.Now;

                Action action = delegate
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += delegate
                    {
                        var history = new List<Commit>();
                        var daysBetween = stop.Subtract(start).Days;

                        for (int i = 0; i < daysBetween + 1; i++)
                        {
                            history.AddRange(CommandLine.Execute(directory, start.AddDays(i)));
                        }
                    
                        // Parse and display new graph.
                        var parser = new Parser(System.IO.Path.Combine("Resources", "flot-template.txt"));
                        parser.ReadTemplate();
                        parser.Write(_fileInfo, history.ToArray());
                    };

                    worker.RunWorkerCompleted += delegate
                    {
                        // Browse to our file.
                        this.browser.Refresh();

                        // Enable the graphing button again.
                        this.graphIt.IsEnabled = true;
                    };

                    worker.RunWorkerAsync();
                };

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
            }
            else
            {
                // Notify of invalid inputs.
                if (string.IsNullOrWhiteSpace(this.workingDirectory.Text))
                {
                    this.workingDirectory.Background = Brushes.Red;
                }

                if (!this.startDate.SelectedDate.HasValue)
                {
                    this.startDate.Background = Brushes.Red;
                }

                this.graphIt.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the SizeChanged event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">  The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.browser.Source != null)
            {
                this.browser.Refresh();
            }
        }
    }
}
