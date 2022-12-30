namespace SettingsUI.Helpers;
public class WindowsSystemDispatcherQueueHelper
{
    private object m_dispatcherQueueController = null;
    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            // one already exists, so we'll just use it.
            return;
        }

        if (m_dispatcherQueueController == null)
        {
            NativeMethods.DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(NativeMethods.DispatcherQueueOptions));
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            NativeMethods.CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }
}
