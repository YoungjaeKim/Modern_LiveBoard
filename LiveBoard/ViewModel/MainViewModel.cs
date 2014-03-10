using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveBoard.Common;
using LiveBoard.Model;
using LiveBoard.PageTemplate.Model;

namespace LiveBoard.ViewModel
{
	/// <summary>
	/// ���� ���
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
		private BoardViewModel _activeBoard = new BoardViewModel();
		private bool _isPreview;
		private bool _isPlaying;
		private DateTime _startTime;
		private IPage _currentPage;
		private int _currentPageElapsedRatio;
		private int _currentRemainedSecond;
		readonly DispatcherTimer _timer;
		private bool _currentPageStarted;
		private string _popupMessage;
		private TemplateListViewModel _templates;
		private ObservableCollection<string> _recentBoards;
		private int _maxRecentBoardCount = 5;
		private RecentOpenedListViewModel _recentOpenedList;
		private BoardViewModel _selectedBoard = new BoardViewModel();

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			////if (IsInDesignMode)
			////{
			////    // Code runs in Blend --> create design time data.
			////}
			////else
			////{
			////    // Code runs "for real"
			////}

			/* �� ���� ����: ����
			 * 1. �ε�.
			 * 2. MainViewModel.Play�� ActiveBoard.Start�� ȣ���ϰ� �̸� ���� ù ������ �ε� �� EVT_STARTING ����. Starting���� ShowPage Navigate.
			 * 3. ShowPage���� �� �޽����� ActiveBoard�� CurrentPage�� �ε��ϰ� �׿� �´� XAML �ε�.
			 * 4. ���ο� XAML�� EVT_LOADED �ε�.
			 * 5. �׿� ���� �޽����� ���� �������� ���� RemainedSecond�� ����.
			 * 6. tick���� --RemainedSecond.
			 * 7. RemainedSecond < 0  �� �Ǹ� MoveNext..
			 * 
			 * �� �޽����� 3��: ready, loaded, start.
			*/
			/* �� ���ۿ���: ����
			 * 1. MainViewModel���� Ÿ�̸� ����, ù������ ������, Finishing ��� ����.
			 * 2. ShowPage ���� �̸� ���� Finished �� Page Close.
			 * */

			_timer = new DispatcherTimer();
			_timer.Tick += PlayTimerEventHandler;
			_timer.Interval = new TimeSpan(0, 0, 1);

			Messenger.Default.Register<GenericMessage<LbMessage>>(this, message =>
			{
				Debug.WriteLine("* MainViewModel Received Message: " + message.Content.MessageType.ToString());
				switch (message.Content.MessageType)
				{
					case LbMessageType.EVT_SHOW_FINISHING:
						Stop(ActiveBoard);
						break;
					case LbMessageType.EVT_PAGE_STARTED:
						// ������ ���� �Ϸ�.
						CurrentRemainedSecond = (int)CurrentPage.Duration.TotalSeconds;
						CurrentPageStarted = true;

						_timer.Start();
						break;
					case LbMessageType.EVT_SHOW_STARTED:
						// ù������ �ε�.
						ActiveBoard.Start();
						CurrentPage = ActiveBoard.Board.Pages[ActiveBoard.CurrentIndex];
						break;
					case LbMessageType.EVT_PAGE_CREATING:
						var page = generatePageFromTemaplate(message.Content.Data as LbTemplate);
						ActiveBoard.Board.Pages.Add(page);
						break;
				}
			});

