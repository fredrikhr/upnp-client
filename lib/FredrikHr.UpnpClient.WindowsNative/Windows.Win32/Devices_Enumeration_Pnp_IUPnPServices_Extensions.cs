using System.Collections;

using Windows.Win32.Devices.Enumeration.Pnp;
using Windows.Win32.System.Com;

namespace Windows.Win32;

internal static partial class Devices_Enumeration_Pnp_IUPnPServices_Extensions
{
    private class Enumerable(IUPnPServices services) : IEnumerable
    {
        public IEnumerator GetEnumerator() => services.GetEnumerator();
    }

    internal static IEnumerator GetEnumerator(this IUPnPServices services) =>
        ((IEnumUnknown)(services._NewEnum)).GetEnumerator();

    internal static IEnumerable ToEnumerable(this IUPnPServices services) =>
        new Enumerable(services);
}
