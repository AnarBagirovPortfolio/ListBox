using ListBox.Common;
using ListBox.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace ListBox
{
    public sealed partial class App : Application
    {
        public static int GroupIndex = 0;
        public static int ItemIndex = 0;

        private TransitionCollection transitions;

        public static Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader();

        public static string AppAccentColor;
        public static string AppBGColor;
        public static bool isTotalPriceVisible;

        private async Task GetSettings()
        {
            try
            {
                StorageFile SettingsFile = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("settings.txt");

                string JsonSettingsString = await FileIO.ReadTextAsync(SettingsFile);

                JsonObject JsonSettings = JsonObject.Parse(JsonSettingsString);

                AppAccentColor = JsonSettings.GetNamedString("Theme", "");
                isTotalPriceVisible = JsonSettings.GetNamedBoolean("TotalPrice", true);
                AppBGColor = JsonSettings.GetNamedString("Background", "");
            }
            catch
            {
                AppAccentColor = resourceLoader.GetString("Indigo");
                AppBGColor = resourceLoader.GetString("Light");
                isTotalPriceVisible = true;
            }
        }

        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            var tileId = e.TileId.ToString();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page.
                rootFrame = new Frame();

                // Associate the frame with a SuspensionManager key.
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                await GetSettings();

                await StatusBar.GetForCurrentView().HideAsync();

                await SampleDataSource.GetGroupsAsync();

                if (AppAccentColor != resourceLoader.GetString("Indigo"))
                {
                    if (AppAccentColor == resourceLoader.GetString("Light Blue"))
                    {
                        Resources["AppMainColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 2, 136, 209));
                        Resources["CheckBoxBGColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 2, 150, 229));
                    }

                    else if (AppAccentColor == resourceLoader.GetString("Teal"))
                    {
                        Resources["AppMainColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 121, 107));
                        Resources["CheckBoxBGColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 140, 124));
                    }
                    else
                    {
                        Resources["AppMainColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 198, 40, 40));
                        Resources["CheckBoxBGColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 224, 45, 45));
                    }
                }

                if (AppBGColor == resourceLoader.GetString("Light"))
                {
                    rootFrame.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 245, 245));
                }
                else
                {
                    rootFrame.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 20, 20, 20));

                    Resources["AppBackgroundColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 20, 20, 20));

                    Resources["CommandBarBackgroundColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 25, 25, 25));

                    Resources["ItemForegroundColor"] = new SolidColorBrush(Colors.White);

                    Resources["TextBlockColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30));
                    Resources["TextBoxBorderBrush"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 10, 10));
                    Resources["TextBlockForegroundColor"] = new SolidColorBrush(Colors.White);

                    Resources["ItemPageTextBoxColor"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30));
                }            

                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
                }

                // Place the frame in the current Window.
                Window.Current.Content = rootFrame;
            }

            else
            {
                if (tileId.Contains("id"))
                {
                    if (!rootFrame.Navigate(typeof(SectionPage), e.Arguments))
                    {
                        throw new Exception("Failed to create initial page");
                    }

                    rootFrame.BackStack.Clear();
                }
                else if (tileId.Contains("App"))
                {
                    if (!rootFrame.Navigate(typeof(HubPage), e.Arguments))
                    {
                        throw new Exception("Failed to create initial page");
                    }

                    rootFrame.BackStack.Clear();
                }
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter.
                if (tileId.Contains("id"))
                {
                    if (!rootFrame.Navigate(typeof(SectionPage), e.Arguments))
                    {
                        throw new Exception("Failed to create initial page");
                    }
                }
                else
                {
                    if (!rootFrame.Navigate(typeof(HubPage), e.Arguments))
                    {
                        throw new Exception("Failed to create initial page");
                    }

                }
                
            }

            // Ensure the current window is active.
            Window.Current.Activate();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
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
    }
}
