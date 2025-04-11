using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MAUIAppSerialExample;


public partial class MainPage : ContentPage
{
    public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    int count = 0;
    App pApp = ((App)App.Current);

    public List<UInt32> BaudRates => MAUIAppSerialExample.BaudRates.Items;

    private UInt32 serialBaudRate = Convert.ToUInt32(Preferences.Get(Constants.PREFS_SERIAL_BAUD_RATE, (uint)9600));
    public UInt32 SerialBaudRate
    {
        get { return serialBaudRate; }
        set
        {
            this.serialBaudRate = value;
            OnPropertyChanged("SerialBaudRate");
            Preferences.Set(Constants.PREFS_SERIAL_BAUD_RATE, value);

        }
    }


    public MainPage()
    {

       // Preferences.Default.Clear();

        InitializeComponent();

        // Create CODE-BEHIND Bindings
        _ = new Binding("SendData") { Source = this};
        _ = new Binding("RcvData") { Source = this };
        _ = new Binding("CanSend") { Source = this };
        _ = new Binding("SelectedDevice") { Source = this };
        _ = new Binding("DeviceList") { Source = this };
        _ = new Binding("SendCR") { Source = this };
        _ = new Binding("Version") { Source = this };
        _ = new Binding("EOTCharacterString") { Source = this };
        _ = new Binding("SerialBaudRate") { Source = this };
        _ = new Binding("CheckForEOT") { Source = this };
        _ = new Binding("ExpectResponse") { Source = this };

           EOTCharacterString = Preferences.Default.Get(Constants.SETTINGS_EOT_CHARACTER, ">");;



        // Assign a delegate for the device's event callback method
        eventsCallback = this.OnDeviceEvent;

        // Create a Communication Timeout (so we don't get stuck)
        this._CommTimeoutHandler = this.OnCommTimeout;
        _CommTimer = new System.Threading.Timer(_CommTimeoutHandler, null, Timeout.Infinite, Timeout.Infinite);


        //Bind the xaml to this class
        BindingContext = this;

        pApp.PermissionsReadyEvent += PApp_PermissionsReadyEvent;


        EOTCharacterString = Preferences.Default.Get(Constants.SETTINGS_EOT_CHARACTER, ">"); ;
    }



    
    private void PApp_PermissionsReadyEvent(object sender, EventArgs e)
    {
        if (!pApp.HasPermissions) return;
        LoadDevices();
        OnPropertyChanged("CanSend");
    }

    private void LoadDevices()
    {

        if (!pApp.HasPermissions) throw new PermissionException("LoadDevices() - User is not permitted to access device");
        var j = serialService as IDevicesService;
        if (j == null) return;
        this.DeviceList = j.GetDeviceList();
        OnPropertyChanged("DeviceList");
        // Get the last-set device values - if any
        var storedDeviceName = Preferences.Default.Get(Constants.SETTINGS_DEVICE_NAME, string.Empty);
        // Search the list of available devices for the name
        this.SelectedDevice = this.DeviceList.Where(d => string.Compare(d, storedDeviceName) == 0).FirstOrDefault();
    }

    ICommunicationDevice serialService = (new MAUI_SerialDevice() as ICommunicationDevice);


    public string EOTCharacterString
    {
        get
        {

           // if (char.IsWhiteSpace(this._EOTCharacter)) return $"{((byte)this._EOTCharacter):X02}";
            return $"{this._EOTCharacter}";
        }
        set
        {
            if(!char.TryParse(value, out this._EOTCharacter))
            {
                return;
            }
            Preferences.Default.Set(Constants.SETTINGS_EOT_CHARACTER, value);
            OnPropertyChanged("EOTCharacterString");
        }
    }
    private string _EOTCharacterString = Preferences.Default.Get(Constants.SETTINGS_EOT_CHARACTER, ">");
    private char _EOTCharacter = '>';

    public bool CanSend 
    {
        get => _canSend && pApp.HasPermissions;
        set
        {
            // update the value and notify the xaml
            _canSend = value;
            OnPropertyChanged("CanSend");
        }
    }
    private bool _canSend = true;
    public string SendData { get; set; } = String.Empty;

