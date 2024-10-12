using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MP3Joiner
{
    public partial class MainWindow : Window
    {
        private string _draggedItem;

        public MainWindow()
        {
            InitializeComponent();
        }

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

        private void btnClearFiles_Click(object sender, RoutedEventArgs e)
        {
            mp3FileList.Items.Clear();
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
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void mp3FileList_Drop(object sender, DragEventArgs e)
        {
            // Check if files are being dragged from external source (like Windows Explorer)
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
            // Handle internal reordering
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                var listBox = sender as ListBox;
                var draggedFile = e.Data.GetData(DataFormats.StringFormat) as string;

                var target = GetItemAtPosition(listBox, e.GetPosition(listBox));

                if (target != null && draggedFile != null && draggedFile != target)
                {
                    int oldIndex = listBox.Items.IndexOf(draggedFile);
                    int newIndex = listBox.Items.IndexOf(target);

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
    }
}
