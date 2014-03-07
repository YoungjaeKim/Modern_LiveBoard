﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using GalaSoft.MvvmLight.Ioc;
using LiveBoard.Common;

using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226
using LiveBoard.View;
using LiveBoard.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace LiveBoard
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public sealed partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton Application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used when the application is launched to open a specific file, to display
		/// search results, and so forth.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs args)
		{
			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active

			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();
				//Associate the frame with a SuspensionManager key                                
				SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

				if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					// Restore the saved session state only when appropriate
					try
					{
						await SuspensionManager.RestoreAsync();
					}
					catch (SuspensionManagerException)
					{
						//Something went wrong restoring state.
						//Assume there is no state and continue
					}
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;

				/*
				 * 참바 설정
				 * */
				// Settings 참 설정.
				//SettingsPane.GetForCurrentView().CommandsRequested += (sender, eventArgs) =>
				//{
				//	// Privacy Policy 연결.
				//	var policyCommand = new SettingsCommand("PrivacySettingsCommand", "Privacy Policy", delegate
				//	{
				//		// create a new instance of the flyout
				//		var settings = new SettingsFlyout
				//		{
				//			HeaderBrush = new SolidColorBrush(Color.FromArgb(255, 6, 195, 255)),
				//			HeaderText = "Privacy Policy",
				//			SmallLogoImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SmallLogo.png")),
				//			Content = new PrivacyPolicyControl(),
				//		};
				//		// open it
				//		settings.IsOpen = true;
				//	});

				//	eventArgs.Request.ApplicationCommands.Add(policyCommand);
				//};

			}
			if (rootFrame.Content == null)
			{
				// When the navigation stack isn't restored navigate to the first page,
				// configuring the new page by passing required information as a navigation
				// parameter
				if (!rootFrame.Navigate(typeof(StartPage)))
				{
					throw new Exception("Failed to create initial page");
				}
			}
			// Ensure the current window is active
			Window.Current.Activate();
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private async void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			await SuspensionManager.SaveAsync();
			deferral.Complete();
		}

		protected override void OnWindowCreated(WindowCreatedEventArgs args)
		{
			SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
		}

		private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
		{
			// 설정 참바.
			args.Request.ApplicationCommands.Add(new SettingsCommand(
				"PrivacySettingsCommand", "Privacy Policy", handler =>
				{
					var customSettingFlyout = new PrivacyPolicySettingsFlyout();
					customSettingFlyout.Show();
				}));

			args.Request.ApplicationCommands.Add(new SettingsCommand(
				"InquirySettingsCommand", "문의하기 (이메일)", async handler =>
				{
					await Launcher.LaunchUriAsync(new Uri("mailto:hello@bapul.net"));
				}));
		}
	}
}