    public bool SendCR
    {
        get => sendCR;
        set
        {
            // update the value and notify the xaml
            if (sendCR == value) return;
            sendCR = value;
            Preferences.Default.Set(Constants.SETTINGS_SEND_CR, value);
            OnPropertyChanged("SendCR");
        }
    }
    private bool sendCR = Preferences.Default.Get(Constants.SETTINGS_SEND_CR, false);
    public bool ExpectResponse
    {
        get => expectResponse;
        set
        {
            // update the value and notify the xaml
            if (expectResponse == value) return;
            expectResponse = value;
            Preferences.Default.Set(Constants.PREFS_TEST_FOR_RESPONSE, value);
            OnPropertyChanged("ExpectResponse");
        }
    }
    private bool expectResponse = Preferences.Default.Get(Constants.PREFS_TEST_FOR_RESPONSE, false);

    public bool ExpectEOT
    {
        get => expectEOT;
        set
        {
            // update the value and notify the xaml
            if (expectEOT == value) return;
            expectEOT = value;
            Preferences.Default.Set(Constants.PREFS_TEST_FOR_EOT, value);
            OnPropertyChanged("ExpectResponse");
        }
    }
    private bool expectEOT = Preferences.Default.Get(Constants.PREFS_TEST_FOR_EOT, false);

    public IList<String> DeviceList 
    {
        get; 
        private set; 
    }

    public string SelectedDevice
    {
        get => selectedDeviceName; 
        set
        {
            // Only set if value has changed
            if (string.Compare(selectedDeviceName, value) == 0 || string.IsNullOrEmpty(value)) return;
            selectedDeviceName = value;
            Preferences.Default.Set(Constants.SETTINGS_DEVICE_NAME, value);
            OnPropertyChanged("SelectedDevice");
        }
    }
    private string selectedDeviceName = string.Empty;
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

