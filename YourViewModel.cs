// YourViewModel.cs
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MP3Joiner
{
    public class YourViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FileInfo> FileList { get; } = new ObservableCollection<FileInfo>();
       
        public YourDropHandler DropHandler { get; private set; }

        public YourViewModel()
        {
            
            DropHandler = new YourDropHandler(this);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class FileInfo
    {
        public string FilePath { get; set; }
        public string FileName => System.IO.Path.GetFileName(FilePath);
    }
    public class YourDropHandler : IDropTarget
    {
        private readonly YourViewModel _viewModel;

        public YourDropHandler(YourViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            // Logic for handling drag over event
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is FileInfo && dropInfo.TargetItem is FileInfo)
            {
                var droppedData = dropInfo.Data as FileInfo;
                var targetData = dropInfo.TargetItem as FileInfo;

                // Find the index of the target data in the file list
                var index = _viewModel.FileList.IndexOf(targetData);

                if (index != -1)
                {
                    // Remove the dropped data and insert it at the target index
                    _viewModel.FileList.Remove(droppedData);
                    _viewModel.FileList.Insert(index, droppedData);
                }
            }
        }

    }
}




