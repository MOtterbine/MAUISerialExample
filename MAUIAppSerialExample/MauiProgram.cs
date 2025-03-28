using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

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
                        AppWindow mauiWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                        mauiWindow.SetPresenter(AppWindowPresenterKind.Overlapped);  // TO SET THE APP INTO FULL SCREEN

                        var s = mauiWindow.Presenter as OverlappedPresenter;
                        s?.SetBorderAndTitleBar(true, true);

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
          //  handler.PlatformView.SetPadding(10,10,10,10);  
#elif WINDOWS
                             handler.PlatformView.FontWeight = new Windows.UI.Text.FontWeight(1000);  
#endif

                         });


                     });





#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
