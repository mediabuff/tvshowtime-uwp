﻿using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Models;
using TVShowTime.UWP.Services;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TVShowTime.UWP.Views
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        private IHamburgerMenuService _hamburgerMenuService;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            // Retrieve colors from app resources
            var primaryBlackColor = (App.Current.Resources["PrimaryBlack"] as SolidColorBrush).Color;
            var primaryWhiteColor = (App.Current.Resources["PrimaryWhite"] as SolidColorBrush).Color;
            var primaryYellowColor = (App.Current.Resources["PrimaryYellow"] as SolidColorBrush).Color;

            // Style title bar (Desktop)
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = primaryBlackColor;
                    titleBar.ButtonForegroundColor = primaryWhiteColor;

                    titleBar.ButtonHoverBackgroundColor = primaryYellowColor;
                    titleBar.ButtonHoverForegroundColor = primaryBlackColor;

                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(
                        primaryYellowColor.A, 
                        primaryYellowColor.R, 
                        primaryYellowColor.G, 
                        (byte)(primaryYellowColor.B + 100));
                    titleBar.ButtonPressedForegroundColor = primaryBlackColor;

                    titleBar.BackgroundColor = primaryBlackColor;
                    titleBar.ForegroundColor = primaryWhiteColor;
                }

                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                Window.Current.SetTitleBar(ExtendedTitleBar);
            }

            // Style status bar (Mobile)
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = primaryBlackColor;
                    statusBar.ForegroundColor = primaryWhiteColor;
                }
            }

            // Register a handler for BackRequested events
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Manage hamburger menu service
            _hamburgerMenuService = ServiceLocator.Current.GetInstance<IHamburgerMenuService>();

            HamburgerMenuControl.ItemsSource = _hamburgerMenuService.MenuItems.Where(item => item.Type == MenuItemType.Main);
            HamburgerMenuControl.OptionsItemsSource = _hamburgerMenuService.MenuItems.Where(item => item.Type == MenuItemType.Options);
        }

        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                // Initialize hamburger menu service
                InitializeHamburgerNavigationService();
                _hamburgerMenuService.NavigateTo(ViewConstants.ToWatch);
            }

            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NavigationMenuItem navigationMenuItem)
            {
                ContentFrame.Navigate(navigationMenuItem.PageType);
            }

            if (e.ClickedItem is ActionMenuItem actionMenuItem)
            {
                actionMenuItem.Action();
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                e.Handled = true;
                ContentFrame.GoBack();
            }
        }

        #endregion

        #region Methods

        private void InitializeHamburgerNavigationService()
        {
            _hamburgerMenuService.SetHamburgerMenuElement(HamburgerMenuControl);
            _hamburgerMenuService.SetFrameElement(ContentFrame);

            _hamburgerMenuService.Configure(ViewConstants.Episode, typeof(EpisodePage));
            _hamburgerMenuService.Configure(ViewConstants.Show, typeof(ShowPage));
            _hamburgerMenuService.Configure(ViewConstants.ToWatch, typeof(ToWatchPage));
            _hamburgerMenuService.Configure(ViewConstants.Upcoming, typeof(UpcomingPage));
        }

        #endregion
    }
}