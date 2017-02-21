using ListBox.Common;
using ListBox.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class SettingsPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private static bool isTransparentTilePinned = false;
        private static string TransparentTileID = "TransparentAppTile";

        public SettingsPage()
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
            //Скрыть статус бар
            await StatusBar.GetForCurrentView().HideAsync();

            //Акцентный цвет
            this.SelectThemeBox.Items.Add(App.resourceLoader.GetString("Indigo"));
            this.SelectThemeBox.Items.Add(App.resourceLoader.GetString("Light Blue"));
            this.SelectThemeBox.Items.Add(App.resourceLoader.GetString("Teal"));
            this.SelectThemeBox.Items.Add(App.resourceLoader.GetString("Deep Orange"));

            if (this.SelectThemeBox.Items.Contains(App.AppAccentColor))
            {
                this.SelectThemeBox.SelectedItem = App.AppAccentColor;
            }

            //Фон приложения
            this.AppBCBox.Items.Add(App.resourceLoader.GetString("Light"));
            this.AppBCBox.Items.Add(App.resourceLoader.GetString("Dark"));
            if (this.AppBCBox.Items.Contains(App.AppBGColor))
            {
                this.AppBCBox.SelectedItem = App.AppBGColor;
            }

            //Выбор прозрачной плитки
            this.TransparentTileBox.Items.Add(App.resourceLoader.GetString("On"));
            this.TransparentTileBox.Items.Add(App.resourceLoader.GetString("Off"));

            if (SecondaryTile.Exists(TransparentTileID))
            {
                isTransparentTilePinned = true;
                this.TransparentTileBox.SelectedIndex = 0;
            }
            else
            {
                this.TransparentTileBox.SelectedIndex = 1;
            }

            //Выключатель суммарной цены
            this.TotalPriceBox.Items.Add(App.resourceLoader.GetString("On"));
            this.TotalPriceBox.Items.Add(App.resourceLoader.GetString("Off"));

            if (App.isTotalPriceVisible == true)
            {
                this.TotalPriceBox.SelectedIndex = 0;
            }
            else
            {
                this.TotalPriceBox.SelectedIndex = 1;
            }

            //Версия программы
            var Version = Windows.ApplicationModel.Package.Current.Id.Version;
            this.AppVersion.Text = App.resourceLoader.GetString("Version") + " " + Version.Major + "." + Version.Minor + "." + Version.Build + "." + Version.Revision;
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

        public async Task WriteData(string str)
        {
            JsonObject jobject = new JsonObject();
            jobject["Theme"] = JsonValue.CreateStringValue(str);

            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile SampleFile = await localFolder.CreateFileAsync("settings.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(SampleFile, jobject.Stringify());
        }

        private async void SelectThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if (App.AppAccentColor != cb.SelectedItem.ToString())
            {
                App.AppAccentColor = cb.SelectedItem.ToString();

                MessageDialog mg = new MessageDialog(App.resourceLoader.GetString("ApplyThemeMessage"));

                mg.Title = App.resourceLoader.GetString("Attention");

                mg.Commands.Clear();
                mg.Commands.Add(new UICommand(App.resourceLoader.GetString("CloseApp"), new UICommandInvokedHandler(this.CommandInvokedHandler)));
                await mg.ShowAsync();
            }            
        }

        private async void CommandInvokedHandler(IUICommand command)
        {
            await SuspensionManager.SaveAsync();
            App.Current.Exit();
        }

        private bool isPinned(string str)
        {
            if (str == App.resourceLoader.GetString("On")) return true;
            else return false;
        }

        private async void TransparentTileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isTransparentTilePinned != isPinned(this.TransparentTileBox.SelectedItem.ToString()))
            {
                if (isPinned(this.TransparentTileBox.SelectedItem.ToString()) == true)
                {
                    //Закрепить
                    Uri square150x150Logo = new Uri("ms-appx:///Assets/Icon360.png");
                    TileSize newTileDesiredSize = TileSize.Square150x150;
                    SecondaryTile secondaryTile = new SecondaryTile(TransparentTileID, "ListBox", "App", square150x150Logo, newTileDesiredSize);

                    secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Icon170.png");
                    secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                    secondaryTile.VisualElements.ForegroundText = ForegroundText.Dark;

                    await secondaryTile.RequestCreateAsync();

                    isTransparentTilePinned = true;
                }
                else
                {
                    //Открепить
                    SecondaryTile secondaryTile = new SecondaryTile(TransparentTileID);

                    await secondaryTile.RequestDeleteAsync();

                    isTransparentTilePinned = false;
                }
            }
        }

        private void TotalPriceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isPinned(this.TotalPriceBox.SelectedItem.ToString()))
            {
                App.isTotalPriceVisible = true;
            }
            else
            {
                App.isTotalPriceVisible = false;
            }
        }

        private async void AppBCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if (App.AppBGColor != cb.SelectedItem.ToString())
            {
                App.AppBGColor = cb.SelectedItem.ToString();

                MessageDialog mg = new MessageDialog(App.resourceLoader.GetString("ApplyThemeMessage"));

                mg.Title = App.resourceLoader.GetString("Attention");

                mg.Commands.Clear();
                mg.Commands.Add(new UICommand(App.resourceLoader.GetString("CloseApp"), new UICommandInvokedHandler(this.CommandInvokedHandler)));
                await mg.ShowAsync();
            }      
        }

    }
}
