using System;
using System.Configuration;
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
                configure.SetDisplayName("Topshelf Service");
                configure.SetDescription("Topshelf service with differently timed methods");
            });
        }
    }

    public class ServiceManager
    {
        private Timer timer;
        private Timer sendToApiTimer;
        private int valueOne;
        private int valueTwo;
        public void Start()
        {
            WriteMessage("-- Service started -- ");
            sendToApiTimer = new Timer();
            valueOne = int.Parse(ConfigurationManager.AppSettings["FirstIntervalMinutes"]);
            valueTwo = int.Parse(ConfigurationManager.AppSettings["SecondIntervalMinutes"]);
            sendToApiTimer.Interval = valueOne * 60 * 1000;
            sendToApiTimer.Elapsed += api_timerElapsed;
            sendToApiTimer.Enabled = true;
            sendToApiTimer.Start();

            timer = new Timer();
            timer.Interval = 2 * 60 * 1000;
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;
            timer.Start();

            Initiate1600Method();
        }

        private void api_timerElapsed(object sender, ElapsedEventArgs e)
        {
            MethodToRunEveryOneMinute();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MethodToRunEveryFiveMinutes();
        }

        private void MethodToRunEveryOneMinute()
        {
            string message;
            message = "[Runs every " + valueOne + " minute(s)] - ";
            WriteMessage(message);
        }

        private void Initiate1600Method()
        {
            string timespan = ConfigurationManager.AppSettings["DailyTime"];
            TimeSpan span = TimeSpan.Parse(timespan);
            var runAt = DateTime.Today + span;
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
            message = "[Runs every " + valueTwo + " minute(s)] - ";
            WriteMessage(message);
        }

        private void MethodtoRunAt1600()
        {
            string message;
            message = "[Activity Ran at 1600 hrs] - ";
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
            if (timer != null && sendToApiTimer != null)
            {
                timer.Enabled = false;
                timer.Stop();
                timer.Dispose();
                timer = null;

                sendToApiTimer.Enabled = false;
                sendToApiTimer.Stop();
                sendToApiTimer.Dispose();
                sendToApiTimer = null;

                WriteMessage("-- Service Stopped -- ");
            }
        }


    }
}
