using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;


namespace LogApp
{
    class Program
    {
        private static UdpClient srv = new UdpClient(514);
        private static DateTime _time = new DateTime();
        private static string _message;
        private static List<string> _messages = new List<string>();
        private static Dictionary<string, string> id = new Dictionary<string, string>(); 
        public static List< Tuple<string, string, string, string, string, string, string> > parser(List<string> messages)
        {
            UInt16 pos, last_pos;
            string ip_addr;
            string date;
            string time;
            string message_source;
            string message_state = "";
            string mac_addr = "";
            string host = "";
            List<Tuple<string, string, string, string, string, string, string>> list_of_tuples = new List<Tuple<string, string, string, string, string, string, string>> ();

            foreach (var source in messages)
            {
                last_pos = 0;
                pos = 0;
                // IP
                pos = (UInt16)source.IndexOf(';', pos);
                ip_addr = source.Substring(0, pos);
                last_pos = pos;
                // Дата
                pos = (UInt16)source.IndexOf(';', pos + 1);
                date = source.Substring(last_pos + 1, pos - (last_pos + 1));
                last_pos = pos;
                // Время
                pos = (UInt16)source.IndexOf(';', pos + 1);
                time = source.Substring(last_pos + 1, pos - (last_pos + 1));
                last_pos = pos;
                // Источник сообщения
                pos = (UInt16)source.IndexOf(' ', pos + 1);
                message_source = source.Substring(last_pos + 1, pos - (last_pos + 1));
                last_pos = pos;

                if (message_source == "wireless,info")
                {
                    // MAC
                    pos = (UInt16)source.IndexOf('@', pos + 1);
                    mac_addr = source.Substring(last_pos + 1, pos - (last_pos + 1));
                    // State
                    pos = (UInt16)source.IndexOf(' ', pos + 1);
                    last_pos = pos;
                    pos = (UInt16)source.IndexOf(',', pos + 1);
                    message_state = source.Substring(last_pos + 1, pos - (last_pos + 1));
                    // tuple find mac::name/////
                }
                else if (message_source == "dhcp,info")
                {
                    // State
                    pos = (UInt16)source.IndexOf(' ', pos + 1);
                    last_pos = pos;
                    pos = (UInt16)source.IndexOf(' ', pos + 1);
                    message_state = source.Substring(last_pos + 1, pos - (last_pos + 1));
                    // MAC, быдло-стиль
                    last_pos = Convert.ToUInt16(source.Length - 17);
                    pos = 17;
                    mac_addr = source.Substring(last_pos, pos);
                }
                if (ip_addr == "192.168.1.1")
                {
                    host = "Office";
                }
                else if (ip_addr == "192.168.16.1")
                {
                    host = "Remote_Office";
                }

                list_of_tuples.Add(Tuple.Create(ip_addr, date, time, message_source, message_state, mac_addr, host));
            }



            //Console.WriteLine(ip_addr);
            //Console.WriteLine(date);
            //Console.WriteLine(time);
            //Console.WriteLine(message_source);
            //Console.WriteLine(message_state);
            //Console.WriteLine(mac_addr);
            _messages.Clear();
            return list_of_tuples;
        }
        // Добавляем строку с сообщением в список, добавляя ip и дату-время
        private static void addMessage(string message, IPEndPoint iP)
        {
            _time = DateTime.Now;
            string result = iP.Address.ToString() + ';' + 
                            _time.Day.ToString()+'.'+_time.Month.ToString()+'.'+_time.Year.ToString()+';'+
                            _time.Hour.ToString()+':'+_time.Minute.ToString()+':'+_time.Second.ToString()+';'+
                            message;
            _messages.Add(result);
            Console.WriteLine(result);
            parser(_messages);
        }
        static void Main(string[] args)
        {
            Console.WriteLine(_messages.Count);
            for (; ; )
            {
                
                if (srv.Available>0)
                {
                    byte[] data;
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    data = srv.Receive(ref endPoint);
                    _message = Encoding.UTF8.GetString(data);
                    addMessage(_message, endPoint);
                    //_messages.Add(_message);
                    //Console.WriteLine(Encoding.UTF8.GetString(data));

                    Console.WriteLine(_messages.Count);

                }
                Thread.Sleep(1);
            }
            
        }
    }
}
