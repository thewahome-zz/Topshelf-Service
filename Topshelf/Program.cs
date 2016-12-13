using System;
using System.IO;
using System.Timers;
using log4net;

namespace Topshelf
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureService.Configure();
        }

    }
    internal static class ConfigureService
    {
        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<ServiceManager>(service =>
                {
                    service.ConstructUsing(s => new ServiceManager());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();
                configure.SetServiceName("MyWindowServiceWithTopshelf");
                configure.SetDisplayName("MyWindowServiceWithTopshelf");
                configure.SetDescription("My .Net windows service with Topshelf");
            });
        }
    } 

    public class ServiceManager
    {
        private Timer timer;

        public ServiceManager()
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var fileName = "TopshelfLog.txt";

            // Import Namespace for 'Path': using Sytem.IO
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // Write current time to a file
            using (var writer = File.AppendText(fullPath))
            {
                writer.WriteLine(DateTime.Now);
            }
        }

        public void Start()
        {
            timer.Enabled = true;
            timer.Start();
        }
        public void Stop()
        {
            if (timer != null)
            {
                timer.Enabled = false;
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }
    }
}