        OnPropertyChanged("EOTCharacterString");
    }

    private TimerCallback _CommTimeoutHandler = null;

    private Timer _CommTimer = null;
    protected void OnCommTimeout(object sender)
    {
        CancelCommTimout();
        this.rawStringData.Clear();

        this.RcvData = Constants.DEVICE_NO_RESPONSE;
        Debug.WriteLine($"Timeout - {Constants.DEVICE_NO_RESPONSE}");

        // Close - controls changed via the device event callback
        serialService.Close();
        CanSend = true;
#if WINDOWS
        // put the user right back on the data to send control - Great in Windows, but keyboard pops up on Android when focus goes to Entry
        Dispatcher.Dispatch(() => SendDataEntry.Focus());
#endif

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
            case CommunicationEvents.Error:
                CancelCommTimout();
                RcvData = e.Description;
                Debug.WriteLine($"Error {e.Description}");
                serialService.Close();
                CanSend = true;
                break;
            case CommunicationEvents.ConnectedAsClient:
                Debug.WriteLine($"Connected...");
                break;
            case CommunicationEvents.Disconnected:
                // Disconnect Events Callback - we're closed
                serialService.CommunicationEvent -= eventsCallback;
                Debug.WriteLine($"Disconnected...");
                this.CanSend = true;
                break;
            case CommunicationEvents.Receive:
            case CommunicationEvents.ReceiveEnd:

                // collect any incoming data
                this.rawStringData.Append(Encoding.UTF8.GetString(e.data));

                // Test for the end of data character, here it is '>'
                if (ExpectEOT && ExpectResponse)
                {
                    if (e.data[e.data.Length - 1] != _EOTCharacter)
                    {
                        // incomplete response, give it more time
                        ResetCommTimout();
                        return;
                    }
                }
                // Complete response received, stop the timer
                CancelCommTimout();

                // show binary of the byte specified
                if (e.data != null)
                {
                    // NO EOT
                    if (!ExpectEOT)
                    {
                        int i = 0;
                        if (e.data.Length > 0)
                        {
                            // byte to binary (i.e. 01101100)
                            // Set initial 
                            this.RcvData = $"byte {i}: {Convert.ToString(e.data[i], 2).PadLeft(8, '0')}{Environment.NewLine}";
                        }
                        i++;
                        for (; i < e.data.Length; i++) // max 4 bytes
                        {
                            if (e.data[i] != null)
                            {
                                // byte to binary (i.e. 01101100)
                                // Append...
                                this.RcvData += $"byte {i}: {Convert.ToString(e.data[i], 2).PadLeft(8, '0')}{Environment.NewLine}";
                            }
                        }
                    }
                    else
                    {
                        // WITH EOT
                        // Transfer the entire stream of data, with '\r' trimmed from the edges
                        this.RcvData = System.Text.RegularExpressions.Regex.Replace(rawStringData.ToString(), @$"(^a-zA-Z|\r\r\r|\r\r|\r\n|\n\r|\r|\n|{_EOTCharacter})", $"{Environment.NewLine}").Trim(Environment.NewLine.ToArray()[0]);
                    }

                  //  Debug.WriteLine($"Data Received: {e.data.Length} bytes - {this.RcvData}.");
                }

                CloseCommChannel(); // callback should set   CanSend = true

                if (!string.IsNullOrEmpty(SendData))
                {
                    SendDataEntry.SelectionLength = SendData.Length-1;
                }
                break;
        }
    }

    private async Task TestSerial(string deviceName, string data)
    {

#if ANDROID || WINDOWS
        if (serialService == null) throw new NullReferenceException("Failed to instantiate a valid ICommunicationDevice");

        // Attach Events Callback before opening
        serialService.CommunicationEvent += eventsCallback;

        if (this.DeviceList == null || this.DeviceList.Count < 1)
        {
            Debug.WriteLine("Error: No Devices Found");
            return;
        }

        //// Get a list of paired devices to validate the device name 
        //IList<string> deviceList = null;
        //if (serialService is IDevicesService == null)
        //{
        //    deviceList = new List<string>(); // empty, but not null
        //}
        //else
        //{
        //    deviceList = (serialService as IDevicesService).GetDeviceList();
        //}

        //if (deviceList == null || deviceList.Count < 1)
        //{
        //    // throw an exception, put up a message - Do something!
        //    Debug.WriteLine("No devices found");
        //    return;
        //}
        // Ensure the name to be used is in the list of available devices
        var validDeviceName = DeviceList.Where(i => i.CompareTo(deviceName) == 0).FirstOrDefault();
        if (string.IsNullOrEmpty(validDeviceName))
        {
            Debug.WriteLine($"Device {deviceName} not found in list.");
            return;
        }
        if (serialService.Open(validDeviceName))
        {
           
            // Clear out the data buffer
            this.rawStringData.Clear();
            if (ExpectResponse)
            {
                // Reset the timer and send without waiting...
                ResetCommTimout();
            }
            // SEND THE DATA
            await serialService.Send(data);
            // Don't close if a response is expected. timeout will close if doesn't come
            if (ExpectResponse)
            {

                return;
            }

            this.CloseCommChannel();
#if WINDOWS
            // put the user right back on the data to send control - Great in Windows, but keyboard pops up on Android when focus goes to Entry
            Dispatcher.Dispatch(()=>SendDataEntry.Focus());
#endif

        }
        else
        {
            Debug.WriteLine($"Unable to open device: {deviceName}.");
        }
#endif

    }

    private void CloseCommChannel()
    {
#if WINDOWS
        // put the user right back on the data to send control - Great in Windows, but keyboard pops up on Android when focus goes to Entry
        Dispatcher.Dispatch(()=>SendDataEntry.Focus());
#endif

        if (serialService == null) return;
        this.serialService.Close();
        this.CanSend = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!pApp.HasPermissions) return;
        LoadDevices();

        if (pApp.HasPermissions) LoadDevices();
        //OnPropertyChanged("DeviceList");
    }
    /// <summary>
    ///  Begin the asychronous send process
    /// </summary>
    private void StartSendData()
    {
        if(String.IsNullOrEmpty(SelectedDevice))
        {
            RcvData = "No Device Selected";
            return;
        }

        // Disables controls and updates UI-thread about it
        Dispatcher.Dispatch(() => { 
            CanSend = false;
            RcvData = string.Empty;
        });
        Task.Factory.StartNew(()=> TestSerial(SelectedDevice, $"{SendData}{(SendCR ? "\r\n" : string.Empty)}"));

        /* TWO WAYS (method overloads) TO SEND DATA
         Task.Factory.StartNew(() => TestSerial(<DEVICE NAME>, <STRING DATA>));
                                    or
         Task.Factory.StartNew(() => TestSerial(<DEVICE NAME>, <BYTE [] DATA>));
        */
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
