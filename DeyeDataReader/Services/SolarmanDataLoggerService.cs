using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using DeyeDataReader.Configuration;
using DeyeDataReader.Models;

namespace DeyeDataReader.Services
{
    public class SolarmanDataLoggerService
    {
        private double _totalBatteryCharge = 0;
        private double _totalBatteryDischarge = 0;
        private double _totalEnergyBought = 0;
        private double _totalEnergySold = 0;
        private double _totalProduction = 0;

        private readonly InverterConfig _config;
        private readonly List<RegisterMapping> _registerMappings;
        private readonly string[] _signedRegisters = {
            "0x00A7", "0x00A8", "0x00A9", "0x00AA", "0x00AB",
            "0x00AD", "0x00AE", "0x00AF", "0x00A4", "0x00A5",
            "0x00BE", "0x00BF", "0x005A", "0x005B", "0x00B6", "0x013E"
        };

        public SolarmanDataLoggerService()
        {
            _config = new();
            _registerMappings = LoadRegisterMappings();
        }

        public SolarmanDataLoggerService(InverterConfig config)
        {
            _config = config;
            _registerMappings = LoadRegisterMappings();
        }

        private List<RegisterMapping> LoadRegisterMappings()
        {
            try
            {
                var jsonPath = Directory.GetCurrentDirectory();

                jsonPath = Path.Combine(jsonPath, _config.InverterRegistersMapFile);

                if (jsonPath == null || !File.Exists(jsonPath))
                {
                    Console.WriteLine("DEYEMap.xml not found in any expected location");
                    return new List<RegisterMapping>();
                }

                var jsonContent = File.ReadAllText(jsonPath);
                var root = JsonSerializer.Deserialize<RegisterMappingRoot[]>(jsonContent);
                var mappings = root?.SelectMany(r => r.Items).ToList() ?? new List<RegisterMapping>();
                Console.WriteLine($"Loaded {mappings.Count} register mappings from {jsonPath}");
                return mappings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading register mappings: {ex.Message}");
                return new List<RegisterMapping>();
            }
        }

        public async Task<InverterDataDto?> GetInverterDataAsync()
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_config.InverterIp, _config.InverterPort);

                if (_config.Verbose)
                    Console.WriteLine("Connected to logger successfully!");

                var inverterData = new InverterDataDto();

                for (int i = 0; i < _config.RegisterStarts.Length; i++)
                {
                    var start = _config.RegisterStarts[i];
                    var end = _config.RegisterEnds[i];
                    var data = await ReadRegisterRangeAsync(client, start, end);
                    if (data != null)
                    {
                        ParseRegisterData(data, start, end, inverterData);
                    }
                }

