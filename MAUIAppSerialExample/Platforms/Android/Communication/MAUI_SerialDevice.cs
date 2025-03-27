namespace MAUIAppSerialExample;

public partial class MAUI_SerialDevice : ICommunicationDevice, IDevicesService
{
    private CancellationTokenSource tokenSource = null;


    public MAUI_SerialDevice()
    {

    }

    private AndroidBluetoothDevice _currentDevice = new AndroidBluetoothDevice();
    public IList<string> GetDeviceList() => _currentDevice.GetDeviceList();

    public string DeviceName 
    { 

        get => this._currentDevice==null?null:this._currentDevice.DeviceName;
        set
        {
            this.SetDevice(value);
        }
    }

    private void SetDevice(string channelName)
    {

        if (string.IsNullOrEmpty(channelName))
        {
            return;
        }
        if (_currentDevice.GetDeviceList().Contains(channelName))
        {
            this._currentDevice.DeviceName = channelName;
            return;
        }
        throw new ArgumentException($"Device Name Invalid: {channelName}");

    }

    public bool IsEnabled => this._currentDevice != null;

    public bool IsConnected => this._currentDevice == null ? false : this._currentDevice.IsConnected;

    public string Description => this._currentDevice == null ? "n/a" : this._currentDevice.DeviceName;

    public bool Open(string commChannel)
    {
        if (this._currentDevice == null)
        {
            if (String.IsNullOrEmpty(this.DeviceName)) FireErrorEvent(Constants.DEVICE_NOT_SETUP);
            else FireErrorEvent(Constants.DEVICE_NOT_CONNECTED);
            return false;
        }
        return this._currentDevice.Open(commChannel);
    }

    public bool Close()
    {
        return this._currentDevice == null ? false : this._currentDevice.Close();
    }

    private object eventLock = new object();
    private event DeviceEvent _communicationEvent;
    public event DeviceEvent CommunicationEvent
    {
        add
        {
            lock (eventLock)
            {
                this._communicationEvent += value;
                if (this._currentDevice == null) return;
                this._currentDevice.CommunicationEvent += value;
            }
        }
        remove
        {
            lock (eventLock)
            {
                this._communicationEvent -= value;
                if (this._currentDevice == null) return;
                this._currentDevice.CommunicationEvent -= value;
            }
        }
    }

    private void FireErrorEvent(string message)
    {
        using (DeviceEventArgs evt = new DeviceEventArgs())
        {
            evt.Event = CommunicationEvents.Error;
            evt.Description = message;
            if(this._communicationEvent != null) this._communicationEvent(this, evt);
        }
    }

    public override string ToString() => this._currentDevice.DeviceName;

    public async Task<bool> Send(string text) => await this._currentDevice?.Send(text);

    public async Task<bool> Send(byte[] buffer, int offset, int count) => await this._currentDevice?.Send(buffer, offset, count);
    
    
}
