using System.IO.Ports;
using System.Linq;

namespace Showcase.PU.Core;

public static class PortUtil
{
    public static string[] GetPorts() =>
        SerialPort.GetPortNames().OrderBy(x => x).ToArray();
}
