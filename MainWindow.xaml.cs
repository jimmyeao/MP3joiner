using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;

using System.Windows.Controls;
using System.Windows.Input;
using NAudio.Wave;
using System.Collections.Generic;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop;

namespace MP3Joiner
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new YourViewModel();
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
                YourViewModel viewModel = DataContext as YourViewModel;
                foreach (string file in openFileDialog.FileNames)
                {
                    viewModel.FileList.Add(file);
                }
            }
        }

        private void btnJoinFiles_Click(object sender, RoutedEventArgs e)
        {
            // Call your method to join the files in fileList
        }
    }
}

