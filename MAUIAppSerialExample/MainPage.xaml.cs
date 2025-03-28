using System.Linq;
using System.Text;

namespace MAUIAppSerialExample;


public partial class MainPage : ContentPage
{
    int count = 0;
    public MainPage()
    {
        InitializeComponent();

        // Create CODE-BEHIND Bindings
        _ = new Binding("SendData") { Source = this};
        _ = new Binding("RcvData") { Source = this };
        _ = new Binding("CanSend") { Source = this };
        _ = new Binding("DeviceName") { Source = this };

        //Bind the xaml to this class
        BindingContext = this;

        // delegate to device event callback method
        eventsCallback = this.OnDeviceEvent;

        this._CommTimeoutHandler = this.OnCommTimeout;
        _CommTimer = new System.Threading.Timer(_CommTimeoutHandler, null, Timeout.Infinite, Timeout.Infinite);


    }

    ICommunicationDevice serialService = (new MAUI_SerialDevice() as ICommunicationDevice);

    public bool CanSend 
    {
        get => _canSend;
        set
        {
            // update the value and notify the xaml
            _canSend = value;
            OnPropertyChanged("CanSend");
        }
    }
    private bool _canSend = true;
    public string SendData { get; set; } = String.Empty;
    public string DeviceName
    {
        get => deviceName; 
        set
        {
            deviceName = value;
            Preferences.Set(Constants.SETTINGS_DEVICE_NAME, value);
            OnPropertyChanged("DeviceName");
        }
    }
    private string deviceName = Preferences.Default.Get(Constants.SETTINGS_DEVICE_NAME, string.Empty);
    public string RcvData
    { 
        get => _rcvData; 
        set 
        {
            // update the value and notify the xaml
            _rcvData = value;
            OnPropertyChanged("RcvData");
        }
    }
    private string _rcvData = string.Empty;
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
#if WINDOWS
        /*
            Because this is the first, default page to be shown, the controls must be instantiated
            before they can be assigned CursorIcon.Hand behavor - so it's like this...
            (otherwise, the Behavior can normally be set in xaml)
        */
        SendButton.SetCustomCursor(CursorIcon.Hand, SendButton.Handler?.MauiContext);

#endif


    }

    private TimerCallback _CommTimeoutHandler = null;

    private Timer _CommTimer = null;
    protected void OnCommTimeout(object sender)
    {
        this.rawStringData.Clear();

        this.RcvData = Constants.DEVICE_NO_RESPONSE;
        Console.WriteLine($"Timeout - {Constants.DEVICE_NO_RESPONSE}");

        // Close 
        serialService.Close();
        CanSend = true;

    }
    private int _RetryCounter = 0;

    /// <summary>
    /// Starts and enables RX timeout timer
    /// </summary>
    private void ResetCommTimout(bool clearRetries = true)
    {
        // Reset RX timeout timer
        this._CommTimer.Change(Constants.DEFAULT_COMM_NO_RESPONSE_TIMEOUT, Constants.DEFAULT_COMM_NO_RESPONSE_TIMEOUT);

        if (!clearRetries) return;
        this._RetryCounter = 0;
    }
    /// <summary>
    /// Stops RX timeout timer
    /// </summary>
    private void CancelCommTimout()
    {
        // Clear out the data buffer
        // Reset RX timeout timer
        this._CommTimer.Change(Timeout.Infinite, Timeout.Infinite);
        this._RetryCounter = 0;
    }

    protected StringBuilder rawStringData = new StringBuilder();

    DeviceEvent eventsCallback = null;

    private async Task OnDeviceEvent(object sender, DeviceEventArgs e)
    {
        switch (e.Event)
        {
            case CommunicationEvents.ConnectedAsClient:
                Console.WriteLine($"Connected...");
                break;
            case CommunicationEvents.Disconnected:
                // Disconnect Events Callback - we're closed
                serialService.CommunicationEvent -= eventsCallback;
                Console.WriteLine($"Disconnected...");
                break;
            case CommunicationEvents.Receive:
            case CommunicationEvents.ReceiveEnd:

                // collect any incoming data
                this.rawStringData.Append(Encoding.UTF8.GetString(e.data));

                // Test for the end of data character, here it is '>'
                if (e.data[e.data.Length - 1] != '>')
                {
                    // incomplete response, give it more time
                    ResetCommTimout();
                    return;
                }
                // Complete response received, stop the timer
                CancelCommTimout();

                // Transfer the entire stream of data
                this.RcvData = System.Text.RegularExpressions.Regex.Replace(rawStringData.ToString(), @"(^a-zA-Z|\r\r\r|\r\r|\r\n|\n\r|\r|\n|>)", "\r");
                

                Console.WriteLine($"Data Received: {e.data.Length} bytes - {this.RcvData}.");

                // Close 
                serialService.Close();
                CanSend = true;

                break;
        }
    }

    private async Task TestSerial(string deviceName, string data)
    {

#if ANDROID || WINDOWS
        if (serialService == null) throw new NullReferenceException("Failed to instantiate a valid ICommunicationDevice");

        // Attach Events Callback before opening
        serialService.CommunicationEvent += eventsCallback;

        // Get a list of paired devices to validate the device name 
        IList<string> deviceList = null;
        if (serialService is IDevicesService == null)
        {
            deviceList = new List<string>(); // empty, but not null
        }
        else
        {
            deviceList = (serialService as IDevicesService).GetDeviceList();
        }

        if (deviceList == null || deviceList.Count < 1)
        {
            // throw an exception, put up a message...
            Console.WriteLine("No devices found");
            return;
        }
        // Ensure the name to be used is in the list of available devices
        var validDeviceName = deviceList.Where(i => i.CompareTo(deviceName) == 0).FirstOrDefault();
        if (string.IsNullOrEmpty(validDeviceName))
        {
            Console.WriteLine($"Device {deviceName} not found in list.");

            return;
        }
        if (serialService.Open(validDeviceName))
        {
           

            // Clear out the data buffer
            this.rawStringData.Clear();

            // SEND THE DATA
            await serialService.Send(data);
        }
        else
        {
            Console.WriteLine($"Unable to open device: {deviceName}.");
        }
#endif

    }
    /// <summary>
    ///  Begin the asychronous send process
    /// </summary>
    private void StartSendData()
    {
        Dispatcher.Dispatch(() => { CanSend = false; });
        // Task.Factory.StartNew(() => TestSerial(<DEVICE NAME>, <STRING DATA>));
        //                            or
        // Task.Factory.StartNew(() => TestSerial(<DEVICE NAME>, <BYTE [] DATA>));

        // specific use case - notice the data sent (SendData) is bound to the entry
        ResetCommTimout();
        Task.Factory.StartNew(() => TestSerial(DeviceName, $"{SendData}\r"));
    }
    private void OnSendButtonClicked(object sender, EventArgs e)
    {
        StartSendData();
    }

    private void SendDataEntry_Completed(object sender, EventArgs e)
    {
        StartSendData();
    }


}
