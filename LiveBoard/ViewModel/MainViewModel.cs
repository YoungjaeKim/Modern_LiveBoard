using System;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveBoard.Model;
using LiveBoard.Model.Page;

namespace LiveBoard.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private Board _activeBoard = new Board();
        private bool _isPreview;
        private bool _isPlaying;

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
        }
        #region ICommand
        public ICommand SaveCmd { get { return new RelayCommand(Save); } }
        public ICommand AddPageCmd { get { return new RelayCommand(AddPage); } }
        public ICommand DeletePageCmd { get { return new RelayCommand<IPage>(DeletePage); } }

        #endregion ICommand

        /// <summary>
        /// �ҷ�����.
        /// </summary>
        /// <param name="file"></param>
        public void Load(StorageFile file)
        {
            if (ActiveBoard == null)
                ActiveBoard = new Board();

        }

        /// <summary>
        /// ����ϱ�
        /// </summary>
        /// <param name="board"></param>
        public void Play(Board board)
        {
            if (!IsPlaying)
                IsPlaying = true;
            else
                return;

            var timer = new DispatcherTimer();
            timer.Tick += PlayTimerEventHandler;
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Start();
            
        }

        private void PlayTimerEventHandler(object sender, object e)
        {
            // NOTE: YoungjaeKim: ������� ����.
            // TODO: Ÿ�̸Ӹ��� ��������� ��.
            throw new NotImplementedException();
        }

        /// <summary>
        /// �����ϱ�.
        /// </summary>
        public void Save()
        {
            ActiveBoard.SaveAsync();
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
                TemplateCode = ""
            };
            ActiveBoard.Pages.Add(page);
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        /// <param name="page"></param>
        private void DeletePage(IPage page)
        {
            // TODO: ������ �����ϱ�.
            ActiveBoard.Pages.Remove(page);
        }

        #region Properties
        /// <summary>
        /// ���� ����.
        /// </summary>
        public Board ActiveBoard
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

        #endregion Properties

    }
}