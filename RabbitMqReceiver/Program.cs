using System;
using System.Xml.Linq;

namespace RabbitMqReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            string str =
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><add_user><name>lonzo</name><uuid>55</uuid><email>stefack@student.ehb.be</email><street>Nijverheidskaai 177</street><municipal>Chino Hills LA</municipal><postalCode>9000</postalCode><vat>123</vat></add_user>";
            XDocument doc = XDocument.Parse(str);
            Console.WriteLine(doc);

        }
    }
}
