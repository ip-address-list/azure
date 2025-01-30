using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace azure
{
    class Program
    {
        static void Main(string[] args)
        {
            // Azure Geographies in the United States, as of January 2025.
            List<string> USGeographyRegions = new List<string>
            {
                "centralus",
                "eastus",
                "eastus2",
                "northcentralus",
                "southcentralus",
                "westcentralus",
                "westus",
                "westus2",
                "westus3"
            };

            var today = DateTime.UtcNow;
            var monday = today.AddDays(DayOfWeek.Monday - today.DayOfWeek);
            var url = @"https://download.microsoft.com/download/7/1/D/71D86715-5596-4529-9B13-DA13A5DE5B63/ServiceTags_Public_" + monday.ToString("yyyyMMdd") + ".json";
            var fileName = monday.ToString("yyyyMMdd") + ".json";
            var client = new HttpClient();
            var jsonDoc = JsonDocument.Parse(client.GetStringAsync(url).Result);
            var values = jsonDoc.RootElement.GetProperty("values");

            var ipAddresses = new List<string>();

            foreach (string region in USGeographyRegions)
            {
                string azureCloudRegion = $"AzureCloud.{region}";

                var ipList = values.EnumerateArray()
                    .Where(v => v.GetProperty("name").GetString() == azureCloudRegion)
                    .SelectMany(v => v.GetProperty("properties").GetProperty("addressPrefixes").EnumerateArray());

                foreach (var ip in ipList)
                {
                    ipAddresses.Add(ip.GetString());
                }
            }

            File.WriteAllText(@"azure-us.txt", string.Join(Environment.NewLine, ipAddresses));
            File.WriteAllText(@"azure-us-v4.txt", string.Join(Environment.NewLine, ipAddresses.Where(ip => !ip.Contains(":"))));
            File.WriteAllText(@"azure-us-v6.txt", string.Join(Environment.NewLine, ipAddresses.Where(ip => ip.Contains(":"))));
            File.WriteAllText(@"azure-us.csv", string.Join(",", ipAddresses));
            File.WriteAllText(@"azure-us-v4.csv", string.Join(",", ipAddresses.Where(ip => !ip.Contains(":"))));
            File.WriteAllText(@"azure-us-v6.csv", string.Join(",", ipAddresses.Where(ip => ip.Contains(":"))));

            File.Delete(fileName);
        }
    }
}
