using System.Net.NetworkInformation;

using Windows.Win32.Devices.Enumeration.Pnp;
using Windows.Win32.Foundation;

namespace FredrikHr.UpnpClient.WindowsNative;

internal class UPnPDeviceFinderCallback
    : IUPnPDeviceFinderCallback,
    IUPnPDeviceFinderAddCallbackWithInterface
{
    private static readonly ReaderWriterLockSlim NwIfLock = new();
    private static readonly Dictionary<Guid, NetworkInterface> NwIfs = [];

    private void OnDeviceAdded(int findData, IUPnPDevice device, NetworkInterface? networkInterface = null)
    {

    }
    
    unsafe void IUPnPDeviceFinderAddCallbackWithInterface
    .DeviceAddedWithInterface(
        int lFindData,
        IUPnPDevice pDevice,
        Guid* pguidInterface
        )
    {
        OnDeviceAdded(lFindData, pDevice, GetNetworkInterface(*pguidInterface));
    }

    void IUPnPDeviceFinderCallback
    .DeviceAdded(int lFindData, IUPnPDevice pDevice)
    {
        OnDeviceAdded(lFindData, pDevice);
    }

    void IUPnPDeviceFinderCallback
    .DeviceRemoved(int lFindData, BSTR bstrUDN)
    {
        throw new NotImplementedException();
    }

    void IUPnPDeviceFinderCallback
    .SearchComplete(int lFindData)
    {
        throw new NotImplementedException();
    }

    private static NetworkInterface? GetNetworkInterface(Guid networkInterfaceId)
    {
        NwIfLock.EnterReadLock();
        try
        {
            if (NwIfs.TryGetValue(networkInterfaceId, out var nwIf))
                return nwIf;
        }
        finally
        {
            NwIfLock.ExitReadLock();
        }

        return RefreshNetworkInterfaces(networkInterfaceId);
    }

    private static NetworkInterface? RefreshNetworkInterfaces(Guid searchNwIfId)
    {
        NwIfLock.EnterUpgradeableReadLock();
        try
        {
            if (NwIfs.TryGetValue(searchNwIfId, out var searchNwIf))
                return searchNwIf;
            NwIfLock.EnterWriteLock();
            try
            {
                foreach (var nwIf in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (!Guid.TryParse(nwIf.Id, out var nwIfId))
                        continue;
                    if (nwIfId == searchNwIfId)
                        searchNwIf = nwIf;
                    NwIfs[nwIfId] = nwIf;
                }
            }
            finally
            {
                NwIfLock.ExitWriteLock();
            }

            return searchNwIf;
        }
        finally
        {
            NwIfLock.ExitWriteLock();
        }
    }
}