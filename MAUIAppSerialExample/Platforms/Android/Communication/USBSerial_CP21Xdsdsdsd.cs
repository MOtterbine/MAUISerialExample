using Android.Hardware.Usb;
using Android.OS;
using Java.Lang;
using Java.Nio;
using Java.Util;
using MAUIAppSerialExample;
using String = System.String;
using Exception = System.Exception;

namespace MAUIAppSerialExample.Platforms.Android.Communication;

public class USBSerial_CP21X : AndroidUSB_Base
{

        private static int USB_WRITE_TIMEOUT_MILLIS = 5000;




    /*
     * Configuration Request Types
     */





    private static readonly UsbAddressing REQTYPE_HOST_TO_DEVICE = (UsbAddressing)0x41;
private static readonly UsbAddressing REQTYPE_DEVICE_TO_HOST = (UsbAddressing)0xc1;

private int _portIndex = 0;

/*
 * Configuration Request Codes
 */
private static int SILABSER_IFC_ENABLE_REQUEST_CODE = 0x00;
private static int SILABSER_SET_LINE_CTL_REQUEST_CODE = 0x03;
private static int SILABSER_SET_BREAK_REQUEST_CODE = 0x05;
private static int SILABSER_SET_MHS_REQUEST_CODE = 0x07;
private static int SILABSER_GET_MDMSTS_REQUEST_CODE = 0x08;
private static int SILABSER_SET_XON_REQUEST_CODE = 0x09;
private static int SILABSER_SET_XOFF_REQUEST_CODE = 0x0A;
private static int SILABSER_GET_COMM_STATUS_REQUEST_CODE = 0x10;
private static int SILABSER_FLUSH_REQUEST_CODE = 0x12;
private static int SILABSER_SET_FLOW_REQUEST_CODE = 0x13;
private static int SILABSER_SET_CHARS_REQUEST_CODE = 0x19;
private static int SILABSER_SET_BAUDRATE_REQUEST_CODE = 0x1E;

private static int FLUSH_READ_CODE = 0x0a;
private static int FLUSH_WRITE_CODE = 0x05;

/*
 * SILABSER_IFC_ENABLE_REQUEST_CODE
 */
private static int UART_ENABLE = 0x0001;
private static int UART_DISABLE = 0x0000;

/*
 * SILABSER_SET_MHS_REQUEST_CODE
 */
private static int DTR_ENABLE = 0x101;
private static int DTR_DISABLE = 0x100;
private static int RTS_ENABLE = 0x202;
private static int RTS_DISABLE = 0x200;

/*
* SILABSER_GET_MDMSTS_REQUEST_CODE
 */
private static int STATUS_DTR = 0x01;
private static int STATUS_RTS = 0x02;
private static int STATUS_CTS = 0x10;
private static int STATUS_DSR = 0x20;
private static int STATUS_RI = 0x40;
private static int STATUS_CD = 0x80;


private bool dtr = false;
private bool rts = false;

// second port of Cp2105 has limited baudRate, dataBits, stopBits, parity
// unsupported baudrate returns error at controlTransfer(), other parameters are silently ignored
private bool mIsRestrictedPort;


private void setConfigSingle(int request, int value) 
{
            int result = deviceConnection.ControlTransfer(REQTYPE_HOST_TO_DEVICE, request, value,
                    _portIndex, null, 0, USB_WRITE_TIMEOUT_MILLIS);
            if (result != 0) {
        throw new IOException("Control transfer failed: " + request + " / " + value + " -> " + result);
    }
}

private byte getStatus() 
{
            byte[]
    buffer = new byte[1];
            int result = deviceConnection.ControlTransfer(REQTYPE_DEVICE_TO_HOST, SILABSER_GET_MDMSTS_REQUEST_CODE, 0,
                    _portIndex, buffer, buffer.Length, USB_WRITE_TIMEOUT_MILLIS);
            if (result != buffer.Length) {
                throw new IOException("Control transfer failed: " + SILABSER_GET_MDMSTS_REQUEST_CODE + " / " + 0 + " -> " + result);
            }
            return buffer[0];
        }

    //    protected void openInt() 
    //    {
    //    mIsRestrictedPort = usbDevice.InterfaceCount == 2 && _portIndex == 1;
    //            if(_portIndex >= usbDevice.InterfaceCount) {
    //        throw new IOException("Unknown port number");
    //    }
    //    UsbInterface dataIface = usbDevice.GetInterface(_portIndex);
    //            if (!deviceConnection.ClaimInterface(dataIface, true)) {
    //        throw new IOException("Could not claim interface " + _portIndex);
    //    }
    //            for (int i = 0; i < dataIface.EndpointCount; i++) {
    //        UsbEndpoint ep = dataIface.GetEndpoint(i);
    //        if (ep.GetType() == UsbConstants.USB_ENDPOINT_XFER_BULK)
    //        {
    //            if (ep.Direction == UsbConstants.USB_DIR_IN)
    //            {
    //                    _endpoint_rx = ep;
    //                //mReadEndpoint = ep;
    //            }
    //            else
    //            {
    //                    _endpoint_tx = ep;
    //                //mWriteEndpoint = ep;
    //            }
    //        }
    //    }

