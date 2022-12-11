using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace azure
{
    class Program
    {
        static void Main(string[] args)
        {
            // United States geography has the following regions:
            // Central US, East US, East US 2, North Central US, 
            // South Central US, West Central US, West US, West US 2
            // This list is accurate as of 8/26/2021
            List<string> USGeographyRegions = new List<string>
            {
                "centralus",
                "eastus",
                "eastus2",
                "northcentralus",
                "southcentralus",
                "westcentralus",
                "westus",
                "westus2"
            };

            // Load the weekly file
            var today = DateTime.UtcNow;
            var monday = today.AddDays(DayOfWeek.Monday - today.DayOfWeek);
            var url = @"https://download.microsoft.com/download/7/1/D/71D86715-5596-4529-9B13-DA13A5DE5B63/ServiceTags_Public_" + monday.ToString("yyyyMMdd") + ".json";
            var fileName = monday.ToString("yyyyMMdd") + ".json";
            var client = new WebClient();
            Console.WriteLine("Download " + url + " to " + fileName + ".");
            client.DownloadFile(url, fileName);
            JObject weeklyFile = JObject.Parse(File.ReadAllText(fileName));
            JArray values = (JArray)weeklyFile["values"];

            var ipAddresses = new List<String>();

            foreach (string region in USGeographyRegions)
            {
                string azureCloudRegion = $"AzureCloud.{region}";

                var ipList =
                    from v in values
                    where (string)v["name"] == azureCloudRegion
                    select v["properties"]["addressPrefixes"];

                foreach (var ip in ipList.Children())
                {
                    ipAddresses.Add(ip.ToString());
                }
            }

            System.IO.File.WriteAllText(@"azure-us.txt", String.Join(Environment.NewLine, ipAddresses));
            System.IO.File.WriteAllText(@"azure-us-v4.txt", String.Join(Environment.NewLine, ipAddresses.Where(ip => ip.IndexOf(":") < 0)));
            System.IO.File.WriteAllText(@"azure-us-v6.txt", String.Join(Environment.NewLine, ipAddresses.Where(ip => ip.IndexOf(":") > 0)));
            System.IO.File.WriteAllText(@"azure-us.csv", String.Join(",", ipAddresses));
            System.IO.File.WriteAllText(@"azure-us-v4.csv", String.Join(",", ipAddresses.Where(ip => ip.IndexOf(":") < 0)));
            System.IO.File.WriteAllText(@"azure-us-v6.csv", String.Join(",", ipAddresses.Where(ip => ip.IndexOf(":") > 0)));

            System.IO.File.Delete(fileName);
        }
    }
}
