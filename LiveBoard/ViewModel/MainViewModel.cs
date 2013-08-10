using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveBoard.Common;
using LiveBoard.Model;
using LiveBoard.Model.Page;

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
		DispatcherTimer _timer;
		private bool _currentPageStarted;

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
				}
			});
		}
		#region ICommand
		public ICommand SaveCmd { get { return new RelayCommand(Save); } }
		public ICommand AddPageCmd { get { return new RelayCommand(AddPage); } }
		public ICommand DeletePageCmd { get { return new RelayCommand<IPage>(DeletePage); } }
		public ICommand PlayCmd { get { return new RelayCommand<BoardViewModel>(Play); } }
		public ICommand PreviewCmd { get { return new RelayCommand<BoardViewModel>(Preview); } }
		public ICommand StopCmd { get { return new RelayCommand<BoardViewModel>(Stop); } }
		#endregion ICommand

		/// <summary>
		/// �ҷ�����.
		/// </summary>
		/// <param name="file"></param>
		public void Load(StorageFile file)
		{
			if (ActiveBoard == null)
				ActiveBoard = new BoardViewModel();

		}

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
			if (!IsPlaying)
				IsPlaying = true;
			else
			{
				Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
				{
					MessageType = LbMessageType.ERROR,
					Data = "IsPlaying is true"
				}));
				return;
			}

			Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
			{
				MessageType = LbMessageType.EVT_SHOW_STARTING,
				Data = ActiveBoard
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
			if (!IsPlaying)
				return;

			--CurrentRemainedSecond;
			if (CurrentRemainedSecond < 0)
			{
				_timer.Stop();
				try
				{
					CurrentPage = ActiveBoard.Board.Pages[ActiveBoard.MoveNext()];
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

			//Messenger.Default.Send(new GenericMessage<LbMessage>(this, new LbMessage()
			//{
			//	MessageType = LbMessageType.EVT_TICK
			//}));
		}

		/// <summary>
		/// �����ϱ�.
		/// </summary>
		public void Save()
		{
			ActiveBoard.Board.SaveAsync();
		}

		/// <summary>
		/// ������ �߰�
		/// </summary>
		private void AddPage()
		{
			var page = new SingleTextPage
			{
				Title = "Ÿ��Ʋ " + DateTime.Now.Ticks.ToString(),
				Duration = TimeSpan.FromSeconds(5.0d),
				IsVisible = true,
				Guid = new Guid(),
				TemplateCode = "BlankPage_SingleText"
			};
			ActiveBoard.Board.Pages.Add(page);
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

			if (obj is IPage)
			{
				ActiveBoard.Board.Pages.Remove((IPage)obj);
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

		#endregion Properties

	}
}