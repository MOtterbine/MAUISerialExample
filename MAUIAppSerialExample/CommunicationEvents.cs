using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIAppSerialExample;

public enum CommunicationEvents
{
    ConnectedAsClient,
    ClientConnected,
    RemoteDisconnect,
    Disconnected,
    Transmit,
    TransmitEnd,
    Receive,
    ReceiveEnd,
    Listening,
    Connecting,
    LinkInitFailure,
    LinkInitSuccess,
    Information,
    Error
}