                return inverterData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to inverter: {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]?> ReadRegisterRangeAsync(TcpClient client, int start, int end)
        {
            try
            {
                var frame = BuildRequestFrame(start, end);

                if (_config.Verbose)
                    Console.WriteLine($"Sent data: {BitConverter.ToString(frame)}");

                await client.GetStream().WriteAsync(frame, 0, frame.Length);

                var buffer = new byte[1024];
                var bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);

                if (_config.Verbose)
                    Console.WriteLine($"Received data: {BitConverter.ToString(buffer, 0, bytesRead)}");

                return buffer.Take(bytesRead).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading register range: {ex.Message}");
                return null;
            }
        }

        private byte[] BuildRequestFrame(int start, int end)
        {
            var frame = new List<byte>
            {
                // Start byte
                0xA5,

                // Length (23 bytes)
                0x17,
                0x00,

                // Control code
                0x10,
                0x45,

                // Serial (0x0000)
                0x00,
                0x00
            };

            // Inverter serial number (little endian)
            var snBytes = BitConverter.GetBytes((uint)_config.InverterSerialNumber);
            frame.AddRange(snBytes);

            // Data field
            frame.AddRange(new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            // Business field (Modbus request)
            var registerCount = end - start + 1;
            frame.Add(0x01); // Device address
            frame.Add(0x03); // Function code (read holding registers)
            frame.Add((byte)(start >> 8)); // Start address high
            frame.Add((byte)(start & 0xFF)); // Start address low
            frame.Add((byte)(registerCount >> 8)); // Register count high
            frame.Add((byte)(registerCount & 0xFF)); // Register count low

            // Calculate CRC16-Modbus
            var businessField = frame.Skip(frame.Count - 6).ToArray();
            var crc = CalculateCrc16Modbus(businessField);
            frame.Add((byte)(crc & 0xFF)); // CRC low
            frame.Add((byte)(crc >> 8)); // CRC high

            // Checksum
            frame.Add(0x00);

            // End code
            frame.Add(0x15);

            // Calculate and set checksum
            var checksum = 0;
            for (int i = 1; i < frame.Count - 2; i++)
            {
                checksum += frame[i];
            }
            frame[frame.Count - 2] = (byte)(checksum & 0xFF);

            return frame.ToArray();
        }

        private ushort CalculateCrc16Modbus(byte[] data)
        {
            ushort crc = 0xFFFF;

            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        private void ParseRegisterData(byte[] data, int start, int end, InverterDataDto inverterData)
        {
            var registerCount = end - start + 1;

            if (_config.Verbose)
                Console.WriteLine($"Parsing {registerCount} registers from 0x{start:X4} to 0x{end:X4}, data length: {data.Length}");

            for (int i = 0; i < registerCount; i++)
            {
                var registerAddress = start + i;
                var hexAddress = $"0x{registerAddress:X4}";

                // Data starts at position 28 in the response (after Solarman + Modbus headers), each register is 2 bytes
                var dataStart = 28 + (i * 2);
                if (dataStart + 2 > data.Length)
                {
                    if (_config.Verbose)
                        Console.WriteLine($"Skipping register {hexAddress} - data position {dataStart} exceeds data length {data.Length}");
                    continue;
                }

                // Extract 2 bytes for the value (big endian), skip 2 bytes padding
                var value = (data[dataStart] << 8) | data[dataStart + 1];

                // Check if this is a signed register
                if (_signedRegisters.Contains(hexAddress))
                {
                    value = ConvertToSigned16((ushort)value);
                }

                // Apply register mapping
                ApplyRegisterMapping(hexAddress, value, inverterData);
            }
        }

        private short ConvertToSigned16(ushort value)
        {
            if (value > 32767)
                return (short)(value - 65536);
            return (short)value;
        }

        private void ApplyRegisterMapping(string hexAddress, int value, InverterDataDto inverterData)
        {
            var mapping = _registerMappings.FirstOrDefault(m => m.Registers.Contains(hexAddress));

            if (mapping == null)
            {
                if (_config.Verbose)
                    Console.WriteLine($"No mapping found for register {hexAddress}");
                return;
            }

            // Handle battery charge/discharge registers for hybrid inverters
            // Note: For hybrid inverters, these are SEPARATE 16-bit registers (not 32-bit combined)
            // Low word and high word are stored separately, each with 0.1kWh units
            if (hexAddress == "0x0048")
            {
                _totalBatteryCharge += value * 0.1;
                inverterData.TotalBatteryCharge = _totalBatteryCharge;
            }
            else if (hexAddress == "0x0049")
            {
                _totalBatteryCharge += value * 0.1 * 65536;
                inverterData.TotalBatteryCharge = _totalBatteryCharge;
            }
            else if (hexAddress == "0x004A")
            {
                _totalBatteryDischarge += value * 0.1;
                inverterData.TotalBatteryDischarge = _totalBatteryDischarge;
            }
            else if (hexAddress == "0x004B")
            {
                _totalBatteryDischarge += value * 0.1 * 65536;
                inverterData.TotalBatteryDischarge = _totalBatteryDischarge;
            }
            else if (hexAddress == "0x004E")
            {
                _totalEnergyBought += value * 0.1;
                inverterData.TotalEnergyBought = _totalEnergyBought;
            }
            else if (hexAddress == "0x004F")
            {
                // For hybrid inverters with low total energy values, high word may contain invalid data
                // Only add contribution if value is reasonable (< 10, representing < 655,360 kWh total)
                if (value < 10)
                {
                    _totalEnergyBought += value * 0.1 * 65536;
                }
                else if (_config.Verbose)
                {
                    Console.WriteLine($"  DEBUG 0x004F: raw value = {value} ignored (likely invalid data for hybrid inverter)");
                }
                inverterData.TotalEnergyBought = _totalEnergyBought;
            }
            else if (hexAddress == "0x0051")
            {
                _totalEnergySold += value * 0.1;
                inverterData.TotalEnergySold = _totalEnergySold;
            }
            else if (hexAddress == "0x0052")
            {
                // For hybrid inverters with low total energy values, high word may contain invalid data
                if (value < 10)
                {
                    _totalEnergySold += value * 0.1 * 65536;
                }
                else if (_config.Verbose)
                {
                    Console.WriteLine($"  DEBUG 0x0052: raw value = {value} ignored (likely invalid data for hybrid inverter)");
                }
                inverterData.TotalEnergySold = _totalEnergySold;
            }
            else if (hexAddress == "0x0060")
            {
                _totalProduction += value * 0.1;
                inverterData.TotalProduction = _totalProduction;
            }
            else if (hexAddress == "0x0061")
            {
                // For hybrid inverters with low total energy values, high word may contain invalid data
                if (value < 10)
                {
                    _totalProduction += value * 0.1 * 65536;
                }
                else if (_config.Verbose)
                {
                    Console.WriteLine($"  DEBUG 0x0061: raw value = {value} ignored (likely invalid data for hybrid inverter)");
                }
                inverterData.TotalProduction = _totalProduction;
            }
            else
            {
                // For all other registers, apply ratio and add to JSON output
                var scaledValue = value * mapping.Ratio;

                // Custom handling for temperatures
                if (mapping.Unit == "°C")
                {
                    if (scaledValue >= 100)
                    {
                        scaledValue -= 100;
                    }
                    else
                    {
                        scaledValue *= -1;
                    }
                }

                var prop = inverterData.GetType()
                    .GetProperty(mapping.TitleEN, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop == null)
                    throw new ArgumentException($"Property '{mapping.TitleEN}' not found");

                if (!prop.CanWrite)
                    throw new InvalidOperationException($"Property '{mapping.TitleEN}' is read-only.");

                Type propType = prop.PropertyType;

                object convertedValue;

                if (propType == typeof(int))
                {
                    convertedValue = Convert.ToInt32(scaledValue);
                }
                else if (propType == typeof(double))
                {
                    convertedValue = scaledValue;
                }
                else
                {
                    throw new NotSupportedException(
                        $"Property '{mapping.TitleEN}' is not an int or double; it is {propType.Name}");
                }

                prop.SetValue(inverterData, convertedValue);
            }

            if (_config.Verbose)
            {
                var displayValue = value * mapping.Ratio;

                Console.WriteLine($"{hexAddress} - {mapping.TitleEN}: {displayValue}{mapping.Unit}");
            }
        }
    }
}
