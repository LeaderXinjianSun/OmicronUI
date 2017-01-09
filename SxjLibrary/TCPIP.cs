using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using BingLibrary.hjb;

namespace SxjLibrary
{
    public class TCPIPConnect
    {
        TcpClient client = new TcpClient();
        NetworkStream stream;
        public bool tcpConnected { set; get; } = false;
        public async Task<bool> Connect(string ip, int port)
        {
            bool r = false;
            Func<Task> taskFun = () =>
            {
                return Task.Run(() =>
                {
                    try
                    {
                        client.SendTimeout = 600;
                        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
                        client.Connect(ipe);
                        tcpConnected = true;
                        r = true;
                    }
                    catch (Exception ex) { client.Close(); client = new TcpClient(); tcpConnected = false;
                        r = false;
                        Log.Default.Error("TCPIP.Connect", ex.Message); }
                });
            };

            Task taskDelay = Task.Delay(6000);
            var completedTask = await Task.WhenAny(taskFun(), taskDelay);
            if (completedTask == taskDelay)
            {

                client.Close(); client = new TcpClient();
                r = false;
            }

            return r;
        }
        
        public string SendThenReceive(string message)
        {

            try
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message + "\r\n");
                stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                data = new Byte[256];
                string responseData = string.Empty;
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                return responseData;
            }
            catch { return "error"; }
        }
        public void IsOnline()
        {
            try
            {
                tcpConnected = !((client.Client.Poll(1, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected);
            }
            catch { tcpConnected = false; }
        }
        public async Task<string> ReceiveAsync()
        {

            try
            {
                byte[] data = new Byte[256];
                string responseData = string.Empty;
                stream = client.GetStream();
                Int32 bytes = await stream.ReadAsync(data, 0, data.Length);
                responseData = System.Text.Encoding.GetEncoding("GBK").GetString(data, 0, bytes);
                return responseData;
            }
            catch (Exception ex)
            {
                return "error";
                Log.Default.Error("TCPIP.ReceiveAsync", ex.Message);
            }

        }
        public async Task<string> SendAsync(string message)
        {
            try
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message + "\r\n");
                stream = client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                //await Task.Delay(50);
                return "";
            }
            catch (Exception ex) { tcpConnected = false; return "error";
                Log.Default.Error("TCPIP.SendAsync", ex.Message);
            }

        }
    }
    public class TcpListenerServer
    {
        TcpListener server = null;
        public bool ServerStatus = false;
        public TcpListenerServer(string ip, Int32 port)
        {
            try
            {
                Int32 MyPort = port;
                IPAddress MyLocalAddr = IPAddress.Parse(ip);
                server = new TcpListener(MyLocalAddr, MyPort);
                server.Start();
            }
            catch (Exception e)
            {
                Log.Default.Error("TcpListenerServer", e.Message);
            }

        }
        public delegate string ProcessedDelegate(string msgstr);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">收到的字符串，处理一下，再传出相应信息</param>
        public void run(ProcessedDelegate callback)
        {
            Byte[] bytes = new Byte[512];
            String data = null;
            while (true)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    int i;
                    ServerStatus = true;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        string str = callback(data);
                        byte[] bytes1 = System.Text.Encoding.ASCII.GetBytes(str);
                        stream.Write(bytes1, 0, bytes1.Length);
                    }
                }
                catch (Exception ex)
                {
                    Log.Default.Error("TcpListenerServer_run", ex.Message);
                    ServerStatus = false;
                }

            }

        }
    }
}
