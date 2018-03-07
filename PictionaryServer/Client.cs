using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PictionaryServer
{
    class Client
    {
        public byte id { get; set; }
        public IPEndPoint ipEndPoint { get; }
        public byte colorRed { get; set; }
        public byte colorGreen { get; set; }
        public byte colorBlue { get; set; }
        public double x { get; set; }
        public double y { get; set; }

        public Client(byte id, IPEndPoint ipEndPoint, byte colorRed, byte colorGreen, byte colorBlue)
        {
            this.id = id;
            this.ipEndPoint = ipEndPoint;
            this.colorRed = colorRed;
            this.colorGreen = colorGreen;
            this.colorBlue = colorBlue;
        }

        
    }
}
