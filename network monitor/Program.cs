using CommandLine;
using CommandLine.Text;
using Echevil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Web;
using System.Net;
using System.Security.Principal;

namespace nwmon
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FirstInitial();
                var options = new Options();
                var parser = new Parser();
                if (new Parser().ParseArguments(args, options))
                {
                    NetworkMonitor monitor = new NetworkMonitor();
                    var adapters = monitor.Adapters;
                    if (adapters.Length == 0)
                    {
                        Console.WriteLine("Network adapter interface not found, program terminated.");
                        return;
                    }
                    double dlLimit = options.Download;
                    double ulLimit = options.Upload;
                    int i = 0;
                    Console.WriteLine($"Select 1 of {adapters.Count()} below network interfaces");
                    monitor.StartMonitoring();
                    adapters.ToList().ForEach(adapter =>
                    {
                        Console.Write("Initializing...");
                        AdapterInitial(adapter, ref i);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }); //load adapter list, also measure which adapter is active for network connection
                    System.IO.File.Delete("temp");
                    Console.Write("select : ");
                    var selectedInterface = Convert.ToInt32(Console.ReadLine()) - 1;
                    var selectedAdapter = adapters[selectedInterface];
                    Monitor(dlLimit, ulLimit, selectedAdapter, options.Force, options.Interval);
                }
                else
                {

                    Console.WriteLine(options.GetUsage());
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("You must choose one of the interfaces above.");
                Environment.Exit(0);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("You can't select non-exists network interface.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(0);
            }
        }

        private static void AdapterInitial(NetworkAdapter adapter, ref int i)
        {
            var measurement = 0.0; //measuring by try loading some small file into the local disk, then read the adapter download speed (which would become > 0 if it's a primary network interface)
            using (var client = new WebClient())
            {
                client.DownloadFile("https://msdn.microsoft.com/en-us/library/aa364992(VS.85).aspx", "temp");
                measurement += adapter.DownloadSpeed;
            }
            if (measurement != 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\r[{++i}]{adapter.Name} (*)");
            }
            else
            {
                Console.WriteLine($"\r[{++i}]{adapter.Name}");
            }
        }
        private static void Monitor(double dlLimit, double ulLimit, NetworkAdapter selectedAdapter, bool bypass, int interval)
        {
            var result = string.Empty;
            double sumdl = 0;
            double sumul = 0;
            var adapterName = Unknownlength_Substring(selectedAdapter.Name, 25); //we can't just use shorthand if on unknown length of string, thus it's can make IndexOutOfRange
            while (true)
            {
                var dl = selectedAdapter.DownloadSpeedKbps / 1024.0;
                var ul = selectedAdapter.UploadSpeedKbps / 1024.0;
                var dlMbps = dl * 8;
                var ulMbps = ul * 8;
                if ((dlMbps > dlLimit) | (ulMbps > ulLimit))
                {
                    if (!bypass)
                    {
                        Console.Beep();
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    result = $"\r[SPEED EXCEEDING LIMIT] {adapterName} | Download : {string.Format("{0:0.00}", dl)} Mbps, Upload {string.Format("{0:0.00}", ul)} Mbps {Space(10)}";
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    result = $"\r{adapterName} | Download : {string.Format("{0:0.00}", dlMbps)} Mbps, Upload {string.Format("{0:0.00}", ulMbps)} Mbps {Space(10)}";
                }
                sumdl += dl;
                sumul += ul;
                Console.Title = $"[{AppDomain.CurrentDomain.FriendlyName}]Total Download {Math.Round(sumdl, 2)/(10/(interval/100))} MB / Upload {Math.Round(sumul, 2)/(10/ (interval / 100))} MB";
                Console.Write(result);
                Thread.Sleep(interval);
            }
        }
        private static string Space(int size)
        {
            return new String(' ', size);
        }
        private static void EvaluateArgs<T>(ref T x, T newval)
        {
            x = newval;
        }
        private static string Unknownlength_Substring(string s, int length)
        {
            if (s.Length <= length)
                return s;
            return s.Substring(0, length);
        }
        private static void FirstInitial() //writing this application to systemroot, so it will be accessible anywhere
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            //Console.WriteLine(principal.IsInRole(WindowsBuiltInRole.Administrator));
            try
            {
                var rootfile = $@"{Environment.GetEnvironmentVariable("SystemRoot")}\{AppDomain.CurrentDomain.FriendlyName}";
                if (principal.IsInRole(WindowsBuiltInRole.Administrator) && !(System.IO.File.Exists(rootfile)))
                {
                    System.IO.File.Copy(AppDomain.CurrentDomain.FriendlyName, rootfile);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("To make the program accessible anywhere, please run as administrator once.");
            }
        }
    }
}
