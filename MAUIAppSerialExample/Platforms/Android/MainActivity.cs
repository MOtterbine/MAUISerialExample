using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace MAUIAppSerialExample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    App pApp = ((App)App.Current);
    public MainActivity()
    {
        // helps with text keyboard - screen becomes scroleable when keyboard is open
        pApp.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);

    }

    protected override void OnCreate(Bundle savedInstanceState)
    {

        if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
        {
            Window.SetNavigationBarColor(Android.Graphics.Color.Black);
            Window.SetStatusBarColor(Android.Graphics.Color.Black);


            Window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
            Window.SetFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds, Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);

            Window.SetNavigationBarColor(Android.Graphics.Color.Argb(0xFF, 0x00, 0x00, 0x00));
            Window.SetStatusBarColor(Android.Graphics.Color.Argb(0xFF, 0x00, 0x00, 0x00));


            //DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;
        }
        base.OnCreate(savedInstanceState);



        if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
        {

            // Start permissions check...
            RequestPermissions(new string[] { "android.permission.BLUETOOTH_CONNECT" }, 15001); // the value 15001 is arbitrary and random

        }
        else
        {
            pApp.HasPermissions = true;
            pApp.FirePermissionsReadyEvent();
        }

    }
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {

        //  Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        switch (requestCode)
        {
            case 15001:
                // Check for Android sdk 31, or higher bluetooth permissions
                if (grantResults.Length > 0)
                {
                    if (grantResults[0] == 0) // good permission - this is a sdk 31, or higher, device
                    {
                        pApp.HasPermissions = true;
                        pApp.FirePermissionsReadyEvent();
                        return;
                    }
                }
                pApp.HasPermissions = false; // No device permissions at all...
                pApp.FirePermissionsReadyEvent();
                break;
        }

    }

}
