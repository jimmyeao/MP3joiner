using Microsoft.Win32; // Import the Microsoft.Win32 namespace for OpenFileDialog and SaveFileDialog
using MP3joiner; // Import the MP3joiner namespace
using NAudio.Wave; // Import the NAudio.Wave namespace
using System; // Import the System namespace
using System.Collections.Generic; // Import the System.Collections.Generic namespace
using System.ComponentModel; // Import the System.ComponentModel namespace
using System.IO; // Import the System.IO namespace
using System.Linq; // Import the System.Linq namespace
using System.Windows; // Import the System.Windows namespace
using System.Windows.Media.Animation; // Import the System.Windows.Media.Animation namespace

namespace MP3Joiner
{
    public partial class MainWindow : Window
    {
        #region Private Fields

        private BackgroundWorker worker = new BackgroundWorker(); // Create a BackgroundWorker object

        #endregion Private Fields

        #region Public Constructors

        // Constructor for the MainWindow class
        public MainWindow()
        {
            // Initialize the components of the MainWindow
            InitializeComponent();

            // Set the DataContext of the MainWindow to a new instance of YourViewModel
            this.DataContext = new YourViewModel();

            // Call the method to setup the BackgroundWorker
            SetupBackgroundWorker();

            // Create a new Uri for the theme file
            Uri themeUri;
            themeUri = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");

            // Check if the theme is already added to the application resources
            var existingTheme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == themeUri);

            // If the theme is not already added, add it to the application resources
            if (existingTheme == null)
            {
                existingTheme = new ResourceDictionary() { Source = themeUri };
                Application.Current.Resources.MergedDictionaries.Add(existingTheme);
            }
        }

        #endregion Public Constructors

