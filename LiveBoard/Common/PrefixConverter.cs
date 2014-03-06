using System;
using Windows.UI.Xaml.Data;

namespace LiveBoard.Common
{
	/// <summary>
	/// parameter�� ���� ���� 'parameter: value' ������ ������ ��.
	/// </summary>
	public class PrefixConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (parameter == null)
				return value;
			return String.Format("{0}: {1}", parameter, value ?? "N/A");
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}