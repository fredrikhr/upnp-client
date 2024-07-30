using System.Collections;
using System.Diagnostics;

using Windows.Win32.System.Com;

namespace Windows.Win32;

internal static partial class System_Com_IEnumUnknown_Extensions
{
    private class Enumerator(IEnumUnknown nativeEnumerator) : IEnumerator
    {
        private readonly object[] _current = [null!];

        public object Current => _current[0];

        public unsafe bool MoveNext()
        {
            uint cRecv;
            var hr = nativeEnumerator.Next(_current, &cRecv);
            if (hr.Succeeded)
            {
                Debug.Assert(cRecv == 1U);
                return true;
            }
            if (hr != Foundation.HRESULT.S_FALSE)
            {
                hr.ThrowOnFailure();
            }
            return false;
        }

        public void Reset()
        {
            nativeEnumerator.Reset();
            _current[0] = null!;
        }
    }

    internal static IEnumerator GetEnumerator(this IEnumUnknown enumUnknown)
    {
        ArgumentNullException.ThrowIfNull(enumUnknown);
        return new Enumerator(enumUnknown);
    }
}
