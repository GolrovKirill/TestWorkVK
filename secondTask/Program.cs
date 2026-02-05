using Newtonsoft.Json;
using secondTask;

class Program
{
    static async Task Main()
    {
        try
        {
            var ipList = await LoadIpAddressesFromFileAsync("IPs.txt");

            Console.WriteLine($"Загружено {ipList.Length} IP-адресов");

            var results = new List<IpData>();
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);

                var testIps = ipList.Take(20).ToArray();

                foreach (var ip in testIps)
                {
                    try
                    {
                        if (IsValidIp(ip))
                        {
                            Console.Write($"Обработка {ip}... ");
                            var data = await GetIpDataAsync(client, ip);
                            results.Add(data);
                            Console.WriteLine($"OK: {data.Country}, {data.City}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка для {ip}: {ex.Message}");
                    }

                    await Task.Delay(100);
                }
            }

            var countryStats = results
                .Where(r => !string.IsNullOrEmpty(r.Country))
                .GroupBy(r => r.Country)
                .Select(g => new
                {
                    Country = g.Key,
                    Count = g.Count(),
                    Cities = g.Where(x => !string.IsNullOrEmpty(x.City))
                             .Select(x => x.City)
                             .Distinct()
                             .ToList()
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            Console.WriteLine("\n=== СТАТИСТИКА ПО СТРАНАМ ===");
            foreach (var stat in countryStats)
            {
                Console.WriteLine($"{stat.Country} - {stat.Count}");
            }

            if (countryStats.Any())
            {
                var topCountry = countryStats.First();
                Console.WriteLine($"\n=== ГОРОДА {topCountry.Country} ===");
                foreach (var city in topCountry.Cities)
                {
                    Console.WriteLine($"• {city}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Общая ошибка: {ex.Message}");
        }

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }

    static async Task<string[]> LoadIpAddressesFromFileAsync(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                var content = await System.IO.File.ReadAllLinesAsync(filePath);
                return content.Where(IsValidIp).ToArray();
            }
        }
        catch { }
        return Array.Empty<string>();
    }

    static bool IsValidIp(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        ip = ip.Trim();

        if (ip.Contains("<") || ip.Contains(">") || ip.Contains("DOCTYPE") ||
            ip.Contains("html") || ip.Contains("script") || ip.Contains("function"))
            return false;

        var parts = ip.Split('.');
        if (parts.Length != 4)
            return false;

        foreach (var part in parts)
        {
            if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                return false;
        }

        return true;
    }

    static async Task<IpData> GetIpDataAsync(HttpClient client, string ip)
    {
        var apiUrl = $"https://ipinfo.io/{ip}/json";
        var response = await client.GetStringAsync(apiUrl);
        return JsonConvert.DeserializeObject<IpData>(response);
    }
}