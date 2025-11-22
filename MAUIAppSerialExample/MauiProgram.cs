using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;


#if WINDOWS
using Microsoft.UI;
    using Microsoft.UI.Windowing;
    using Windows.Graphics;
#endif


namespace MAUIAppSerialExample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{

        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).ConfigureLifecycleEvents(events =>
                     {

#if WINDOWS


                events.AddWindows(lifeCycleBuilder =>
                {

                    lifeCycleBuilder
                    .OnAppInstanceActivated((sender, e) =>{})
                    .OnWindowCreated(w =>
                    {
                        
                        //w.ExtendsContentIntoTitleBar = false;
                        IntPtr wHandle = WinRT.Interop.WindowNative.GetWindowHandle(w);
                        WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(wHandle);
                        Microsoft.UI.Windowing.AppWindow mauiWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                        //When user execute the closing method, we can push a display alert. If user click Yes, close this application, if click the cancel, display alert will dismiss.
                        mauiWindow.Closing += async (s, e) =>
                        {
                            var _app = Application.Current as App;
                            if (_app == null) return;
                            
                            //_app.NotifyAppClosing();

                            //e.Cancel = true;
                            //bool result = await App.Current.MainPage.DisplayAlert(
                            //    "Alert title",
                            //    "You sure want to close app?",
                            //    "Yes",
                            //    "Cancel");

                           // if (result)
                            //{
                                App.Current.Quit();
                            //}
                        };

                       

                        // Fixed window size and no minimize or maximize
                        //mauiWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay); 

                        // Resizable window with minimize/maximize
                        mauiWindow.SetPresenter(AppWindowPresenterKind.Overlapped);  

                        mauiWindow.Title = String.Empty;

                        var s = mauiWindow.Presenter as OverlappedPresenter;
                        s?.SetBorderAndTitleBar(true, true);

                        var titleBar = mauiWindow.TitleBar;


                        titleBar.ExtendsContentIntoTitleBar = true;

                        //titleBar.ButtonBackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent.ToWindowsColor(); // Makes the title bar background transparent
                        //titleBar.ButtonForegroundColor = Microsoft.Maui.Graphics.Colors.Transparent.ToWindowsColor(); // Hides the title text



                        //  titleBar.ForegroundColor = Windows.UI.Color.FromArgb(0, 255, 0, 255);
                        titleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;

                        titleBar.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("FF505050").ToWindowsColor();
                        titleBar.ForegroundColor = Microsoft.Maui.Graphics.Color.FromArgb("FFd6d6d6").ToWindowsColor();

                        titleBar.ButtonBackgroundColor = Color.FromArgb("FF505050").ToWindowsColor();
                        titleBar.ButtonForegroundColor = Color.FromArgb("FFd6d6d6").ToWindowsColor();

                        titleBar.InactiveBackgroundColor = Color.FromArgb("FF505050").ToWindowsColor();
                        titleBar.InactiveForegroundColor  = Color.FromArgb("FFd6d6d6").ToWindowsColor();

                        titleBar.ButtonInactiveBackgroundColor = Color.FromArgb("FF505050").ToWindowsColor();
                        titleBar.ButtonInactiveForegroundColor = Color.FromArgb("FFd6d6d6").ToWindowsColor();

                        titleBar.ButtonHoverBackgroundColor = Color.FromArgb("FF505050").ToWindowsColor();
                        titleBar.ButtonHoverForegroundColor = Color.FromArgb("FFffffff").ToWindowsColor();

                        titleBar.ButtonPressedBackgroundColor = Color.FromArgb("FF505050").ToWindowsColor();
                        titleBar.ButtonPressedForegroundColor = Color.FromArgb("FFd6d6d6").ToWindowsColor();



                        var dispH = DeviceDisplay.Current.MainDisplayInfo.Height;
                        var dispW = DeviceDisplay.Current.MainDisplayInfo.Width;
                        var dispD = DeviceDisplay.Current.MainDisplayInfo.Density;
                        var p = new PointInt32(Convert.ToInt32((dispW / dispD - Constants.MIN_WINDOW_WIDTH_WINDOWS) / 2), Convert.ToInt32((dispH / dispD - Constants.MIN_WINDOW_HEIGHT_WINDOWS) / 2));


                        var wndRect = new RectInt32(p.X, p.Y, Constants.MIN_WINDOW_WIDTH_WINDOWS, Constants.MIN_WINDOW_HEIGHT_WINDOWS);
                        //  titleBar.SetDragRectangles([new RectInt32(0, 0, WindowWidth, WindowHeight)]);


                        // CENTER AND RESIZE THE APP
                        mauiWindow.MoveAndResize(wndRect);
                    });
                });
#endif


                         Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("PickerHandlerCustomization", (handler, view) =>
                         {

#if ANDROID
            //    handler.PlatformView.Focusable = false;
                //handler.PlatformView.ShowSoftInputOnFocus = false;
                handler.PlatformView.SetPadding(15,15,15,15);  
#elif IOS || MACCATALYST

#elif WINDOWS
                             handler.PlatformView.FontWeight = new Windows.UI.Text.FontWeight(700);  
#endif

                         });


                     });

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("EntryCustomization", (handler, view) =>
        {
            if (view is Entry)
            {
#if ANDROID
            //    handler.PlatformView.Focusable = false;
                //handler.PlatformView.ShowSoftInputOnFocus = false;
               // handler.PlatformView.SetHeight(40);
                //handler.PlatformView.SetPaddingRelative(0,0,0,0);
               // handler.PlatformView.SetPadding(0, 0, 0, 0);
#elif IOS || MACCATALYST

#elif WINDOWS
             //   handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
#endif
            }
        });


#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
