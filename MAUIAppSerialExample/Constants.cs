
namespace MAUIAppSerialExample;

public class Constants
{

    // this timeout is for the device hardware to setup, initialize and connect can be affected by device performance
    public const int COMMUNICATION_WAIT_CONNECT_TIMEOUT = 5000;
    // this timeout is for waiting for an external response
    public const int DEFAULT_COMM_NO_RESPONSE_TIMEOUT =3500;
    public const int MIN_WINDOW_HEIGHT_WINDOWS = 780;
    public const int MIN_WINDOW_WIDTH_WINDOWS = 650;
    public const String DEVICE_NO_RESPONSE = "<No Response>";
    public const String SERIAL_DEVICE_NAME = "OBDII";
    public const String DEVICE_NOT_SETUP = "** Device Not Set ***";
    public const String DEVICE_NOT_CONNECTED = "** Device is not connected or invalid ***";
    public const String SETTINGS_DEVICE_NAME = "selected_device_name";
    public const String SETTINGS_SEND_CR = "send_cr";
    public const String SETTINGS_EOT_CHARACTER = "eot_char";

    public const string COMMUNICATION_DEVICE_NOT_SETUP = "*** Device Not Setup ***";
    public const string PREFS_SERIAL_BAUD_RATE = "serial_baud";
    public const string PREFS_TEST_FOR_EOT = "eot_test";
    public const string PREFS_TEST_FOR_RESPONSE = "expect_response";

}
