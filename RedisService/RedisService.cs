using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace RedisService
{
    public class RedisService : ServiceBase
    {
        const string RedisServer = "redis-server.exe";
        //const string RedisCLI = "redis-cli.exe";
        private string _path;
        private int _port;
        private Process _process;

        protected override void OnStart(string[] args)
        {
            _path = AppDomain.CurrentDomain.BaseDirectory;
            if (args.Length > 2)
                Exit("Too many arguments");
            StartRedis(args.Length == 2 ? args[1] : null);
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopRedis();
        }

        private void StartRedis(string configPath = null)
        {
            var pi = new ProcessStartInfo(Path.Combine(_path, RedisServer));

            if (configPath != null)
            {
                FindPort(configPath);
                // Workaround for spaces in configuration filename.
                pi.Arguments = Path.GetFileName(configPath);
                pi.WorkingDirectory = Path.GetDirectoryName(configPath);
            }

            _process = new Process { StartInfo = pi };

            if (!_process.Start())
                Exit("Failed to start Redis process");
        }

        private void FindPort(string path)
        {
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.IndexOf("port") == 0)
                    {
                        _port = int.Parse(line.Substring(5, line.Length - 5));
                        break;
                    }
                }
                if (_port == 0)
                    Exit("Couldn`t find Redis port in config file");
            }
        }

        private void StopRedis()
        {
            _process.Kill();
        }

        static void Exit(string message)
        {
            throw new ApplicationException(message);
        }
    }
}
