using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetMod
{
    class ModbusRun
    {
        private ModbusSlave slave;

        public ModbusRun()
        {
            int port = 503;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = IPAddress.Parse("127.0.0.1");
            TcpListener tcpListener = new TcpListener(addr, port);
            slave = ModbusTcpSlave.CreateTcp(1, tcpListener);
            slave.DataStore = Modbus.Data.DataStoreFactory.CreateDefaultDataStore();
            slave.Listen();
        }

        public void setHoldingValue(int register, ushort value)
        {
            slave.DataStore.HoldingRegisters[register] = value;
        }

        public void setHoldingFloat(int firstRegister, float value)
        {
            ushort[] words = float2doubleushort(value);
            setHoldingValue(firstRegister,   words[0]);
            setHoldingValue(firstRegister+1, words[1]);
        }

        private ushort[] float2doubleushort(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            ushort a = (ushort)(bytes[3] | (bytes[2] << 8));
            ushort b = (ushort)(bytes[1] | (bytes[0] << 8));
            return new ushort[] { a, b };
        }
    }
}