using System;
using System.Diagnostics;
using TwitchClient.Controls;
using TwitchClient.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TwitchClient.Views
{
    public sealed partial class MediaPage : Page
    {
        private ShellPage rootPage;
        private DevicePicker picker;
        ProjectionViewBroker pvb = new ProjectionViewBroker();
        DeviceInformation activeDevice = null;

        int thisViewId;
        private MediaViewModel ViewModel
        {
            get { return ViewModelLocator.Current.MediaViewModel; }
        }

        public MediaPage()
        {
            InitializeComponent();
            DataContext = new MediaViewModel();
            rootPage = ShellPage.Current;
            // Instantiate the Device Picker
            picker = new DevicePicker();

            // Get the device selecter for Miracast devices
            picker.Filter.SupportedDeviceSelectors.Add(ProjectionManager.GetDeviceSelector());

            //Hook up device selected event
            picker.DeviceSelected += Picker_DeviceSelected;

            //Hook up device disconnected event
            picker.DisconnectButtonClicked += Picker_DisconnectButtonClicked;

            //Hook up picker dismissed event
            picker.DevicePickerDismissed += Picker_DevicePickerDismissed;

            // Hook up the events that are received when projection is stoppped
            pvb.ProjectionStopping += Pvb_ProjectionStopping;
        }
       
        private async void Picker_DeviceSelected(DevicePicker sender, DeviceSelectedEventArgs args)
        {
            //Casting must occur from the UI thread.  This dispatches the casting calls to the UI thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    // Set status to Connecting
                    picker.SetDisplayStatus(args.SelectedDevice, "Connecting", DevicePickerDisplayStatusOptions.ShowProgress);

                    // Getting the selected device improves debugging
                    DeviceInformation selectedDevice = args.SelectedDevice;

                    thisViewId = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Id;

                    // If projection is already in progress, then it could be shown on the monitor again
                    // Otherwise, we need to create a new view to show the presentation
                    if (rootPage.ProjectionViewPageControl == null)
                    {
                        // First, create a new, blank view
                        var thisDispatcher = Window.Current.Dispatcher;
                        await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            // ViewLifetimeControl is a wrapper to make sure the view is closed only
                            // when the app is done with it
                            rootPage.ProjectionViewPageControl = ViewLifetimeControl.CreateForCurrentView();

                            // Assemble some data necessary for the new page
                            pvb.MainPageDispatcher = thisDispatcher;
                            pvb.ProjectionViewPageControl = rootPage.ProjectionViewPageControl;
                            pvb.MainViewId = thisViewId;

                            // Display the page in the view. Note that the view will not become visible
                            // until "StartProjectingAsync" is called
                            var rootFrame = new Frame();
                            rootFrame.Navigate(typeof(ProjectionViewPage), pvb);
                            Window.Current.Content = rootFrame;

                            Window.Current.Activate();
                        });
                    }

                    try
                    {
                        // Start/StopViewInUse are used to signal that the app is interacting with the
                        // view, so it shouldn't be closed yet, even if the user loses access to it
                        rootPage.ProjectionViewPageControl.StartViewInUse();

                        try
                        {
                            await ProjectionManager.StartProjectingAsync(rootPage.ProjectionViewPageControl.Id, thisViewId, selectedDevice);

                        }
                        catch (Exception ex)
                        {
                            if (!ProjectionManager.ProjectionDisplayAvailable || pvb.ProjectedPage == null)
                                throw ex;
                        }

                        // ProjectionManager currently can throw an exception even when projection has started.\
                        // Re-throw the exception when projection has not been started after calling StartProjectingAsync 
                        if (ProjectionManager.ProjectionDisplayAvailable && pvb.ProjectedPage != null)
                        {
                            this.player.Pause();
                            await pvb.ProjectedPage.SetMediaSource(this.player.Source, this.player.Position);
                            activeDevice = selectedDevice;
                            // Set status to Connected
                            picker.SetDisplayStatus(args.SelectedDevice, "Connected", DevicePickerDisplayStatusOptions.ShowDisconnectButton);
                            picker.Hide();
                        }
                        else
                        {
                            // Set status to Failed
                            picker.SetDisplayStatus(args.SelectedDevice, "Connection Failed", DevicePickerDisplayStatusOptions.ShowRetryButton);
                        }
                    }
                    catch (Exception)
                    {
                        // Set status to Failed
                        try { picker.SetDisplayStatus(args.SelectedDevice, "Connection Failed", DevicePickerDisplayStatusOptions.ShowRetryButton); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }
        private async void Picker_DevicePickerDismissed(DevicePicker sender, object args)
        {
            //Casting must occur from the UI thread.  This dispatches the casting calls to the UI thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (activeDevice == null)
                {
                    player.Play();
                }
            });
        }
        private async void Picker_DisconnectButtonClicked(DevicePicker sender, DeviceDisconnectButtonClickedEventArgs args)
        {
            //Casting must occur from the UI thread.  This dispatches the casting calls to the UI thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //Update the display status for the selected device.
                sender.SetDisplayStatus(args.Device, "Disconnecting", DevicePickerDisplayStatusOptions.ShowProgress);

                if (this.pvb.ProjectedPage != null)
                    this.pvb.ProjectedPage.StopProjecting();

                //Update the display status for the selected device.
                sender.SetDisplayStatus(args.Device, "Disconnected", DevicePickerDisplayStatusOptions.None);

                // Set the active device variables to null
                activeDevice = null;
            });
        }

        private async void Pvb_ProjectionStopping(object sender, EventArgs e)
        {
            ProjectionViewBroker broker = sender as ProjectionViewBroker;

            TimeSpan position;
            Uri source = null;

            await broker.ProjectedPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                position = broker.ProjectedPage.Player.Position;
                source = broker.ProjectedPage.Player.Source;
            });

            await rootPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.player.Source = source;

                this.player.Position = position;
                this.player.Play();
                rootPage.ProjectionViewPageControl = null;
            });
        }
    }
}
