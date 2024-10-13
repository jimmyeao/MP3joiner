using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Documents;
using NAudio.Lame;
using NAudio.Wave;
using System.IO;

namespace MP3Joiner
{
    public partial class MainWindow : Window
    {
        private string _draggedItem;
        private DropAdorner _dropAdorner;
        private AdornerLayer _adornerLayer;

        public MainWindow()
        {
            InitializeComponent();
            SetDisplayIcon();
        }

        // Add MP3 Files
        private void btnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "MP3 Files (*.mp3)|*.mp3"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    if (!mp3FileList.Items.Contains(file))
                    {
                        mp3FileList.Items.Add(file);
                    }
                }
            }
        }

        // Clear MP3 File List
        private void btnClearFiles_Click(object sender, RoutedEventArgs e)
        {
            mp3FileList.Items.Clear();  // Clear MP3 list
            imgAlbumArt.Source = null;   // Clear album art image
            AlbumArtPlaceholder.Visibility = Visibility.Visible;  // Show "Drop Image Here" text again
            AlbumArtBorder.Background = new SolidColorBrush(Colors.LightGray);
            txtTrackName.Clear();        // Clear track name
            txtArtistName.Clear();       // Clear artist name
           
            progressBar.Value = 0;       // Reset progress bar
        }
        private static void SetDisplayIcon()
        {
            //only run in Release

            try
            {
                // executable file
                var exePath = Environment.ProcessPath;
                if (!System.IO.File.Exists(exePath))
                {
                    return;
                }

                //DisplayIcon == "dfshim.dll,2" => 
                var myUninstallKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                string[]? mySubKeyNames = myUninstallKey?.GetSubKeyNames();
                for (int i = 0; i < mySubKeyNames?.Length; i++)
                {
                    RegistryKey? myKey = myUninstallKey?.OpenSubKey(mySubKeyNames[i], true);
                    // ClickOnce(Publish)
                    // Publish -> Settings -> Options 
                    // Publish Options -> Description -> Product name (is your DisplayName)
                    var displayName = (string?)myKey?.GetValue("DisplayName");
                    if (displayName?.Contains("YourApp") == true)
                    {
                        myKey?.SetValue("DisplayIcon", exePath + ",0");
                        break;
                    }
                }
                MP3Joiner.Properties.Settings.Default.IsFirstRun = false;
                MP3Joiner.Properties.Settings.Default.Save();
            }catch{ }
            
        }

        // Handle Mouse Move to start drag operation for reordering
        private void mp3FileList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mp3FileList.SelectedItem != null)
            {
                _draggedItem = mp3FileList.SelectedItem as string;
                if (_draggedItem != null)
                {
                    DragDrop.DoDragDrop(mp3FileList, _draggedItem, DragDropEffects.Move);
                }
            }
        }

        private void mp3FileList_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) && !e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void mp3FileList_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effects = DragDropEffects.Move;

                var listBox = sender as ListBox;
                var target = GetItemAtPosition(listBox, e.GetPosition(listBox));

                // Remove the previous adorner if any
                RemoveAdorner();

                if (target != null)
                {
                    ListBoxItem container = listBox.ItemContainerGenerator.ContainerFromItem(target) as ListBoxItem;
                    if (container != null)
                    {
                        _adornerLayer = AdornerLayer.GetAdornerLayer(container);

                        // Determine whether to place the line above or below
                        bool isAbove = e.GetPosition(container).Y < container.ActualHeight / 2;

                        // Create and add the adorner
                        _dropAdorner = new DropAdorner(container, isAbove);
                        _adornerLayer.Add(_dropAdorner);
                    }
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
                RemoveAdorner();
            }
        }

        private void RemoveAdorner()
        {
            if (_dropAdorner != null && _adornerLayer != null)
            {
                _adornerLayer.Remove(_dropAdorner);
                _dropAdorner = null;
            }
        }
        private void JoinMP3Files(string[] mp3Files, string outputFile, int bitrate, string artist, string trackName, BitmapImage albumArt)
        {
            try
            {
                // Join MP3 files
                using (var writer = new LameMP3FileWriter(outputFile, new WaveFormat(44100, 2), bitrate))
                {
                    foreach (string mp3File in mp3Files)
                    {
                        using (var reader = new Mp3FileReader(mp3File))
                        {
                            reader.CopyTo(writer);
                        }
                    }
                }

                // Tag the output file with metadata
                TagOutputFile(outputFile, artist, trackName, albumArt);

                MessageBox.Show("MP3 files combined successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error joining MP3 files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to tag the MP3 file with metadata
        private void TagOutputFile(string outputFile, string artist, string trackName, BitmapImage albumArt)
        {
            try
            {
                var tfile = TagLib.File.Create(outputFile);

                tfile.Tag.Performers = new[] { artist };
                tfile.Tag.Title = trackName;

                if (albumArt != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Convert BitmapImage to byte array
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(albumArt));
                        encoder.Save(ms);

                        tfile.Tag.Pictures = new[] { new TagLib.Picture(ms.ToArray()) };
                    }
                }

                tfile.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error tagging MP3 file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handler for the "Join Files" button click
        private async void btnJoinFiles_Click(object sender, RoutedEventArgs e)
        {
            // Fetch MP3 files from the list
            var mp3Files = mp3FileList.Items.Cast<string>().ToArray();

            if (mp3Files.Length == 0)
            {
                MessageBox.Show("No MP3 files selected.");
                return;
            }

            // Fetch metadata
            string artist = txtArtistName.Text;
            string trackName = txtTrackName.Text;
            
            BitmapImage albumArt = imgAlbumArt.Source as BitmapImage;

            // Use a SaveFileDialog to prompt for output file location
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "MP3 Files (*.mp3)|*.mp3",
                DefaultExt = "mp3",
                Title = "Save Joined MP3 File"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string outputFileName = saveFileDialog.FileName;

                // Reset progress bar
                progressBar.Value = 0;

                try
                {
                    await JoinMP3FilesAsync(mp3Files, outputFileName, artist, trackName, albumArt, ReportProgress);
                    MessageBox.Show("MP3 files joined successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
                finally
                {
                    // Reset the progress bar after completion
                    progressBar.Value = 0;
                }
            }
        }



        private void ReportProgress(int progress)
        {
            Dispatcher.Invoke(() => { progressBar.Value = progress; });
        }
        private void ApplyMetadata(string outputFile, string artist, string trackName, BitmapImage albumArt)
        {
            var tfile = TagLib.File.Create(outputFile);
            tfile.Tag.Performers = new[] { artist };
            tfile.Tag.Title = trackName;

            if (albumArt != null)
            {
                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(albumArt));
                    encoder.Save(ms);

                    tfile.Tag.Pictures = new[] { new TagLib.Picture(ms.ToArray()) };
                }
            }
            try
            {
                tfile.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error tagging MP3 file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Asynchronous method to join MP3 files
        private async Task JoinMP3FilesAsync(string[] mp3Files, string outputFile, string artist, string trackName, BitmapImage albumArt, Action<int> reportProgress)
        {
            await Task.Run(() =>
            {
                long totalBytes = mp3Files.Sum(file => new FileInfo(file).Length);
                long processedBytes = 0;

                using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    foreach (var file in mp3Files)
                    {
                        using (var reader = new Mp3FileReader(file))
                        {
                            Mp3Frame frame;
                            while ((frame = reader.ReadNextFrame()) != null)
                            {
                                outputStream.Write(frame.RawData, 0, frame.RawData.Length);
                                processedBytes += frame.RawData.Length;

                                int progressPercentage = (int)((double)processedBytes / totalBytes * 100);
                                reportProgress(progressPercentage);
                            }
                        }
                    }
                }

                // Apply metadata (this step happens only after all files are joined)
                ApplyMetadata(outputFile, artist, trackName, albumArt);
            });
        }

        private void mp3FileList_Drop(object sender, DragEventArgs e)
        {
            RemoveAdorner(); // Remove the adorner when the drop occurs

            // Handle file drop
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files.Where(f => f.EndsWith(".mp3")))
                {
                    if (!mp3FileList.Items.Contains(file))
                    {
                        mp3FileList.Items.Add(file);
                    }
                }
            }

            // Handle reordering of items within the list
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                var listBox = sender as ListBox;
                var draggedFile = e.Data.GetData(DataFormats.StringFormat) as string;

                var target = GetItemAtPosition(listBox, e.GetPosition(listBox));

                if (target != null && draggedFile != null && draggedFile != target)
                {
                    int oldIndex = listBox.Items.IndexOf(draggedFile);
                    int newIndex = listBox.Items.IndexOf(target);

                    // Remove from old position and insert in the new position
                    if (oldIndex != -1 && newIndex != -1)
                    {
                        listBox.Items.Remove(draggedFile);
                        listBox.Items.Insert(newIndex, draggedFile);
                    }
                }
            }

            e.Handled = true;
        }


        private string GetItemAtPosition(ListBox listBox, Point position)
        {
            var element = listBox.InputHitTest(position) as UIElement;

            while (element != null && !(element is ListBoxItem))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            return element != null ? (listBox.ItemContainerGenerator.ItemFromContainer(element) as string) : null;
        }

        private void AlbumArtBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var imageFile = files.FirstOrDefault();

                if (imageFile != null && (imageFile.EndsWith(".jpg") || imageFile.EndsWith(".png")))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imageFile);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    imgAlbumArt.Source = bitmap;
                    AlbumArtPlaceholder.Visibility = Visibility.Collapsed;
                    AlbumArtBorder.Background = Brushes.Transparent;
                }
            }

            e.Handled = true;
        }

        private void AlbumArtBorder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Any(file => file.EndsWith(".jpg") || file.EndsWith(".png")))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }

            e.Handled = true;
        }

        private void btnreadidv3tag_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a file is selected in the ListBox
            if (mp3FileList.SelectedItem == null)
            {
                MessageBox.Show("Please select a file to read ID3 tags.", "No File Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ensure only one file is selected
            if (mp3FileList.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select only one file to read ID3 tags.", "Multiple Files Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Get the selected file path
                string selectedFile = mp3FileList.SelectedItem as string;

                // Use TagLib to read the file's ID3 tags
                var file = TagLib.File.Create(selectedFile);

                // Check and set track name if available
                if (!string.IsNullOrEmpty(file.Tag.Title))
                {
                    txtTrackName.Text = file.Tag.Title;
                }
                else
                {
                    txtTrackName.Clear(); // Clear if no track name found
                }

                // Check and set artist name if available
                if (file.Tag.Performers.Length > 0)
                {
                    txtArtistName.Text = file.Tag.Performers[0];
                }
                else
                {
                    txtArtistName.Clear(); // Clear if no artist name found
                }

                // Set album art if available
                if (file.Tag.Pictures.Length > 0)
                {
                    var picture = file.Tag.Pictures[0];
                    using (var ms = new MemoryStream(picture.Data.Data))
                    {
                        BitmapImage albumArt = new BitmapImage();
                        albumArt.BeginInit();
                        albumArt.StreamSource = ms;
                        albumArt.CacheOption = BitmapCacheOption.OnLoad;
                        albumArt.EndInit();
                        imgAlbumArt.Source = albumArt; // Set the album art in the Image control
                        AlbumArtPlaceholder.Visibility = Visibility.Collapsed; // Hide the "Drop Image Here" text
                    }
                }
                else
                {
                    imgAlbumArt.Source = null; // Clear if no album art found
                    AlbumArtPlaceholder.Visibility = Visibility.Visible; // Show "Drop Image Here" if no image
                }


                MessageBox.Show("ID3 tags read successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading ID3 tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
