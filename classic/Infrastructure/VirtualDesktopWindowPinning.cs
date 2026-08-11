using System;
using System.Runtime.InteropServices;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class VirtualDesktopWindowPinning
    {
        private static readonly Guid ImmersiveShellClassId = new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
        private static readonly Guid VirtualDesktopPinnedAppsServiceId = new Guid("B5A399E7-1C87-46B8-88E9-FC5747B171BD");

        internal void SetPinned(IntPtr window, bool pinned)
        {
            if (window == IntPtr.Zero) return;

            object shellObject = null;
            object collectionObject = null;
            object pinnedAppsObject = null;
            IApplicationView view = null;

            try
            {
                shellObject = Activator.CreateInstance(Type.GetTypeFromCLSID(ImmersiveShellClassId));
                IServiceProvider shell = (IServiceProvider)shellObject;

                Guid collectionId = typeof(IApplicationViewCollection).GUID;
                collectionObject = shell.QueryService(ref collectionId, ref collectionId);
                IApplicationViewCollection collection = (IApplicationViewCollection)collectionObject;
                collection.GetViewForHwnd(window, out view);

                Guid pinnedAppsId = typeof(IVirtualDesktopPinnedApps).GUID;
                Guid serviceId = VirtualDesktopPinnedAppsServiceId;
                pinnedAppsObject = shell.QueryService(ref serviceId, ref pinnedAppsId);
                IVirtualDesktopPinnedApps pinnedApps = (IVirtualDesktopPinnedApps)pinnedAppsObject;
                bool isPinned = pinnedApps.IsViewPinned(view);
                if (pinned && !isPinned) pinnedApps.PinView(view);
                if (!pinned && isPinned) pinnedApps.UnpinView(view);
            }
            catch (Exception)
            {
                // Virtual desktop pinning is an undocumented Windows shell service.
                // Keep the overlay usable if it is unavailable on a Windows build.
            }
            finally
            {
                ReleaseComObject(view);
                ReleaseComObject(pinnedAppsObject);
                ReleaseComObject(collectionObject);
                ReleaseComObject(shellObject);
            }
        }

        private static void ReleaseComObject(object value)
        {
            if (value == null || !Marshal.IsComObject(value)) return;
            try { Marshal.ReleaseComObject(value); }
            catch (Exception) { }
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        [Guid("372E1D3B-38D3-42E4-A15B-8AB2B178F513")]
        private interface IApplicationView
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("1841C6D7-4F9D-42C0-AF41-8747538F10E5")]
        private interface IApplicationViewCollection
        {
            int GetViews(out object views);
            int GetViewsByZOrder(out object views);
            int GetViewsByAppUserModelId(string id, out object views);
            int GetViewForHwnd(IntPtr window, out IApplicationView view);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("4CE81583-1E4C-4632-A621-07A53543148F")]
        private interface IVirtualDesktopPinnedApps
        {
            bool IsAppIdPinned(string appId);
            void PinAppID(string appId);
            void UnpinAppID(string appId);
            bool IsViewPinned(IApplicationView view);
            void PinView(IApplicationView view);
            void UnpinView(IApplicationView view);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
        private interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid service, ref Guid interfaceId);
        }
    }
}
