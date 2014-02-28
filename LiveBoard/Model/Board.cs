﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using LiveBoard.PageTemplate.Model;
using Newtonsoft.Json;

namespace LiveBoard.Model
{
	/// <summary>
	/// 보드 오브젝트.
	/// </summary>
	public class Board : ObservableObject
	{
		/// <summary>
		/// Title of board
		/// </summary>
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				RaisePropertyChanged("Title");
			}
		}

		/// <summary>
		/// Author name
		/// </summary>
		public string Author
		{
			get { return _author; }
			set
			{
				_author = value;
				RaisePropertyChanged("Author");
			}
		}

		/// <summary>
		/// Author contact point
		/// </summary>
		public string AuthorEmail
		{
			get { return _authorEmail; }
			set
			{
				_authorEmail = value;
				RaisePropertyChanged("AuthorEmail");
			}
		}

		/// <summary>
		/// [ReadOnly] Loop or run once.
		/// <para>[읽기전용] 단발성 실행 또는 무한루프.</para>
		/// <value>false</value> if <see cref="LoopCount"/> is 0. Otherwise, <value>true</value>. 
		/// </summary>
		[JsonIgnore]
		public bool IsLoop
		{
			get { return LoopCount != 0; }
			set
			{
				if (value)
					LoopCount = -1;
				RaisePropertyChanged("IsLoop");
				RaisePropertyChanged("LoopCount");
			}
		}

		/// <summary>
		/// Loop count.
		/// <para>Default is -1 (infinite)</para>
		/// </summary>
		public int LoopCount
		{
			get { return _loopCount; }
			set
			{
				_loopCount = value;
				RaisePropertyChanged("IsLoop");
				RaisePropertyChanged("LoopCount");
			}
		}


		/// <summary>
		/// Limit time to run
		/// </summary>
		public DateTime RunUntil
		{
			get { return _runUntil; }
			set
			{
				_runUntil = value;
				RaisePropertyChanged("RunUntil");
			}
		}


		/// <summary>
		/// List of page
		/// </summary>
		[JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
		public ObservableCollection<IPage> Pages
		{
			get { return _pages; }
			set
			{
				_pages = value;
				RaisePropertyChanged("Pages");
			}
		}


		#region Private Variables
		private ObservableCollection<IPage> _pages = new ObservableCollection<IPage>();
		private string _title;
		private string _author;
		private string _authorEmail;
		private bool _isLoop = true;
		private int _loopCount = -1; // default -1 (infinite)
		private DateTime _runUntil;

		#endregion Private Variables


		public static IPage ExportToPage(XElement xElement, IEnumerable<LbTemplate> templates)
		{
			/**
			 * var t = new LbTemplate
			{
				Key = "SingleUrlImage",
				DisplayName = "Simple image viewer from web",
				Description = "Show single image",
				TemplateView = "SimpleUrlImage",
				TemplateModel = "SingleStringPage",
				DataList = new List<LbPageData>()
				{
					new LbPageData()
					{
						Key = "Url", 
						DefaultData = "http://inserbia.info/news/wp-content/uploads/2013/05/grizzly-650x487.jpg",
						Name = "헤더 정보",
						ValueType = typeof(String)
					}
				}
			};
			 * */
			LbTemplate template = null;
			foreach (var t in templates)
			{
				if (t.Key.Equals(xElement.Attribute("TemplateKey").Value))
				{
					template = t;
					break;
				}
			}
			if (template == null)
				return null;

			var model = Type.GetType("LiveBoard.PageTemplate.Model." + template.TemplateModel);
			if (model == null)
				throw new ArgumentException("Template model not found.");

			var page = (IPage)Activator.CreateInstance(model);
			page.TemplateKey = template.Key;
			page.View = template.TemplateView;
			page.Title = xElement.Attribute("Title").Value;
			page.Description = xElement.Attribute("Description").Value;
			page.Duration = TimeSpan.FromMilliseconds(Convert.ToDouble(xElement.Attribute("Duration").Value));
			page.IsVisible = Convert.ToBoolean(xElement.Attribute("IsVisible").Value);
			page.Guid = xElement.Attribute("Guid").Value;
			page.ViewOption = xElement.Attribute("ViewOption").Value;
			page.Data = template.DataList;

			for (int i = 0; i < ((List<LbPageData>)page.Data).Count; i++)
			{
				foreach (var dataElement in xElement.Element("DataList").Elements("Data"))
				{
					if (((List<LbPageData>)page.Data)[i].Key.Contains(dataElement.Attribute("Key").Value))
					{
						((List<LbPageData>)page.Data)[i]
							= LbPageData.Parse(((List<LbPageData>)page.Data)[i], dataElement.Attribute("Data").Value);
						break;
					}
				}
			}
			return page;
		}

		/// <summary>
		/// XML로 출력.
		/// </summary>
		/// <returns></returns>
		public XElement ToXml()
		{
			var xElement = new XElement("LiveBoard",
				new XAttribute("IsLoop", IsLoop),
				new XAttribute("LoopCount", LoopCount),
				new XAttribute("RunUntil", RunUntil.ToBinary()),
				new XAttribute("Author", Author ?? ""),
				new XAttribute("Title", Title ?? ""),
				new XAttribute("AuthorEmail", AuthorEmail ?? ""),
				new XElement("Pages", Pages.Select(x => x.ToXml())));
			return xElement;
		}

		/// <summary>
		/// XML에서 구축.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="templates"></param>
		/// <returns></returns>
		public static Board FromXml(XElement xml, IEnumerable<LbTemplate> templates)
		{
			var board = new Board()
			{
				Title = xml.Attribute("Title").Value,
				Author = xml.Attribute("Author").Value,
				AuthorEmail = xml.Attribute("AuthorEmail").Value,
				IsLoop = Convert.ToBoolean(xml.Attribute("IsLoop").Value),
				LoopCount = Convert.ToInt32(xml.Attribute("LoopCount").Value),
				RunUntil = DateTime.FromBinary(Convert.ToInt64(xml.Attribute("RunUntil").Value)),
				Pages = new ObservableCollection<IPage>(xml.Element("Pages").Elements("Page").Select(p => ExportToPage(p, templates)))
			};
			return board;
		}
	}
}
