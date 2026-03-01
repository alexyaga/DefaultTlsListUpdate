using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var profiles = await GetTlsProfilesAsync();

        Console.WriteLine("Available TLS client identifiers:\n");
        if ( profiles.Count == 0)
        {
            Console.WriteLine("No profiles found.");
            return;
        }

        string FileName = "Preset_tls_profiles.txt";
        if (File.Exists(FileName))
        {
            File.Delete(FileName);
        }
        foreach (var profile in profiles)
        {
            File.AppendAllText(FileName, profile + Environment.NewLine);
            Console.WriteLine(profile);
        }
    }

    public static async Task<List<string>> GetTlsProfilesAsync()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("TlsClientProfileParser");

        var url = "https://raw.githubusercontent.com/bogdanfinn/tls-client/master/profiles/profiles.go";

        var content = await http.GetStringAsync(url);

        return ParseMappedTlsClients(content);
    }

    private static List<string> ParseMappedTlsClients(string source)
    {
        var result = new List<string>();

        // Ищем блок MappedTLSClients
        var mapMatch = Regex.Match(
            source,
            @"var\s+MappedTLSClients\s*=\s*map\[string\]ClientProfile\s*{(?<body>[\s\S]*?)}",
            RegexOptions.Multiline);

        if (!mapMatch.Success)
            throw new Exception("MappedTLSClients not found.");

        var body = mapMatch.Groups["body"].Value;

        // Ищем ключи вида "chrome_120":
        var keyMatches = Regex.Matches(body, @"""([^""]+)""\s*:");

        foreach (Match match in keyMatches)
            result.Add(match.Groups[1].Value);

        result.Sort();
        return result;
    }
}