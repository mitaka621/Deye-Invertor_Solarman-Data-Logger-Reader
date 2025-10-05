using System.Text.Json.Serialization;

namespace DeyeDataReader.Models
{
    public class InverterDataDto
    {
        // Daily Energy Data
        [JsonPropertyName("Daily Battery Charge (kWh)")]
        public double DailyBatteryCharge { get; set; }

        [JsonPropertyName("Daily Battery Discharge (kWh)")]
        public double DailyBatteryDischarge { get; set; }

        [JsonPropertyName("Total Battery Charge (kWh)")]
        public double TotalBatteryCharge { get; set; }

        [JsonPropertyName("Total Battery Discharge (kWh)")]
        public double TotalBatteryDischarge { get; set; }

        [JsonPropertyName("Daily Energy Bought (kWh)")]
        public double DailyEnergyBought { get; set; }

        [JsonPropertyName("Daily Energy Sold (kWh)")]
        public double DailyEnergySold { get; set; }

        [JsonPropertyName("Total Energy Bought (kWh)")]
        public double TotalEnergyBought { get; set; }

        [JsonPropertyName("Total Energy Sold (kWh)")]
        public double TotalEnergySold { get; set; }

        [JsonPropertyName("Total Production (kWh)")]
        public double TotalProduction { get; set; }

        [JsonPropertyName("Daily Production (kWh)")]
        public double DailyProduction { get; set; }

        // Temperature Data
        [JsonPropertyName("DC Temperature (°C)")]
        public double DCTemperature { get; set; }

        [JsonPropertyName("AC Temperature (°C)")]
        public double ACTemperature { get; set; }

        [JsonPropertyName("Battery Temperature (°C)")]
        public double BatteryTemperature { get; set; }

        // PV Data
        [JsonPropertyName("PV1 Voltage (V)")]
        public double PV1Voltage { get; set; }

        [JsonPropertyName("PV1 Current (A)")]
        public double PV1Current { get; set; }

        [JsonPropertyName("PV2 Voltage (V)")]
        public double PV2Voltage { get; set; }

        [JsonPropertyName("PV2 Current (A)")]
        public double PV2Current { get; set; }

        [JsonPropertyName("PV1 Power (W)")]
        public int PV1Power { get; set; }

        [JsonPropertyName("PV2 Power (W)")]
        public int PV2Power { get; set; }

        // Grid Data
        [JsonPropertyName("Grid Voltage L1 (V)")]
        public double GridVoltageL1 { get; set; }

        [JsonPropertyName("Grid Voltage L2 (V)")]
        public double GridVoltageL2 { get; set; }

        [JsonPropertyName("Grid Current L1 (A)")]
        public double GridCurrentL1 { get; set; }

        [JsonPropertyName("Grid Current L2 (A)")]
        public double GridCurrentL2 { get; set; }

        [JsonPropertyName("Grid Frequency (Hz)")]
        public double GridFrequency { get; set; }

        // Power Data
        [JsonPropertyName("Micro-inverter Power (W)")]
        public int MicroInverterPower { get; set; }

        [JsonPropertyName("Internal CT L1 Power (W)")]
        public int InternalCTL1Power { get; set; }

        [JsonPropertyName("Internal CT L2 Power (W)")]
        public int InternalCTL2Power { get; set; }

        [JsonPropertyName("Total Grid Power (W)")]
        public int TotalGridPower { get; set; }

        [JsonPropertyName("External CT L1 Power (W)")]
        public int ExternalCTL1Power { get; set; }

        [JsonPropertyName("External CT L2 Power (W)")]
        public int ExternalCTL2Power { get; set; }

        [JsonPropertyName("Inverter L1 Power (W)")]
        public int InverterL1Power { get; set; }

        [JsonPropertyName("Inverter L2 Power (W)")]
        public int InverterL2Power { get; set; }

        [JsonPropertyName("Total Power (W)")]
        public int TotalPower { get; set; }

        // Battery Data
        [JsonPropertyName("Battery Voltage (V)")]
        public double BatteryVoltage { get; set; }

        [JsonPropertyName("Battery SOC (%)")]
        public int BatterySOC { get; set; }

        [JsonPropertyName("Battery Status")]
        public int BatteryStatus { get; set; }

        [JsonPropertyName("Battery Power (W)")]
        public int BatteryPower { get; set; }

        [JsonPropertyName("Battery Current (A)")]
        public double BatteryCurrent { get; set; }

        [JsonPropertyName("Battery Capacity (Ah)")]
        public int BatteryCapacity { get; set; }

        // BMS Data
        [JsonPropertyName("BMS1 Charging Voltage (V)")]
        public double BMS1ChargingVoltage { get; set; }

        [JsonPropertyName("BMS1 Discharge Voltage (V)")]
        public double BMS1DischargeVoltage { get; set; }

        [JsonPropertyName("BMS1 Charge Current Limit (A)")]
        public int BMS1ChargeCurrentLimit { get; set; }

        [JsonPropertyName("BMS1 Discharge Current Limit (A)")]
        public int BMS1DischargeCurrentLimit { get; set; }

        [JsonPropertyName("BMS1 SOC (%)")]
        public int BMS1SOC { get; set; }

        [JsonPropertyName("BMS1 Voltage (V)")]
        public double BMS1Voltage { get; set; }

        [JsonPropertyName("BMS1 Current (A)")]
        public int BMS1Current { get; set; }

        [JsonPropertyName("BMS1 Temperature (°C)")]
        public double BMS1Temperature { get; set; }

        // Settings Data
        [JsonPropertyName("Active Power Regulation (%)")]
        public double ActivePowerRegulation { get; set; }
    }
}