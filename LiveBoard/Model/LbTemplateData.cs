using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace LiveBoard.Model
{
	/// <summary>
	/// 템플릿 데이터
	/// </summary>
	public class LbTemplateData
	{
		public string Key { get; set; }
		public string Name { get; set; }
		public Type ValueType { get; set; }
		public object Data { get; set; }
		public object DefaultData { get; set; }
		public bool IsHidden { get; set; }

		public static LbTemplateData FromXml(XElement xElement)
		{
			// <Data Key="Url" Name="Header" ValueType="String" DefaultData="" />

			var tData = new LbTemplateData { Key = xElement.Attribute("Key").Value, Name = xElement.Attribute("Name").Value };

			switch (xElement.Attribute("ValueType").Value.ToLower())
			{
				case "string":
					tData.ValueType = typeof(string);
					tData.DefaultData = xElement.Attribute("DefaultValue").Value;
					break;
				case "int":
				case "integer":
					tData.ValueType = typeof(int);
					tData.DefaultData = int.Parse(xElement.Attribute("DefaultValue").Value);
					break;
				case "double":
					tData.ValueType = typeof(double);
					tData.DefaultData = double.Parse(xElement.Attribute("DefaultValue").Value);
					break;
				default:
					// typeof(IEnumerable<string>) 이것이 변환. System.Collections.Generic.IEnumerable`1[System.String]
					tData.ValueType = Type.GetType(xElement.Attribute("ValueType").Value);
					tData.DefaultData = new ObservableCollection<string>();
					break;
			}
			return tData;
		}
	}
}