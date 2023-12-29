using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using NAudio.Lame;
using System.ComponentModel;

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
            var mp3Files = data.Mp3Files;

            int totalFiles = mp3Files.Count;
            int processedFiles = 0;
            UpdateStatus("Starting file join process...");
            using (var writer = new LameMP3FileWriter(outputFile, new WaveFormat(44100, 2), LAMEPreset.STANDARD))
            {
                foreach (string mp3File in mp3Files)
                {
                    UpdateStatus("Working on " + mp3File.ToString());
                    using (var reader = new Mp3FileReader(mp3File))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writer.Write(buffer, 0, bytesRead);
                        }
                    }
                    processedFiles++;
                    int progressPercentage = (int)((double)processedFiles / totalFiles * 100);
                    worker.ReportProgress(progressPercentage);
                }
                UpdateStatus("File written " + outputFile.ToString());
            }
        }

        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                statusText.Text = message;
            });
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