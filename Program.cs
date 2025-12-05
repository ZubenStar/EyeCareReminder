using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;

namespace EyeCareReminder
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Initialize Windows App SDK Bootstrap for unpackaged apps
            Microsoft.Windows.ApplicationModel.DynamicDependency.Bootstrap.Initialize(0x00010006);
            
            WinRT.ComWrappersSupport.InitializeComWrappers();
            
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }
    }
}