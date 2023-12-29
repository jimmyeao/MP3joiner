// YourViewModel.cs
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MP3Joiner
{
    public class FileInfo
    {
        #region Public Properties

        public string FileName => System.IO.Path.GetFileName(FilePath);
        public string FilePath { get; set; }

        #endregion Public Properties
    }

    public class YourDropHandler : IDropTarget
    {
        #region Private Fields

        private readonly YourViewModel _viewModel;

        #endregion Private Fields

        #region Public Constructors

        public YourDropHandler(YourViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        #endregion Public Constructors

        #region Public Methods

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

        #endregion Public Methods
    }

    public class YourViewModel : INotifyPropertyChanged
    {
        #region Public Constructors

        public YourViewModel()
        {
            DropHandler = new YourDropHandler(this);
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public YourDropHandler DropHandler { get; private set; }
        public ObservableCollection<FileInfo> FileList { get; } = new ObservableCollection<FileInfo>();

        #endregion Public Properties

        #region Protected Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Protected Methods
    }
}