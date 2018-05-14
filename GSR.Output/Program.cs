using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSR.Output
{

    struct GSROutput
    {
        public float time;
        public int gsrValue;
    }

    class SerialReader
    {
        SerialPort port;
        bool isRunning = false;
        string outputFilename = "output.json";
        List<GSROutput> gsrValues = new List<GSROutput>();
        public SerialReader()
        {
            port = new SerialPort();
            port.PortName = "COM4";
            port.BaudRate = 9600;
        }

        public void Run()
        {
            isRunning = true;
            var task = Task.Factory.StartNew(() =>
            {
                float totalTime = 0;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                port.Open();
                while (isRunning)
                {
                    var line = port.ReadLine();
                    totalTime += stopwatch.ElapsedMilliseconds / 1000f;
                    Console.WriteLine(line);
                    Console.WriteLine(totalTime);
                    var lineVal = new GSROutput();
                    if (string.IsNullOrEmpty(line)) continue;
                    lineVal.time = totalTime;
                    lineVal.gsrValue = Convert.ToInt32(line);
                    gsrValues.Add(lineVal);
                    stopwatch.Restart();
                    Thread.Sleep(100);

                }
                port.Close();
            });
        }

        public void Stop()
        {
            isRunning = false;
            string json = JsonConvert.SerializeObject(gsrValues.ToArray());
            System.IO.File.WriteAllText(outputFilename, json);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SerialReader reader = new SerialReader();
            reader.Run();
            var str = Console.ReadLine();
            reader.Stop();
        }
    }
}
