using System.Collections;

using Windows.Win32.Devices.Enumeration.Pnp;
using Windows.Win32.System.Com;

namespace Windows.Win32;

internal static partial class Devices_Enumeration_Pnp_IUPnPDevices_Extensions
{
    private class Enumerable(IUPnPDevices devices) : IEnumerable
    {
        public IEnumerator GetEnumerator() => devices.GetEnumerator();
    }

    internal static IEnumerator GetEnumerator(this IUPnPDevices devices) =>
        ((IEnumUnknown)(devices._NewEnum)).GetEnumerator();

    internal static IEnumerable ToEnumerable(this IUPnPDevices devices) =>
        new Enumerable(devices);
}
