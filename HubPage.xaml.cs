using ListBox.Common;
using ListBox.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ListBox
{
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public static HubPage Current;

        public HubPage()
        {
            this.InitializeComponent();

            Current = this;

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            if (this.Lists.Items.Count == 0)
            {
                this.EmptyHubPageMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.EmptyHubPageMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            this.DefaultViewModel["Groups"] = sampleDataGroups;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            
        }

        private void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(SectionPage), groupId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void AddList_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void AddListTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (this.AddListTextBox.Text != "")
                {
                    string ListTile = this.AddListTextBox.Text;
                    ListTile = Regex.Replace(ListTile, @"\s+", " ");
                    await SampleDataSource.AddGroup(ListTile);
                    this.AddListTextBox.Text = "";
                    this.AddListTextBox.IsEnabled = false;
                    this.AddListTextBox.IsEnabled = true;
                }
                else
                {
                    await new MessageDialog(App.resourceLoader.GetString("AddEmptyGroupError")).ShowAsync();
                }
            }
        }

        private void CommandBar_Closed(object sender, object e)
        {
            this.HubPageCommandBar.Opacity = 1;
        }

        private void CommandBar_Opened(object sender, object e)
        {
            this.HubPageCommandBar.Opacity = 1;
        }

        private void AddListTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.HubPageCommandBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.EmptyHubPageMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void AddListTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.HubPageCommandBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (this.Lists.Items.Count == 0)
            {
                this.EmptyHubPageMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (this.Lists.Items.Count == 0)
            {
                this.EmptyHubPageMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.EmptyHubPageMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void SettingsInMainPage_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SettingsPage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }
    }
}
