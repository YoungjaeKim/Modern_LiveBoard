using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Linq;
using Windows.UI;
using GalaSoft.MvvmLight;

namespace LiveBoard.PageTemplate.Model
{
	/// <summary>
	/// ���ø� ������
	/// </summary>
	public class LbPageData : ObservableObject
	{
		private object _data;
		private string _name;
		private string _key;
		private object _defaultData;
		private bool _isHidden;
		private Type _valueType;
		private string _description;

		/// <summary>
		/// ���� ����Ű �̸�.
		/// </summary>
		public string Key
		{
			get { return _key; }
			set
			{
				_key = value;
				RaisePropertyChanged("Key");
			}
		}

		/// <summary>
		/// �� �̸�.
		/// </summary>
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				RaisePropertyChanged("Name");
			}
		}

		/// <summary>
		/// ��. DataType�̶�� �ؾ� �°����� ���������� �̷��� ����.
		/// </summary>
		public Type ValueType
		{
			get { return _valueType; }
			set
			{
				_valueType = value;
				RaisePropertyChanged("ValueType");
			}
		}

		/// <summary>
		/// ������
		/// </summary>
		public object Data
		{
			get
			{
				if (_data == null)
					return DefaultData;
				return _data;
			}
			set
			{
				_data = value;
				RaisePropertyChanged("Data");
			}
		}

		/// <summary>
		/// �⺻��
		/// </summary>
		public object DefaultData
		{
			get { return _defaultData; }
			set
			{
				_defaultData = value;
				RaisePropertyChanged("DefaultData");
			}
		}

		/// <summary>
		/// ���� ����
		/// </summary>
		public bool IsHidden
		{
			get { return _isHidden; }
			set
			{
				_isHidden = value;
				RaisePropertyChanged("IsHidden");
			}
		}

		/// <summary>
		/// �� ����
		/// </summary>
		public string Description
		{
			get { return _description; }
			set
			{
				_description = value;
				RaisePropertyChanged("Description");
			}
		}

		/// <summary>
		/// ������ ������ �����Ϳ� �����͸� �Է��Ѵ�.
		/// </summary>
		/// <param name="pageData"></param>
		/// <param name="valueType"></param>
		/// <param name="data"></param>
		/// <param name="defaultData"></param>
		/// <returns></returns>
		public static LbPageData Parse(LbPageData pageData, string valueType, string data, string defaultData = "")
		{
			valueType = !String.IsNullOrEmpty(valueType) ? valueType : (pageData.ValueType != null ? pageData.ValueType.Name : null);
			if (valueType == null)
				throw new ArgumentNullException("valueType");

			switch (valueType.ToLower())
			{
				case "":
				case "string":
					pageData.ValueType = typeof(string);
					if (String.IsNullOrEmpty(data))
						data = defaultData;
					pageData.Data = data;
					break;
				case "int":
				case "int16":
				case "int32":
				case "integer":
					pageData.ValueType = typeof(int);
					if (String.IsNullOrEmpty(data))
						data = defaultData;
					pageData.Data = !String.IsNullOrWhiteSpace(data) ? int.Parse(data) : 0;
					break;
				case "int64":
				case "float":
				case "double":
					pageData.ValueType = typeof(double);
					if (String.IsNullOrEmpty(data))
						data = defaultData;
					pageData.Data = !String.IsNullOrWhiteSpace(data) ? double.Parse(data) : 0d;
					break;
				case "color":
					pageData.ValueType = typeof(Color);
					if (String.IsNullOrEmpty(data))
						data = defaultData;
					string colorcode = data.Replace("#", "");
					if (String.IsNullOrWhiteSpace(colorcode)) // �⺻���� ����.
						colorcode = "000000";
					int argb = Int32.Parse(colorcode, NumberStyles.HexNumber);
					if (colorcode.Length > 6)
					{
						pageData.Data = Color.FromArgb((byte)((argb & -16777216) >> 0x18), // 0x18=24
							(byte)((argb & 0xff0000) >> 0x10), // 0x10=16
							(byte)((argb & 0xff00) >> 8),
							(byte)(argb & 0xff));
					}
					else
					{
						pageData.Data = Color.FromArgb(0xff,
							(byte)((argb & 0xff0000) >> 0x10), // 0x10=16
							(byte)((argb & 0xff00) >> 8),
							(byte)(argb & 0xff));
					}

					break;
				default:
					// typeof(IEnumerable<string>) �̰��� ��ȯ. System.Collections.Generic.IEnumerable`1[System.String]
					pageData.ValueType = Type.GetType(valueType);
					pageData.Data = new ObservableCollection<string>();
					break;
			}
			return pageData;
		}

		/// <summary>
		/// XML���� �ν��Ͻ� ����.
		/// </summary>
		/// <param name="templateKey">�θ� ���ø� Ű</param>
		/// <param name="xElement"></param>
		/// <returns></returns>
		public static LbPageData FromXml(string templateKey, XElement xElement)
		{
			// <Data Key="Url" Name="Header" ValueType="String" DefaultData="" />
			var tData = new LbPageData
			{
				Key = xElement.Attribute("Key").Value,
				Name = xElement.Attribute("Name").Value,
				Description = xElement.Attribute("Description").Value,
				IsHidden = bool.Parse(xElement.Attribute("IsHidden") != null
					? xElement.Attribute("IsHidden").Value
					: bool.FalseString),
				DefaultData = xElement.Attribute("DefaultValue") != null ? xElement.Attribute("DefaultValue").Value : ""
			};

			// �ٱ��� ó��.
			if (String.IsNullOrEmpty(templateKey))
			{
				try
				{
					var loader = new Windows.ApplicationModel.Resources.ResourceLoader("TemplateList");
					var localeName = loader.GetString(String.Format("{0}/{1}/{2}", templateKey, tData.Key, "Name"));
					if (!String.IsNullOrEmpty(localeName))
						tData.Name = localeName;
					var localeDescription = loader.GetString(String.Format("{0}/{1}/{2}", templateKey, tData.Key, "Description"));
					if (!String.IsNullOrEmpty(localeDescription))
						tData.Description = localeDescription;
				}
				catch (Exception)
				{
				}
			}

			return Parse(tData, xElement.Attribute("ValueType").Value, xElement.Attribute("DefaultValue").Value, xElement.Attribute("DefaultValue").Value);
		}

		/// <summary>
		/// XML�� ���� ���.
		/// </summary>
		/// <param name="isVerbose"><c>true</c>, ��� ���� ���. <c>false</c>, Key�� Data�� ���.</param>
		/// <returns></returns>
		public XElement ToXml(bool isVerbose = false)
		{
			XElement xElement;
			if (!isVerbose)
			{
				xElement = new XElement("Data",
					new XAttribute("Key", Key ?? ""),
					new XAttribute("ValueType", ValueType.Name ?? "String"),
					new XAttribute("Data", Data ?? "")
					);
			}
			else
			{
				xElement = new XElement("Data",
					new XAttribute("Key", Key ?? ""),
					new XAttribute("Data", Data ?? ""),
					new XAttribute("DefaultValue", DefaultData ?? ""),
					new XAttribute("ValueType", ValueType != null ? ValueType.Name : "String"),
					new XAttribute("IsHidden", IsHidden.ToString()),
					new XAttribute("Name", Name ?? ""),
					new XAttribute("Description", Description ?? "")
					);
			}
			return xElement;
		}
	}
}