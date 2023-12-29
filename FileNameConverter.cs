using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace MP3joiner
{
    [ValueConversion(typeof(string), typeof(string))]
    public class FileNameConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fullPath = value as string;
            return Path.GetFileName(fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}