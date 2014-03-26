using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace LiveBoard.Common
{
	public class NullToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (parameter == null || String.IsNullOrEmpty(parameter.ToString()))
				return value != null ? Visibility.Visible : Visibility.Collapsed;
			// '!' ���� �Ķ���Ϳ� ������ �ݴ�� ���ش�.
			return value != null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}