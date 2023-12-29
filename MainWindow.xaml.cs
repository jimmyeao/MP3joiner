using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using NAudio.Lame;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Animation;
using MP3joiner;

namespace MP3Joiner
{
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker = new BackgroundWorker();
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new YourViewModel();
            SetupBackgroundWorker();
            Uri themeUri;
            themeUri = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
            var existingTheme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == themeUri);
            if (existingTheme == null)
            {
                existingTheme = new ResourceDictionary() { Source = themeUri };
                Application.Current.Resources.MergedDictionaries.Add(existingTheme);
            }

            // Remove the other theme
           

           
        }

        #endregion Public Constructors

        #region Private Methods
        private void SetupBackgroundWorker()
        {
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var data = e.Argument as dynamic;
            string outputFile = data.OutputFile;

            // Cast mp3Files to a specific type, for example, List<string>
            var mp3Files = data.Mp3Files as List<string>;

            if (mp3Files != null)
            {
                long totalBytes = mp3Files.Sum(file => new System.IO.FileInfo(file).Length);

                long processedBytes = 0;

                Dispatcher.Invoke(() => UpdateStatus("Starting file join process..."));

                using (FileStream outputFs = new FileStream(outputFile, FileMode.Create))
                {
                    foreach (string mp3File in mp3Files)
                    {
                        Dispatcher.Invoke(() => UpdateStatus($"Processing {mp3File}..."));

                        using (Mp3FileReader reader = new Mp3FileReader(mp3File))
                        {
                            if ((outputFs.Position == 0) && (reader.Id3v2Tag != null))
                            {
                                outputFs.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                            }

                            Mp3Frame frame;
                            while ((frame = reader.ReadNextFrame()) != null)
                            {
                                outputFs.Write(frame.RawData, 0, frame.RawData.Length);
                                processedBytes += frame.RawData.Length;

                                int progressPercentage = (int)((double)processedBytes / totalBytes * 100);
                                worker.ReportProgress(progressPercentage);
                            }
                        }
                    }
                }

                Dispatcher.Invoke(() => UpdateStatus($"Joining complete. Output file: {outputFile}"));

                // Inside your background worker completion logic
                Dispatcher.Invoke(() =>
                {
                    AnimateProgressBarToZero();
                    UpdateStatus("Processing complete.");

                    var completionDialog = new CompletionDialog();
                    completionDialog.Owner = this; // Set the main window as the owner
                    completionDialog.ShowDialog();
                });

            }
        }


        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                statusText.Text = message;
            });
        }

        private void AnimateProgressBarToZero()
        {
            var animation = new DoubleAnimation
            {
                From = progressBar.Value,
                To = 0,
                Duration = TimeSpan.FromSeconds(1)
            };
            progressBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation);
        }


        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        private void btnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "MP3 Files|*.mp3"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var viewModel = DataContext as YourViewModel;
                if (viewModel.FileList.Any())
                {
                    var dialog = new AddFilesDialog();
                    dialog.Owner = this; // Set the main window as the owner
                    dialog.ShowDialog();

                    if (dialog.DialogResult == "NewList")
                    {
                        viewModel.FileList.Clear(); // Clears the existing list for a new list
                    }
                    else if (dialog.DialogResult == "Cancel")
                    {
                        return; // Cancel the operation
                    }
                }

                foreach (string file in openFileDialog.FileNames)
                {
                    viewModel.FileList.Add(new FileInfo { FilePath = file });
                }
            }
        }



        private void btnJoinFiles_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "MP3 Files|*.mp3"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var viewModel = DataContext as YourViewModel;
                var mp3Files = viewModel.FileList.Select(f => f.FilePath).ToList();
                var data = new { OutputFile = saveFileDialog.FileName, Mp3Files = mp3Files };
                worker.RunWorkerAsync(data);
            }
        }

        #endregion Private Methods
    }
}