    //    setConfigSingle(SILABSER_IFC_ENABLE_REQUEST_CODE, UART_ENABLE);
    //    setConfigSingle(SILABSER_SET_MHS_REQUEST_CODE, (dtr ? DTR_ENABLE : DTR_DISABLE) | (rts ? RTS_ENABLE : RTS_DISABLE));
    //    setFlowControl(mFlowControl);
    //}

    //protected void closeInt()
    //{
    //    try
    //    {
    //        setConfigSingle(SILABSER_IFC_ENABLE_REQUEST_CODE, UART_DISABLE);
    //    }
    //    catch (Exception ignored) { }
    //    try
    //    {
    //        deviceConnection.ReleaseInterface(usbDevice.GetInterface(_portIndex));
    //    }
    //    catch (Exception ignored) { }
    //}

    protected override void InitUSBSerial()
    {
        setConfigSingle(SILABSER_IFC_ENABLE_REQUEST_CODE, UART_ENABLE);
        setConfigSingle(SILABSER_SET_MHS_REQUEST_CODE, (dtr ? DTR_ENABLE : DTR_DISABLE) | (rts ? RTS_ENABLE : RTS_DISABLE));
        setFlowControl(FlowControl.NONE);

    }

    private void setBaudRate(int baudRate) 
{
    byte[] data = new byte[] {
                    (byte) ( baudRate & 0xff),
                    (byte) ((baudRate >> 8 ) & 0xff),
                    (byte) ((baudRate >> 16) & 0xff),
                    (byte) ((baudRate >> 24) & 0xff)
            };
    int ret = deviceConnection.ControlTransfer(REQTYPE_HOST_TO_DEVICE, SILABSER_SET_BAUDRATE_REQUEST_CODE,
            0, _portIndex, data, 4, USB_WRITE_TIMEOUT_MILLIS);
    if (ret < 0)
    {
        throw new IOException("Error setting baud rate");
    }
        }

    public void setParameters(int baudRate, int dataBits, int stopBits, int parity) 
    {
        if(baudRate <= 0) {
            throw new IllegalArgumentException("Invalid baud rate: " + baudRate);
        }

        setBaudRate(baudRate);

        int configDataBits = 0;
        switch (dataBits)
        {
            case 5:
                if (mIsRestrictedPort)
                    throw new UnsupportedOperationException("Unsupported data bits: " + dataBits);
                configDataBits |= 0x0500;
                break;
            case 6:
                if (mIsRestrictedPort)
                    throw new UnsupportedOperationException("Unsupported data bits: " + dataBits);
                configDataBits |= 0x0600;
                break;
            case 7:
                if (mIsRestrictedPort)
                    throw new UnsupportedOperationException("Unsupported data bits: " + dataBits);
                configDataBits |= 0x0700;
                break;
            case 8:
                configDataBits |= 0x0800;
                break;
            default:
                throw new IllegalArgumentException("Invalid data bits: " + dataBits);
        }

        switch (parity)
        {
            case 0: // PARITY_NONE:
                break;
            case 1: // PARITY_ODD:
                configDataBits |= 0x0010;
                break;
            case 2: // PARITY_EVEN:
                configDataBits |= 0x0020;
                break;
            case 3: // PARITY_MARK:
                if (mIsRestrictedPort)
                    throw new UnsupportedOperationException("Unsupported parity: mark");
                configDataBits |= 0x0030;
                break;
            case 4: // PARITY_SPACE:
                if (mIsRestrictedPort)
                    throw new UnsupportedOperationException("Unsupported parity: space");
                configDataBits |= 0x0040;
                break;
            default:
                throw new IllegalArgumentException("Invalid parity: " + parity);
        }

        switch (stopBits)
        {
            case 1://STOPBITS_1:
                break;
            case 2://STOPBITS_2:
                throw new UnsupportedOperationException("Unsupported stop bits: 2");
                break;
            case 3://STOPBITS_1_5:
                    throw new UnsupportedOperationException("Unsupported stop bits: 1.5");
            default:
                throw new IllegalArgumentException("Invalid stop bits: " + stopBits);
        }
        setConfigSingle(SILABSER_SET_LINE_CTL_REQUEST_CODE, configDataBits);
    }

    public bool getCD()
    {
        return (getStatus() & STATUS_CD) != 0;
    }

    public bool getCTS() 
    {
        return (getStatus() & STATUS_CTS) != 0;
    }

    public bool getDSR()
    { 
        return (getStatus() & STATUS_DSR) != 0;
    }

    public bool getDTR() 
    {
        return dtr;
    }

    public void setDTR(bool value) 
    {
        dtr = value;
        setConfigSingle(SILABSER_SET_MHS_REQUEST_CODE, dtr ? DTR_ENABLE : DTR_DISABLE);
    }

    public bool getRI() 
    {
                return (getStatus() & STATUS_RI) != 0;
    }

    public bool getRTS() 
    {
        return rts;
    }

