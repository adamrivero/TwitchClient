﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TwitchClient.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace TwitchClient.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ProjectionViewPage : Page
    {
        public ProjectionViewPage()
        {
            this.InitializeComponent();

            ProjectedMediaTransportControls pmtcs = this._player.TransportControls as ProjectedMediaTransportControls;
            if (pmtcs != null)
                pmtcs.StopProjectingButtonClick += ProjectionViewPage_StopProjectingButtonClick;

            this._player.MediaOpened += Player_MediaOpened;
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            this._player.IsFullWindow = true;
            this._player.AreTransportControlsEnabled = true;
        }

        private void ProjectionViewPage_StopProjectingButtonClick(object sender, EventArgs e)
        {
            this.StopProjecting();
        }



        public async Task<bool> SetMediaSource(Uri source, TimeSpan position)
        {
            await this._player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this._player.Source = source;
                this._player.Position = position;
                this._player.Play();
            });
            return true;
        }

        public MediaElement Player
        {
            get { return this._player; }
        }

        ProjectionViewBroker broker = null;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            broker = (ProjectionViewBroker)e.Parameter;
            broker.ProjectedPage = this;

            // Listen for when it's time to close this view
            broker.ProjectionViewPageControl.Released += thisViewControl_Released;
        }

        private async void thisViewControl_Released(object sender, EventArgs e)
        {
            // There are two major cases where this event will get invoked:
            // 1. The view goes unused for some time, and the system cleans it up
            // 2. The app calls "StopProjectingAsync"
            broker.ProjectionViewPageControl.Released -= thisViewControl_Released;
            await broker.ProjectionViewPageControl.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.broker.NotifyProjectionStopping();
                ShellPage.Current.ProjectionViewPageControl = null;
            });

            this._player.Stop();
            this._player.Source = null;

            Window.Current.Close();
        }

        public async void SwapViews()
        {
            // The view might arrive on the wrong display. The user can
            // easily swap the display on which the view appears
            broker.ProjectionViewPageControl.StartViewInUse();
            await ProjectionManager.SwapDisplaysForViewsAsync(
                ApplicationView.GetForCurrentView().Id,
                broker.MainViewId
            );
            broker.ProjectionViewPageControl.StopViewInUse();
        }
        private void SwapViews_Click(object sender, RoutedEventArgs e)
        {
            SwapViews();
        }

        public async void StopProjecting()
        {
            broker.NotifyProjectionStopping();

            // There may be cases to end the projection from the projected view
            // (e.g. the presentation hosted in that view concludes)
            // broker.ProjectionViewPageControl.StartViewInUse();
            await this._player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                this._player.Stop();
                this._player.Source = null;

                broker.ProjectionViewPageControl.StartViewInUse();

                try
                {
                    await ProjectionManager.StopProjectingAsync(
                        broker.ProjectionViewPageControl.Id,
                        broker.MainViewId
                        );
                }
                catch { }
                Window.Current.Close();

            });

            broker.ProjectionViewPageControl.StopViewInUse();
        }
        private void StopProjecting_Click(object sender, RoutedEventArgs e)
        {
            StopProjecting();
        }
    }
}
