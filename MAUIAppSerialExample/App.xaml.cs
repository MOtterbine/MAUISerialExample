namespace MAUIAppSerialExample;

public delegate void PermissionsResultReady(object sender, EventArgs e);

public partial class App : Application
{
    public bool HasPermissions { get; set; } = false;
    public event PermissionsResultReady PermissionsReadyEvent;
    public void FirePermissionsReadyEvent()
    {

        if (this.PermissionsReadyEvent != null)
        {
            this.PermissionsReadyEvent(null, EventArgs.Empty);
        }
    }

    public App()
    {

#if WINDOWS
        this.HasPermissions = true;
#endif

        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {

        var window = base.CreateWindow(activationState);

#if WINDOWS

        window.MinimumWidth = Constants.MIN_WINDOW_WIDTH_WINDOWS;
        window.MinimumHeight = Constants.MIN_WINDOW_HEIGHT_WINDOWS;

#endif

        return window;

    }


}
