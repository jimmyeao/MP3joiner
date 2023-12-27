// YourViewModel.cs
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Windows;

namespace MP3Joiner
{
    public class YourViewModel
    {
        public ObservableCollection<string> FileList { get; set; }
        public YourDropHandler DropHandler { get; private set; }

        public YourViewModel()
        {
            FileList = new ObservableCollection<string>();
            DropHandler = new YourDropHandler(this);
        }
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
            if (dropInfo.Data is string && dropInfo.TargetItem is string)
            {
                var droppedData = dropInfo.Data as string;
                var targetData = dropInfo.TargetItem as string;

                // Access FileList through the _viewModel reference
                var index = _viewModel.FileList.IndexOf(targetData);

                _viewModel.FileList.Remove(droppedData);
                _viewModel.FileList.Insert(index, droppedData);
            }
        }
    }
}