			// �ʱ�ȭ�ϸ鼭 TemplateList.xml ������ �ε��Ѵ�.
			Templates = new TemplateListViewModel();
		}

		#region ICommand
		public ICommand SaveCmd { get { return new RelayCommand<BoardViewModel>(Save); } }
		public ICommand AddPageCmd { get { return new RelayCommand(AddPage); } }
		public ICommand DeletePageCmd { get { return new RelayCommand<Object>(DeletePage, CanDeletePage); } }
		public ICommand PlayCmd { get { return new RelayCommand<BoardViewModel>(Play); } }
		public ICommand PreviewCmd { get { return new RelayCommand<BoardViewModel>(Preview); } }
		public ICommand StopCmd { get { return new RelayCommand<BoardViewModel>(Stop); } }
		#endregion ICommand

		/// <summary>
		/// ����
		/// </summary>
		/// <param name="obj"></param>
		private void Stop(BoardViewModel obj)
		{
			_timer.Stop();
			ActiveBoard.Stop();
			IsPlaying = false;
			IsPreview = false;
			Messenger.Default.Send(new GenericMessage<LbMessage>(new LbMessage()
			{
				MessageType = LbMessageType.EVT_SHOW_FINISHED
			}));
		}

		public void Preview(BoardViewModel board)
		{
			IsPreview = true;
			Play(board);
		}

		/// <summary>
		/// ����ϱ�
		/// </summary>
		/// <param name="board"></param>
		public void Play(BoardViewModel board)
		{
			if (board == null)
				board = ActiveBoard;
			ActiveBoard = board;

			if (!IsPlaying)
				IsPlaying = true;
			else
			{
				Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
				{
					MessageType = LbMessageType.ERROR,
					Data = LbError.IsPlayingTrue,
					Board = board.Board
				}));
				return;
			}

			Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
			{
				MessageType = LbMessageType.EVT_SHOW_STARTING,
				Data = ActiveBoard,
				Board = board.Board
			}));

			StartTime = DateTime.Now;

		}


		/// <summary>
		/// �� �ʸ��� ����Ǵ� �ʴ��� �̺�Ʈ �ڵ鷯
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayTimerEventHandler(object sender, object e)
		{
			Debug.WriteLine("tick at {0} and Elapsed {1}", DateTime.Now.ToString("u"), (DateTime.Now - StartTime).ToString("g"));

			// TODO: ������ ���� �Ѵ�.

			--CurrentRemainedSecond;
			if (CurrentRemainedSecond < 0)
			{
				_timer.Stop();
				try
				{
					if (IsPlaying)
					{
						CurrentPage = ActiveBoard.Board.Pages[ActiveBoard.MoveNext()];
					}
					Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
					{
						MessageType = LbMessageType.EVT_PAGE_FINISHING
					}));
				}
				catch (IndexOutOfRangeException)
				{
					Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
					{
						MessageType = LbMessageType.EVT_SHOW_FINISHING
					}));
				}
			}

			CurrentPageElapsedRatio = (int)(((CurrentPage.Duration.TotalSeconds - CurrentRemainedSecond) / CurrentPage.Duration.TotalSeconds) * 100);

			Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
			{
				MessageType = LbMessageType.EVT_TICK
			}));
		}

		internal bool EnsureUnsnapped()
		{
			// FilePicker APIs will not work if the application is in a snapped state.
			// If an app wants to show a FilePicker while snapped, it must attempt to unsnap first
			bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
			if (!unsnapped)
			{
				Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
				{
					MessageType = LbMessageType.ERROR,
					Data = LbError.UnSnappedToSave
				}));
			}

			return unsnapped;
		}

		/// <summary>
		/// �����ϱ�.
		/// </summary>
		public async void Save(BoardViewModel boardViewModel)
		{
			if (!EnsureUnsnapped())
				return;

			if (boardViewModel == null)
				boardViewModel = ActiveBoard;

			var savePicker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
			// Dropdown of file types the user can save the file as
			savePicker.FileTypeChoices.Add("LiveBoard file", new List<string>() { ".lvbd" });
			// Default file name if the user does not type one in or select a file to replace
			savePicker.SuggestedFileName = BoardViewModel.CreateNewFilename();
			savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			StorageFile file = await savePicker.PickSaveFileAsync();
			if (file == null)
			{
				PopupMessage = "Operation cancelled.";
			}
			else
			{
				// Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
				CachedFileManager.DeferUpdates(file);
				// write to file
				String content = boardViewModel.Board.ToXml().ToString();

				await FileIO.WriteTextAsync(file, content, UnicodeEncoding.Utf8);
				// Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
				// Completing updates may require Windows to ask for user input.
				FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
				StorageApplicationPermissions.MostRecentlyUsedList.Add(file, file.Path);

				// TODO: �Ϸ� �˾�. ���� ����.
				if (status == FileUpdateStatus.Complete)
				{
					PopupMessage = "File " + file.Name + " was saved.";
				}
				else
				{
					PopupMessage = "File " + file.Name + " couldn't be saved.";
				}
			}
		}

		public string PopupMessage
		{
			get { return _popupMessage; }
			set
			{
				_popupMessage = value;
				RaisePropertyChanged("PopupMessage");
			}
		}

		/// <summary>
		/// ������ �߰�
		/// </summary>
		private void AddPage()
		{
			// ���ø�������Ʈ���� ������ ����.
			var t = new LbTemplate
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
						Name = "��� ����",
						ValueType = typeof(String)
					}
				}
			};

			var pageExample1 = generatePageFromTemaplate(t);
			ActiveBoard.Board.Pages.Add(pageExample1);

			//var page4 = new SingleStringPage
			//{
			//	TemplateKey = "Countdown",
			//	Title = "Ÿ��Ʋ " + DateTime.Now.Ticks,
			//	Duration = TimeSpan.FromSeconds(6.0d),
			//	IsVisible = true,
			//	Guid = Guid.NewGuid().ToString(),
			//	View = "OneNumberCount",
			//	Data = new List<LbPageData>(){
			//		new LbPageData()
			//			{
			//				Key = "Number",
			//				Data = 5,
			//				Name = "��� ����",
			//				ValueType = typeof(int)
			//			}
			//	}
			//};

			//ActiveBoard.Board.Pages.Add(page4);

			//var page = new SingleStringPage
			//{
			//	TemplateKey = "StaticWebView",
			//	Title = "Ÿ��Ʋ",
			//	Duration = TimeSpan.FromSeconds(7.0d),
			//	IsVisible = true,
			//	Guid = Guid.NewGuid().ToString(),
			//	View = "StaticWebView",
			//	Data = new List<LbPageData>(){
			//		new LbPageData()
			//			{
			//				Key = "URL",
			//				Data = "http://www.naver.com",
			//				Name = "���ͳ� �ּ�",
			//				ValueType = typeof(string)
			//			}
			//	}
			//};

			//ActiveBoard.Board.Pages.Add(page);

			//var page2 = new RssList()
			//{
			//	TemplateKey = "RssList",
			//	Title = "Ÿ��Ʋ " + DateTime.Now.Ticks.ToString(),
			//	Duration = TimeSpan.FromSeconds(5.0d),
			//	IsVisible = true,
			//	Guid = Guid.NewGuid().ToString(),
			//	View = "SimpleList",
			//	Data = new List<LbPageData>()
			//	{
			//		new LbPageData()
			//		{
			//			Key = "Header",
			//			Name = "Ÿ��Ʋ��",
			//			ValueType = typeof(string),
			//			Data = "���� View �α� ���"
			//		},
			//		new LbPageData()
			//		{
			//			Key="RSS",
			//			Name="RSS �ּ�",
			//			ValueType = typeof(string),
			//			Data = "http://v.daum.net/best/rss"
			//		},
			//		new LbPageData()
			//		{
			//			Key="Feeds",
			//			Name="��µ� Feed ���",
			//			ValueType = typeof(IEnumerable<string>),
			//			IsHidden = true
			//		}
			//	}
			//};
			//ActiveBoard.Board.Pages.Add(page2);

		}

		/// <summary>
		/// �����ͷ� ���ø� ���.
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		private IPage generatePageFromTemaplate(LbTemplate template)
		{
			if (template == null)
				throw new ArgumentNullException("template");

			var model = Type.GetType("LiveBoard.PageTemplate.Model." + template.TemplateModel);
			if (model == null)
				throw new ArgumentException("Template model not found.");

			var page = (IPage)Activator.CreateInstance(model);
			page.TemplateKey = template.Key;
			page.View = template.TemplateView;
			page.Title = "Page " + (ActiveBoard.Board.Pages.Count + 1);
			page.Duration = TimeSpan.FromSeconds(5.0d);
			page.IsVisible = true;
			page.Guid = Guid.NewGuid().ToString();
			page.Data = template.DataList;

			return page;
		}

		/// <summary>
		/// ������(��) ����.
		/// </summary>
		/// <param name="obj">���� ������. ������������ ��� <![CDATA[IEnumerable<IPage>]]> �Ǵ� �ϳ��� �������� ��� <see cref="IPage"/> ������Ʈ.</param>
		private void DeletePage(Object obj)
		{
			if (obj is IEnumerable<IPage>)
			{
				foreach (var page in (IEnumerable<IPage>)obj)
					ActiveBoard.Board.Pages.Remove(page);
			}
			else if (obj is IPage)
			{
				ActiveBoard.Board.Pages.Remove((IPage)obj);
			}
		}

		private bool CanDeletePage(Object arg)
		{
			return arg != null && (!(arg is IEnumerable<IPage>) || !(arg is IPage));
		}

		/// <summary>
		/// ���ø� ���.
		/// </summary>
		public TemplateListViewModel Templates
		{
			get { return _templates; }
			set
			{
				_templates = value;
				RaisePropertyChanged("Templates");
			}
		}

		#region Properties

		/// <summary>
		/// ���� ������
		/// </summary>
		public IPage CurrentPage
		{
			get { return _currentPage; }
			set
			{
				_currentPage = value;

				if (_currentPage != null)
					_currentPage.PrepareToLoadAsync(); // Command to Load if it needs to be prepared.

				// Send events.
				MessengerInstance.Send(new GenericMessage<LbMessage>(this, new LbMessage()
				{
					MessageType = LbMessageType.EVT_PAGE_READY,
					Data = CurrentPage
				}));
				RaisePropertyChanged("CurrentPage");
			}
		}

		public bool CurrentPageStarted
		{
			get { return _currentPageStarted; }
			set
			{
				_currentPageStarted = value;
				RaisePropertyChanged("CurrentPageStarted");
			}
		}

		/// <summary>
		/// ���� �����̵��� ���� �ð�
		/// </summary>
		public int CurrentRemainedSecond
		{
			get { return _currentRemainedSecond; }
			set
			{
				_currentRemainedSecond = value;
				RaisePropertyChanged("CurrentRemainedSecond");
			}
		}

		/// <summary>
		/// ������ ���� ���
		/// </summary>
		public BoardViewModel SelectedBoard
		{
			get { return _selectedBoard; }
			set
			{
				_selectedBoard = value;
				RaisePropertyChanged("SelectedBoard");
			}
		}

		/// <summary>
		/// ���� ����.
		/// </summary>
		public BoardViewModel ActiveBoard
		{
			get { return _activeBoard; }
			set
			{
				_activeBoard = value;
				RaisePropertyChanged("ActiveBoard");
			}
		}


		/// <summary>
		/// �̸����� ���
		/// </summary>
		public bool IsPreview
		{
			get { return _isPreview; }
			set
			{
				_isPreview = value;
				RaisePropertyChanged("IsPreview");
			}
		}

		/// <summary>
		/// �� ���� �ð�.
		/// </summary>
		public DateTime StartTime
		{
			get { return _startTime; }
			set
			{
				_startTime = value;
				RaisePropertyChanged("StartTime");
			}
		}

		/// <summary>
		/// ��� ��
		/// </summary>
		public bool IsPlaying
		{
			get { return _isPlaying; }
			set
			{
				_isPlaying = value;
				RaisePropertyChanged("IsPlaying");
			}
		}

		/// <summary>
		/// �ش� ������ ���� �ð�. 0~100 value
		/// </summary>
		public int CurrentPageElapsedRatio
		{
			get { return _currentPageElapsedRatio; }
			set
			{
				_currentPageElapsedRatio = value;
				RaisePropertyChanged("CurrentPageElapsedRatio");
			}
		}

		public RecentOpenedListViewModel RecentOpenedList
		{
			get
			{
				if (_recentOpenedList == null)
					_recentOpenedList = new RecentOpenedListViewModel();

				_recentOpenedList.Clear();
				foreach (var entry in StorageApplicationPermissions.MostRecentlyUsedList.Entries)
				{
					_recentOpenedList.Add(new LbFile()
					{
						Token = entry.Token,
						Metadata = entry.Metadata
					});
					Debug.WriteLine(entry.Metadata);
				}

				return _recentOpenedList;
			}
			set
			{
				_recentOpenedList = value;
				RaisePropertyChanged("RecentOpenedList");
			}
		}

		#endregion Properties

	}
}