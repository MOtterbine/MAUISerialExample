namespace MAUIAppSerialExample
{
    public partial class App : Application
    {
        public App()
        {
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
}
