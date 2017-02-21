using ListBox.Common;
using ListBox.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ListBox
{
    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ItemPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
            var item = await SampleDataSource.GetItemAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            
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
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            bool ChangesApplied = false;

            try
            {
                double Count = double.Parse(ChangeCount.Text.Replace(',','.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                double Price = double.Parse(ChangePrice.Text.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                string Description = ChangeDesc.Text.ToString();
                SampleDataSource.ChangeCountAndPrice(Count, Price, Description);
                ChangesApplied = true;
            }
            catch
            {

            }

            if (ChangesApplied)
            {
                navigationHelper.GoBack();
            }
            else
            {
                MessageDialog mDialog = new MessageDialog(App.resourceLoader.GetString("InputDataError"));
                mDialog.Title = App.resourceLoader.GetString("Error");
                await mDialog.ShowAsync();
            }
        }

        private void CancelChanges_Click(object sender, RoutedEventArgs e)
        {
            navigationHelper.GoBack();
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            await SampleDataSource.DeleteItem();

            navigationHelper.GoBack();
        }
    }
}
