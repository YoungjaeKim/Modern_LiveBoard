﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LiveBoard.Model;
using LiveBoard.ViewModel;

namespace LiveBoard.Common
{
	public class LbEditorDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DefaultTemplate { get; set; }
		public DataTemplate IntTemplate { get; set; }
		public DataTemplate StringTemplate { get; set; }
		public DataTemplate DoubleTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			var data = item as LbTemplateData;
			if (data == null)
				return DefaultTemplate;

			if (data.ValueType == typeof(int))
				return IntTemplate;
			if (data.ValueType == typeof(string))
				return StringTemplate;
			if (data.ValueType == typeof(string))
				return DoubleTemplate;


			return DefaultTemplate;
		}

	}
}
