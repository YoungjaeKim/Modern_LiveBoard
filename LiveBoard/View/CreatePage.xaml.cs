﻿using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
using Windows.UI.Xaml.Data;
using LiveBoard.Common;
using LiveBoard.ViewModel;

namespace LiveBoard.View
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CreatePage : LiveBoard.Common.LayoutAwarePage
    {
        private MainViewModel _viewModel;
        public CreatePage()
        {
            this.InitializeComponent();

            var model = DataContext as MainViewModel;
            if (model != null)
                _viewModel = model;

            var viewSource = new CollectionViewSource { Source = _viewModel.ActiveBoard.Pages };
            ListViewPages.ItemsSource = viewSource.View;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void ButtonPreview_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.IsPreview = true;
            this.Frame.Navigate(typeof(ShowPage));
        }

        private void ButtonPlay_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.IsPreview = false;
            this.Frame.Navigate(typeof(ShowPage));
        }
    }
}
