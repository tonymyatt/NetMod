using HtmlAgilityPack;
using System;

namespace NetMod
{
    struct WanStats
    {
        public float received, sent;
        public bool valid;
    }

    class HtmlValues
    {
        public WanStats loadOptusWanStats()
        {
            WanStats stats = new WanStats();
            stats.valid = false;

            try
            {
                string url = "http://192.168.1.6/statswan.cmd";
                HtmlWeb web = new HtmlWeb();

                HtmlDocument doc = web.Load(url);

                // Collect WAN rview information, which is contained in a tr
                HtmlNodeCollection items = doc.DocumentNode.SelectNodes("//tr");
                foreach (HtmlNode node in items)
                {

                    HtmlNode n1 = node.SelectSingleNode("td[2]");
                    HtmlNode n2 = node.SelectSingleNode("td[3]");
                    HtmlNode n3 = node.SelectSingleNode("td[11]");

                    if (n1 == null || n2 == null || n3 == null)
                    {
                        continue;
                    }

                    if (!n1.InnerText.Equals("ipoe_0_1_1.0"))
                    {
                        continue;
                    }

                    stats.received = HtmlValueToFloat(n2.InnerText);
                    stats.sent = HtmlValueToFloat(n3.InnerText);
                    stats.valid = true;
                    Console.WriteLine("Updated Optus WAN Statistsics {0} MB Recieved, {1} MB Sent", stats.received/1024/1024, stats.sent/1024/1024);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot Update Optus WAN Statistsics");
            }

            return stats;
        }

        private float HtmlValueToFloat(string s)
        {
            float v = 0;
            float.TryParse(s, out v);
            return v;
        }
    }
}
