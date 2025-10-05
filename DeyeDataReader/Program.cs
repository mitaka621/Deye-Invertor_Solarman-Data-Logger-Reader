using System.Text.Json;
using DeyeDataReader.Configuration;
using DeyeDataReader.Services;

namespace DeyeDataReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Deye Inverter Data Reader");
            Console.WriteLine("=========================");
            Console.WriteLine();

            var config = new InverterConfig
            {
                InverterIp = "10.98.128.77",
                InverterPort = 8899,
                Verbose = false,
                InverterSerialNumber = 3119026917,
                RegisterStarts = [0x0046, 0x00C1, 0x0100],
                RegisterEnds = [0x00C0, 0x00CC, 0x013F],
            };

            var solarmanService = new SolarmanDataLoggerService(config);

            try
            {
                Console.WriteLine("Connecting to inverter...");
                var inverterData = await solarmanService.GetInverterDataAsync();

                if (inverterData != null)
                {
                    Console.WriteLine("Successfully retrieved inverter data!");
                    Console.WriteLine();

                    var json = JsonSerializer.Serialize(inverterData, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    Console.WriteLine("*** JSON output:");
                    Console.WriteLine(json);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve inverter data. Check connection and configuration.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}