        public void setRTS(bool value) 
        {
            rts = value;
            setConfigSingle(SILABSER_SET_MHS_REQUEST_CODE, rts ? RTS_ENABLE : RTS_DISABLE);
        }

//        public EnumSet<ControlLine> getControlLines() throws IOException
//{
//            byte status = getStatus();
//    EnumSet<ControlLine> set = EnumSet.noneOf(ControlLine.class);
////if(rts) set.add(ControlLine.RTS);                      // configured value
//if ((status & STATUS_RTS) != 0) set.add(ControlLine.RTS); // actual value
//if ((status & STATUS_CTS) != 0) set.add(ControlLine.CTS);
////if(dtr) set.add(ControlLine.DTR);                      // configured value
//if ((status & STATUS_DTR) != 0) set.add(ControlLine.DTR); // actual value
//if ((status & STATUS_DSR) != 0) set.add(ControlLine.DSR);
//if ((status & STATUS_CD) != 0) set.add(ControlLine.CD);
//if ((status & STATUS_RI) != 0) set.add(ControlLine.RI);
//return set;
//        }

//        public EnumSet<ControlLine> getSupportedControlLines()
//{
//            return EnumSet.allOf(ControlLine.class);
//        }

    public bool getXON()
    {
        byte[]    buffer = new byte[0x13];
        int result = deviceConnection.ControlTransfer(REQTYPE_DEVICE_TO_HOST, SILABSER_GET_COMM_STATUS_REQUEST_CODE, 0,
                _portIndex, buffer, buffer.Length, USB_WRITE_TIMEOUT_MILLIS);
        if (result != buffer.Length)
        {
            throw new IOException("Control transfer failed: " + SILABSER_GET_COMM_STATUS_REQUEST_CODE + " -> " + result);
        }
        return (buffer[4] & 8) == 0;
    }

        /**
         * emulate external XON/OFF
         * @throws IOException
         */
        public void setXON(bool value) 
        {
            setConfigSingle(value ? SILABSER_SET_XON_REQUEST_CODE : SILABSER_SET_XOFF_REQUEST_CODE, 0);
        }

    public void setFlowControl(FlowControl flowControl) 
    {
        byte[] data = new byte[16];
        if (flowControl == FlowControl.RTS_CTS)
        {
            data[4] |= 0b1000_0000; // RTS
            data[0] |= 0b0000_1000; // CTS
        }
        else
        {
            if (rts)
                data[4] |= 0b0100_0000;
        }
        if (flowControl == FlowControl.DTR_DSR)
        {
            data[0] |= 0b0000_0010; // DTR
            data[0] |= 0b0001_0000; // DSR
        }
        else
        {
            if (dtr)
                data[0] |= 0b0000_0001;
        }
        int retVal;
        if (flowControl == FlowControl.XON_XOFF)
        {
            byte[] chars = new byte[] { 0, 0, 0, 0, (byte)CHAR_XON, (byte)CHAR_XOFF };
            retVal = deviceConnection.ControlTransfer(REQTYPE_HOST_TO_DEVICE, SILABSER_SET_CHARS_REQUEST_CODE,
                    0, _portIndex, chars, chars.Length, USB_WRITE_TIMEOUT_MILLIS);
            if (retVal != chars.Length)
            {
                throw new IOException("Error setting XON/XOFF chars");
            }
            data[4] |= 0b0000_0011;
            data[7] |= 0b1000_0000;
            data[8] = (byte)128;
            data[12] = (byte)128;
        }
        if (flowControl == FlowControl.XON_XOFF_INLINE)
        {
            throw new UnsupportedOperationException();
        }
        retVal = deviceConnection.ControlTransfer(REQTYPE_HOST_TO_DEVICE, SILABSER_SET_FLOW_REQUEST_CODE,
                0, _portIndex, data, data.Length, USB_WRITE_TIMEOUT_MILLIS);
        if (retVal != data.Length)
        {
            throw new IOException("Error setting flow control");
        }
        if (flowControl == FlowControl.XON_XOFF)
        {
            setXON(true);
        }
        mFlowControl = flowControl;
    }


    // note: only working on some devices, on other devices ignored w/o error
    public void purgeHwBuffers(bool purgeWriteBuffers, bool purgeReadBuffers)
    { 
        int value = (purgeReadBuffers ? FLUSH_READ_CODE : 0)
                | (purgeWriteBuffers ? FLUSH_WRITE_CODE : 0);

        if (value != 0) {
            setConfigSingle(SILABSER_FLUSH_REQUEST_CODE, value);
        }
    }

    public void setBreak(bool value)
    {
        setConfigSingle(SILABSER_SET_BREAK_REQUEST_CODE, value ? 1 : 0);
    }

//    public static Map<Integer, int[]> getSupportedDevices()
//{
//    final Map<Integer, int[]> supportedDevices = new LinkedHashMap<>();
//    supportedDevices.put(UsbId.VENDOR_SILABS,
//    new int[] {
//            UsbId.SILABS_CP2102, // same ID for CP2101, CP2103, CP2104, CP2109
//            UsbId.SILABS_CP2105,
//            UsbId.SILABS_CP2108,
//    });
//    return supportedDevices;
//}

}