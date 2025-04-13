
namespace MAUIAppSerialExample;

/// <summary>
/// Defines functionality specific to serial device communication
/// </summary>
public interface ISerialDevice
{
    uint BaudRate { get; set; }
}
