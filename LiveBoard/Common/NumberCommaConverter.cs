using System;
using Windows.UI.Xaml.Data;

namespace LiveBoard.Common
{
	/// <summary>
	/// ���ڸ� 3�ڸ����� �޸��� �־��ش�.
	/// </summary>
	public class NumberCommaConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var text = value != null ? value.ToString() : "0";
			return CommaInt(text, 3);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// �޸� �־���.
		/// </summary>
		/// <param name="integerNumber"></param>
		/// <param name="comma"></param>
		/// <returns></returns>
		public static string CommaInt(string integerNumber, int comma)
		{
			// http://stackoverflow.com/a/16892651/361100
			string output = "";
			int q = integerNumber.Length % comma;
			int x = q == 0 ? comma : q;
			int i = -1;
			foreach (char y in integerNumber)
			{
				i++;
				if (i == x) output += "," + y;
				else if (i > comma && (i - x) % comma == 0) output += "," + y;
				else output += y;

			}
			return output;
		}
	}
}