        #region Private Methods
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Show copy cursor
            }
            else
            {
                e.Effects = DragDropEffects.None; // Show no-entry cursor
            }

            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            // Check if the drop is from an external source (Windows Explorer)
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var mp3Files = files.Where(file => System.IO.Path.GetExtension(file).Equals(".mp3", StringComparison.OrdinalIgnoreCase)).ToArray();
                HandleFileDrop(mp3Files);
                e.Handled = true; // Mark the event as handled
            }
            else
            {
                // If the drop is not from an external source, 
                // it might be an internal drag-and-drop for reordering.
                // Do not set e.Handled to true here.
                // This allows GongSolutions.Wpf.DragDrop to handle internal reordering.
            }
        }


        private void HandleFileDrop(string[] files)
        {
            var viewModel = DataContext as YourViewModel;
            if (viewModel == null) return;
            var firstadded = true;
            // Your existing logic for handling file drops
            foreach (string file in files)
            {
               
                    // Add each file to your file list, for example:
                    // Assuming 'fileList' is your ObservableCollection<string> that's bound to the UI
                    if (viewModel.FileList.Any() && viewModel.FileList.All(f => f.FilePath != file) && firstadded == true)
                    {
                        // Create a new instance of AddFilesDialog
                        var dialog = new AddFilesDialog();

                        // Set the main window as the owner of the dialog
                        dialog.Owner = this;

                        // Show the dialog and wait for the user to close it
                        dialog.ShowDialog();

                        // Check the result of the dialog
                        if (dialog.DialogResult == "NewList")
                        {
                            viewModel.FileList.Clear(); // Clear the existing list in the view model
                            firstadded = false;
                        }
                        else if (dialog.DialogResult == "Cancel")
                        {
                            return; // Cancel the operation and return from the event handler
                        }
                    }
                    viewModel.FileList.Add(new FileInfo { FilePath = file });
                    firstadded = false;
                
            }
        }

      
        private void btnclearList_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as YourViewModel;
            viewModel.FileList.Clear();
        }

        // This method is used to animate a progress bar to zero value
        private void AnimateProgressBarToZero()
        {
            // Create a new DoubleAnimation object
            var animation = new DoubleAnimation
            {
                // Set the starting value of the animation to the current value of the progress bar
                From = progressBar.Value,
                // Set the ending value of the animation to zero
                To = 0,
                // Set the duration of the animation to 1 second
                Duration = TimeSpan.FromSeconds(1)
            };

            // Begin the animation on the ValueProperty of the progress bar
            progressBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation);
        }

        // Event handler for the btnAddFiles button click event
        private void btnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true, // Allow multiple file selection
                Filter = "MP3 Files|*.mp3" // Only show MP3 files in the file dialog
            };

            // Show the file dialog and check if the user clicked the "OK" button
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the view model associated with the current DataContext
                var viewModel = DataContext as YourViewModel;

                // Check if the FileList in the view model is not empty
                if (viewModel.FileList.Any())
                {
                    // Create a new instance of AddFilesDialog
                    var dialog = new AddFilesDialog();

                    // Set the main window as the owner of the dialog
                    dialog.Owner = this;

                    // Show the dialog and wait for the user to close it
                    dialog.ShowDialog();

                    // Check the result of the dialog
                    if (dialog.DialogResult == "NewList")
                    {
                        viewModel.FileList.Clear(); // Clear the existing list in the view model
                    }
                    else if (dialog.DialogResult == "Cancel")
                    {
                        return; // Cancel the operation and return from the event handler
                    }
                }

                // Iterate over each selected file in the file dialog
                foreach (string file in openFileDialog.FileNames)
                {
                    // Create a new instance of FileInfo and add it to the FileList in the view model
                    viewModel.FileList.Add(new FileInfo { FilePath = file });
                }
            }
        }

        // This method is called when the "Join Files" button is clicked
        private void btnJoinFiles_Click(object sender, RoutedEventArgs e)
        {
            // Create a new SaveFileDialog instance
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "MP3 Files|*.mp3" // Set the file filter to only show MP3 files
            };

            // Show the SaveFileDialog and check if the user clicked the "Save" button
            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the ViewModel associated with the current DataContext
                var viewModel = DataContext as YourViewModel;

                // Get the list of file paths from the ViewModel's FileList property
                var mp3Files = viewModel.FileList.Select(f => f.FilePath).ToList();

                // Create a new anonymous object with the output file path and the list of MP3 files
                var data = new { OutputFile = saveFileDialog.FileName, Mp3Files = mp3Files };

                // Start the background worker and pass the data object as the argument
                worker.RunWorkerAsync(data);
            }
        }

        // This method sets up the background worker by configuring its properties and event handlers
        private void SetupBackgroundWorker()
        {
            // Enable the worker to report progress
            worker.WorkerReportsProgress = true;

            // Attach the DoWork event handler to the worker's DoWork event
            worker.DoWork += Worker_DoWork;

            // Attach the ProgressChanged event handler to the worker's ProgressChanged event
            worker.ProgressChanged += Worker_ProgressChanged;
        }

        // This method updates the status text with the given message
        private void UpdateStatus(string message)
        {
            // Invoke the following code on the UI thread
            Dispatcher.Invoke(() =>
            {
                // Set the status text to the given message
                statusText.Text = message;
            });
        }

        // This method is the event handler for the DoWork event of a background worker
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Retrieve the data passed to the background worker
            var data = e.Argument as dynamic;
            string outputFile = data.OutputFile;

            // Cast mp3Files to a specific type, for example, List<string>
            var mp3Files = data.Mp3Files as List<string>;

            if (mp3Files != null)
            {
                // Calculate the total size of all mp3 files
                long totalBytes = mp3Files.Sum(file => new System.IO.FileInfo(file).Length);

                long processedBytes = 0;

                // Update the status on the UI thread
                Dispatcher.Invoke(() => UpdateStatus("Starting file join process..."));

                // Create a file stream for the output file
                using (FileStream outputFs = new FileStream(outputFile, FileMode.Create))
                {
                    // Iterate through each mp3 file
                    foreach (string mp3File in mp3Files)
                    {
                        // Update the status on the UI thread
                        Dispatcher.Invoke(() => UpdateStatus($"Processing {mp3File}..."));

                        // Create a Mp3FileReader for the current mp3 file
                        using (Mp3FileReader reader = new Mp3FileReader(mp3File))
                        {
                            // If it is the first mp3 file and it has an ID3v2 tag, write the tag to the output file
                            if ((outputFs.Position == 0) && (reader.Id3v2Tag != null))
                            {
                                outputFs.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                            }

                            Mp3Frame frame;
                            // Read each frame from the mp3 file and write it to the output file
                            while ((frame = reader.ReadNextFrame()) != null)
                            {
                                outputFs.Write(frame.RawData, 0, frame.RawData.Length);
                                processedBytes += frame.RawData.Length;

                                // Calculate the progress percentage and report it to the background worker
                                int progressPercentage = (int)((double)processedBytes / totalBytes * 100);
                                worker.ReportProgress(progressPercentage);
                            }
                        }
                    }
                }

                // Update the status on the UI thread
                Dispatcher.Invoke(() => UpdateStatus($"Joining complete. Output file: {outputFile}"));

                // Inside your background worker completion logic
                Dispatcher.Invoke(() =>
                {
                    // Reset the progress bar animation
                    AnimateProgressBarToZero();
                    // Update the status on the UI thread
                    UpdateStatus("Processing complete.");

                    // Create and show a completion dialog with the main window as the owner
                    var completionDialog = new CompletionDialog();
                    completionDialog.Owner = this;
                    completionDialog.ShowDialog();
                });
            }
        }

        // This method is an event handler for the ProgressChanged event of a worker object.
        // It is called when the progress of the worker has changed.

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Set the value of the progressBar control to the progress percentage provided in the event arguments.
            progressBar.Value = e.ProgressPercentage;
        }

        #endregion Private Methods
    }
}

