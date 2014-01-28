using System.ServiceProcess;

namespace RedisService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBase.Run(new RedisService());
        }      
    }
}
