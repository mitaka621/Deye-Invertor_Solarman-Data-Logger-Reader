namespace DeyeDataReader.Configuration
{
    public class InverterConfig
    {
        public string InverterIp { get; set; } = "10.98.128.77";
        public int InverterPort { get; set; } = 8899;
        public long InverterSerialNumber { get; set; } = 3119026917;

        public string InverterRegistersMapFile { get; set; } = "DEYE_SUN_SG01LP1_EU_Map.json";

        public int[] RegisterStarts { get; set; } = [0x0046, 0x00C1, 0x0100];
        public int[] RegisterEnds { get; set; } = [0x00C0, 0x00CC, 0x013F];

        public bool Verbose { get; set; } = false;
    }
}