using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace ReliablePubSub.Client
{
    class Program
    {
        private static void Main(string[] args)
        {
            new Simple().Run("tcp://localhost:6669");
        }
    }
}
