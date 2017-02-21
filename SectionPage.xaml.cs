using ListBox.Common;
using ListBox.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
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
    public sealed partial class SectionPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        //ID для вспомогательной плитки
        public string SecondaryTileID
        {
            get
            {
                return "id" + this.GroupTitle.Text.Replace(" ", "");  
            }
        }

        HubPage rootPage = HubPage.Current;

        public SectionPage()
        {
            this.InitializeComponent();

            ToggleAppBarButton();

            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            if (this.Items.Items.Count == 0)
            {
                this.EmptySectionPageMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.EmptySectionPageMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (App.isTotalPriceVisible == false)
            {
                this.TotalPrice.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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

        //Загрузка элементов страницы
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var group = await SampleDataSource.GetGroupAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Group"] = group;
            ToggleAppBarButton();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            
        }

        //Метод, вызываемый после нажатия на продукт
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                var resourceLoader = ResourceLoader.GetForCurrentView("Resources");
                throw new Exception(resourceLoader.GetString("NavigationFailedExceptionMessage"));
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
            ToggleAppBarButton();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        //Добавление продукта в список
        private async void AddItemTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (this.AddItemTextBox.Text != "")
                {
                    await SampleDataSource.AddItem(this.AddItemTextBox.Text);
                    this.AddItemTextBox.Text = "";
                }
                else
                {
                    await new MessageDialog(App.resourceLoader.GetString("AddEmptyItemError")).ShowAsync();
                }
            }
        }

        //Отмечение продукта купленным
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var CheckBoxItem = sender as CheckBox;

            try
            {
                SampleDataSource.ItemChecked(CheckBoxItem.DataContext.ToString());  
            }
            catch
            {

            }            

            if (SecondaryTile.Exists(SecondaryTileID))
            {
                BadgeUpdaterForSecondaryTile();
            }
        }

        //Отмечение продукта не купленным
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var CheckBoxItem = sender as CheckBox;

            try
            {
                SampleDataSource.ItemUnchecked(CheckBoxItem.DataContext.ToString());
            }
            catch
            {

            }            

            if (SecondaryTile.Exists(SecondaryTileID))
            {
                BadgeUpdaterForSecondaryTile();
            }
        }

        //Метод, вызываемый после изменения количества элементов в списке
        private void Items_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            TotalPrice.Text = SampleDataSource.GroupSummaryPrice();
            
            if (this.Items.Items.Count == 0)
            {
                this.EmptySectionPageMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.EmptySectionPageMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (SecondaryTile.Exists(SecondaryTileID))
            {
                BadgeUpdaterForSecondaryTile();
            }
        }

        //Обновление индикатора событий
        private void BadgeUpdaterForSecondaryTile()
        {
            string badgeXmlString = "<badge value='" + SampleDataSource.GroupUncheckedItemsCount() + "'/>";

            Windows.Data.Xml.Dom.XmlDocument badgeDOM = new Windows.Data.Xml.Dom.XmlDocument();
            badgeDOM.LoadXml(badgeXmlString);
            BadgeNotification badge = new BadgeNotification(badgeDOM);

            BadgeUpdateManager.CreateBadgeUpdaterForSecondaryTile(SecondaryTileID).Update(badge);
        }

        //AddItemTextBox в фокусе
        private void AddItemTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.SectionPageCommandBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.EmptySectionPageMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        //AddItemTextBox не в фокусе
        private void AddItemTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.SectionPageCommandBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (this.Items.Items.Count == 0)
            {
                this.EmptySectionPageMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        //Сортировка продуктов
        private async void SortItems_Click(object sender, RoutedEventArgs e)
        {
            await SampleDataSource.SortItems();
        }

        //Переключатель кнопки "Pin to start"
        private void ToggleAppBarButton()
        {
            if (SecondaryTile.Exists(SecondaryTileID))
            {
                this.PinToStart.Label = App.resourceLoader.GetString("Unpin");
                this.PinToStart.Icon = new SymbolIcon(Symbol.UnPin);
            }
            else
            {
                this.PinToStart.Label = App.resourceLoader.GetString("Pin");
                this.PinToStart.Icon = new SymbolIcon(Symbol.Pin);
            }

            this.PinToStart.UpdateLayout();
        }

        //Закрепить или открепить вспомогательную плитку
        private async void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            if (SecondaryTile.Exists(SecondaryTileID))
            {
                //Открепить
                SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileID);

                await secondaryTile.RequestDeleteAsync();

                ToggleAppBarButton();
            }
            else
            {
                //Закрепить
                Uri square150x150Logo = new Uri("ms-appx:///Assets/Icon360.png");
                string DisplayNameAndArguments = this.GroupTitle.Text;
                TileSize newTileDesiredSize = TileSize.Square150x150;
                SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileID, DisplayNameAndArguments, DisplayNameAndArguments, square150x150Logo, newTileDesiredSize);

                secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Icon170.png");
                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                secondaryTile.VisualElements.ForegroundText = ForegroundText.Dark;
                
                await secondaryTile.RequestCreateAsync();

                BadgeUpdaterForSecondaryTile();

                ToggleAppBarButton();
            }
        }

        private async void RemindMe_Click(object sender, RoutedEventArgs e)
        {
            DateTime myDateTime = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, DateTime.Now.Date.Day, 19, 0, 0);
            var duration = new DateTimeOffset(myDateTime.AddDays(1)) - new DateTimeOffset(myDateTime);
            var appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);
            var daysAppointments = await appointmentStore.FindAppointmentsAsync(myDateTime, duration);
            var daysAppointmentsSubjects = daysAppointments.Select(n => n.Subject);

            string Subject = App.resourceLoader.GetString("BuyProducts") + " \"" + this.GroupTitle.Text + "\"";

            if (!daysAppointmentsSubjects.Contains(Subject))
            {
                Appointment Premiere = new Appointment();
                Premiere.Subject = Subject;
                Premiere.StartTime = myDateTime;
                Premiere.AllDay = false;
                Premiere.Reminder = new TimeSpan();

                await appointmentStore.ShowAddAppointmentAsync(Premiere, new Rect());
            }
            else
            {
                MessageDialog mg = new MessageDialog(App.resourceLoader.GetString("AddedReminder"));
                mg.Title = App.resourceLoader.GetString("Attention");

                await mg.ShowAsync();
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure we have an app bar
            if (BottomAppBar == null) return;

            // Get the button just clicked
            var replyButton = sender as AppBarButton;
            if (replyButton == null) return;

            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["ReplyFlyout"];
            if (replyFlyout == null) return;

            // Close the app bar before opening the flyout
            replyFlyout.Opening += delegate(object o, object o1)
            {
                if (BottomAppBar != null && BottomAppBar.Visibility == Visibility.Visible)
                {
                    BottomAppBar.Visibility = Visibility.Collapsed;
                }
            };

            // Show the app bar after the flyout closes
            replyFlyout.Closed += delegate(object o, object o1)
            {
                if (BottomAppBar != null && BottomAppBar.Visibility == Visibility.Collapsed)
                {
                    BottomAppBar.Visibility = Visibility.Visible;
                }
            };

            var grid = replyFlyout.Content as Grid;
            if (grid == null) return;
            grid.Tapped += delegate(object o, TappedRoutedEventArgs args)
            {
                var transparentGrid = args.OriginalSource as Grid;
                if (transparentGrid != null)
                {
                    replyFlyout.Hide();
                }
            };

            // Use the ShowAt() method on the flyout to specify where exactly the flyout should be located
            replyFlyout.ShowAt(BottomAppBar);
        }

        private void SendOnlyTiles(object sender, TappedRoutedEventArgs e)
        {
            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["ReplyFlyout"];
            if (replyFlyout == null) return;

            replyFlyout.Hide();

            SampleDataSource.ShareText.Share(2);
        }

        private void SendFullList(object sender, TappedRoutedEventArgs e)
        {
            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["ReplyFlyout"];
            if (replyFlyout == null) return;

            replyFlyout.Hide();

            SampleDataSource.ShareText.Share(0);
        }

        private void SendUncheckedItemS(object sender, TappedRoutedEventArgs e)
        {
            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["ReplyFlyout"];
            if (replyFlyout == null) return;

            replyFlyout.Hide();

            SampleDataSource.ShareText.Share(1);
        }

        private void Delete_Items(object sender, RoutedEventArgs e)
        {
            // Ensure we have an app bar
            if (BottomAppBar == null) return;

            // Get the button just clicked
            var replyButton = sender as AppBarButton;
            if (replyButton == null) return;

            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["DeleteFlyout"];
            if (replyFlyout == null) return;

            // Close the app bar before opening the flyout
            replyFlyout.Opening += delegate(object o, object o1)
            {
                if (BottomAppBar != null && BottomAppBar.Visibility == Visibility.Visible)
                {
                    BottomAppBar.Visibility = Visibility.Collapsed;
                }
            };

            // Show the app bar after the flyout closes
            replyFlyout.Closed += delegate(object o, object o1)
            {
                if (BottomAppBar != null && BottomAppBar.Visibility == Visibility.Collapsed)
                {
                    BottomAppBar.Visibility = Visibility.Visible;
                }
            };

            var grid = replyFlyout.Content as Grid;
            if (grid == null) return;
            grid.Tapped += delegate(object o, TappedRoutedEventArgs args)
            {
                var transparentGrid = args.OriginalSource as Grid;
                if (transparentGrid != null)
                {
                    replyFlyout.Hide();
                }
            };

            // Use the ShowAt() method on the flyout to specify where exactly the flyout should be located
            replyFlyout.ShowAt(BottomAppBar);
        }

        private async void DeleteChecked(object sender, RoutedEventArgs e)
        {
            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["DeleteFlyout"];
            if (replyFlyout == null) return;

            replyFlyout.Hide();

            await SampleDataSource.DeleteCheckedItems();
        }

        private async void DeleteAll(object sender, RoutedEventArgs e)
        {
            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["DeleteFlyout"];
            if (replyFlyout == null) return;

            replyFlyout.Hide();

            await SampleDataSource.ClearList();
        }

        private async void DeleteList(object sender, RoutedEventArgs e)
        {
            // Get the attached flyout
            var replyFlyout = (Flyout)Resources["DeleteFlyout"];
            if (replyFlyout == null) return;

            replyFlyout.Hide();

            //Открепить вспомогательную плитку
            if (SecondaryTile.Exists(SecondaryTileID))
            {
                SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileID);

                await secondaryTile.RequestDeleteAsync();

                ToggleAppBarButton();
            }

            //Удалить список
            await SampleDataSource.DeleteGroup();

            //Перейти на предыдущую страницу или закрыть приложение
            if (navigationHelper.CanGoBack())
            {
                navigationHelper.GoBack();
            }
            else
            {
                await SuspensionManager.SaveAsync();
                Application.Current.Exit();
            }
        }
    }
}
