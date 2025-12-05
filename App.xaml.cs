using Microsoft.UI.Xaml;
using System;

namespace EyeCareReminder
{
    public partial class App : Application
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Initialize Bootstrap for unpackaged apps before anything else
            Microsoft.Windows.ApplicationModel.DynamicDependency.Bootstrap.Initialize(0x00010006);
            WinRT.ComWrappersSupport.InitializeComWrappers();
            
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                    Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window? m_window;
    }
}