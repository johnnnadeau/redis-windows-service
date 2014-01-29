using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace RedisService
{
    public class RedisService : ServiceBase
    {
        const string RedisServer = "redis-server.exe";
        private string _path;
        private List<Process> _process = new List<Process>();

        protected override void OnStart(string[] args)
        {
            _path = AppDomain.CurrentDomain.BaseDirectory;
            base.OnStart(args);
            StartRedis();        
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopRedis();
        }

        private void StartRedis()
        {
            var pi = new ProcessStartInfo(Path.Combine(_path, RedisServer));

            foreach (var file in Directory.GetFiles(_path, "*.conf"))
            {
                string configPath = Path.Combine(_path, file);
                int port = FindPort(configPath);
                // Workaround for spaces in configuration filename.
                pi.Arguments = Path.GetFileName(configPath);
                pi.WorkingDirectory = Path.GetDirectoryName(configPath);

                Process process = new Process {StartInfo = pi};
                _process.Add(process);
                if (!process.Start())
                    Exit("Failed to start Redis process for port " + port);
            }            
        }

        private int FindPort(string path)
        {
            int port = 0;
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.IndexOf("port") == 0)
                    {
                        port = int.Parse(line.Substring(5, line.Length - 5));
                        break;
                    }
                }
                if (port == 0)
                    Exit("Couldn`t find Redis port in config file");
            }

            return port;
        }

        private void StopRedis()
        {
            foreach (var process in _process)
            {
                process.Kill();
            }           
        }

        static void Exit(string message)
        {
            throw new ApplicationException(message);
        }
    }
}
