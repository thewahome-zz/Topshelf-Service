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
                configure.SetServiceName("TopshelfService");
                configure.SetDisplayName("TopshelfService");
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
            timer.Interval = 5 * 60 * 1000;
            timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MethodToRunEveryFiveMinutes();
        }

        public void Start()
        {
            timer.Enabled = true;
            timer.Start();
            TimeSpan span = TimeSpan.Parse("15:25:01");
            var runAt = DateTime.Today + span;
            //var runAt = DateTime.Today + TimeSpan.FromHours(15);
            if (runAt < DateTime.Now)
            {
                MethodtoRunAt1600();
            }
            else
            {
                var dueTime = runAt - DateTime.Now;
                new System.Threading.Timer(_ => MethodtoRunAt1600(), null, dueTime, TimeSpan.Zero);
            }
        }

        private void MethodToRunEveryFiveMinutes()
        {
            string message;
            message = "[Runs every five minutes] - ";
            WriteMessage(message);
        }

        private void MethodtoRunAt1600()
        {
            string message;
            message = "[Activity Ran at 1500 hrs] - ";
            WriteMessage(message);
        }

        private static void WriteMessage(string message)
        {
            var fileName = "TopshelfLog.txt";
            // Import Namespace for 'Path': using Sytem.IO
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // Write current time to a file
            using (var writer = File.AppendText(fullPath))
            {
                writer.WriteLine(message + DateTime.Now);
            }
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
