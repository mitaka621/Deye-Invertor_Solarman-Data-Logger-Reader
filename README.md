# Deye Inverter Data Reader

A C# .NET 9.0 application for reading real-time data from Deye/Solarman hybrid inverters via the Solarman data logger. This application communicates directly with the data logger using the Modbus TCP protocol and returns structured inverter data through a DTO for easy integration into custom applications.

## Features

- Real-time data retrieval from Deye hybrid inverters (tested with SUN-8k-SG01LP1-EU)
- Support for multiple register ranges
- Configurable verbose logging
- Returns structured data via `InverterDataDto` class
- Easy integration into custom C# applications
- Supports battery, solar, grid, and BMS data

## Requirements

- .NET 9.0 SDK or later
- Network access to the Solarman data logger
- Solarman data logger connected to your Deye inverter

## Configuration

### Configuration Location

The application configuration can be set up in two places:

1. **Default Configuration**: `DeyeDataReader/Configuration/InverterConfig.cs`
2. **Runtime Configuration**: `DeyeDataReader/Program.cs` (overrides defaults)

### Required Configuration Parameters

#### 1. Inverter IP Address

The local IP address of your **Solarman data logger**.

**How to find it:**

- Check your router's DHCP client list for the Solarman logger device
- The logger typically has a hostname like "SOLARMAN_XXXXXX"
- Note the local IP address (e.g., `10.98.128.77`)

```csharp
InverterIp = "10.98.128.77"
```

#### 2. Inverter Port

The Modbus TCP port used by the Solarman logger. Default is `8899`.

```csharp
InverterPort = 8899
```

#### 3. Inverter Serial Number

The serial number of the **Solarman data logger** (not the inverter serial number).

**How to find it:**

1. Open a web browser and navigate to the data logger's IP address (e.g., `http://10.98.128.77`)
2. Login with default credentials:
   - Username: `admin`
   - Password: `admin`
3. Navigate to the configuration page
4. Locate the "Logger Serial Number" field
5. Copy the serial number (e.g., `3119026917`)

```csharp
InverterSerialNumber = 3119026917
```

#### 4. Register Ranges

The Modbus register ranges to read from the inverter. These are pre-configured for Deye hybrid inverters:

```csharp
RegisterStarts = [0x0046, 0x00C1, 0x0100]
RegisterEnds = [0x00C0, 0x00CC, 0x013F]
```

**Note:** These ranges are optimized to work within the Solarman logger's response size limitations. Do not modify unless you know what you're doing.

#### 5. Verbose Mode

Enable detailed logging to see register-by-register data parsing:

```csharp
Verbose = true  // Enable verbose logging
Verbose = false // Disable verbose logging (production mode)
```

When enabled, verbose mode displays:

- Connection status
- Sent/received Modbus data
- Register addresses and parsed values
- Mapping information

### Example Configuration (Program.cs)

```csharp
var config = new InverterConfig
{
    InverterIp = "10.98.128.77",
    InverterPort = 8899,
    InverterSerialNumber = 3119026917,
    RegisterStarts = [0x0046, 0x00C1, 0x0100],
    RegisterEnds = [0x00C0, 0x00CC, 0x013F],
    Verbose = false
};
```

## Building and Running

### Build the Application

```bash
cd "C:\Users\Dimitar\Desktop\Sofar_LSW3-main\C# solamrnan logger data reader\DeyeDataReader\DeyeDataReader"
dotnet build
```

### Run the Application

```bash
dotnet run
```

### Expected Output

The application will connect to the data logger, retrieve inverter data, and output it in JSON format:

```json
{
  "Daily Battery Charge (kWh)": 0,
  "Daily Battery Discharge (kWh)": 0.1,
  "Total Battery Charge (kWh)": 10.7,
  "Total Battery Discharge (kWh)": 7.4,
  "Battery Voltage (V)": 52.81,
  "Battery SOC (%)": 97,
  "Battery Capacity (Ah)": 100,
  "PV1 Power (W)": 0,
  "Grid Frequency (Hz)": 50,
  ...
}
```

## Available Data Points

The following table lists all properties available in the `InverterDataDto` class:

| Category                            | Property                  | Description                                                   | Unit |
| ----------------------------------- | ------------------------- | ------------------------------------------------------------- | ---- |
| **Solar Production**                | PV1Voltage                | Photovoltaic string 1 voltage                                 | V    |
|                                     | PV1Current                | Photovoltaic string 1 current                                 | A    |
|                                     | PV1Power                  | Photovoltaic string 1 power output                            | W    |
|                                     | PV2Voltage                | Photovoltaic string 2 voltage                                 | V    |
|                                     | PV2Current                | Photovoltaic string 2 current                                 | A    |
|                                     | PV2Power                  | Photovoltaic string 2 power output                            | W    |
|                                     | DailyProduction           | Total energy produced today                                   | kWh  |
|                                     | TotalProduction           | Cumulative energy produced since installation                 | kWh  |
|                                     | MicroInverterPower        | Micro-inverter power output                                   | W    |
| **Battery**                         | BatteryVoltage            | Battery pack voltage                                          | V    |
|                                     | BatteryCurrent            | Battery charge/discharge current                              | A    |
|                                     | BatteryPower              | Battery charge/discharge power (positive = charging)          | W    |
|                                     | BatterySOC                | Battery state of charge                                       | %    |
|                                     | BatteryCapacity           | Battery capacity                                              | Ah   |
|                                     | BatteryStatus             | Battery operational status code                               | -    |
|                                     | BatteryTemperature        | Battery temperature                                           | °C   |
|                                     | DailyBatteryCharge        | Energy charged to battery today                               | kWh  |
|                                     | DailyBatteryDischarge     | Energy discharged from battery today                          | kWh  |
|                                     | TotalBatteryCharge        | Cumulative energy charged to battery                          | kWh  |
|                                     | TotalBatteryDischarge     | Cumulative energy discharged from battery                     | kWh  |
| **Grid**                            | GridVoltageL1             | Grid voltage phase L1                                         | V    |
|                                     | GridVoltageL2             | Grid voltage phase L2                                         | V    |
|                                     | GridCurrentL1             | Grid current phase L1                                         | A    |
|                                     | GridCurrentL2             | Grid current phase L2                                         | A    |
|                                     | GridFrequency             | Grid frequency                                                | Hz   |
|                                     | TotalGridPower            | Total grid power (positive = importing, negative = exporting) | W    |
|                                     | InternalCTL1Power         | Internal current transformer L1 power reading                 | W    |
|                                     | InternalCTL2Power         | Internal current transformer L2 power reading                 | W    |
|                                     | ExternalCTL1Power         | External current transformer L1 power reading                 | W    |
|                                     | ExternalCTL2Power         | External current transformer L2 power reading                 | W    |
|                                     | InverterL1Power           | Inverter output power phase L1                                | W    |
|                                     | InverterL2Power           | Inverter output power phase L2                                | W    |
|                                     | TotalPower                | Total inverter output power                                   | W    |
|                                     | DailyEnergyBought         | Energy purchased from grid today                              | kWh  |
|                                     | DailyEnergySold           | Energy exported to grid today                                 | kWh  |
|                                     | TotalEnergyBought         | Cumulative energy purchased from grid                         | kWh  |
|                                     | TotalEnergySold           | Cumulative energy exported to grid                            | kWh  |
| **BMS (Battery Management System)** | BMS1ChargingVoltage       | BMS maximum charging voltage limit                            | V    |
|                                     | BMS1DischargeVoltage      | BMS minimum discharge voltage limit                           | V    |
|                                     | BMS1ChargeCurrentLimit    | BMS maximum charge current limit                              | A    |
|                                     | BMS1DischargeCurrentLimit | BMS maximum discharge current limit                           | A    |
|                                     | BMS1SOC                   | BMS reported state of charge                                  | %    |
|                                     | BMS1Voltage               | BMS reported battery voltage                                  | V    |
|                                     | BMS1Current               | BMS reported battery current                                  | A    |
|                                     | BMS1Temperature           | BMS reported battery temperature                              | °C   |
| **Temperature**                     | DCTemperature             | DC side temperature (inverter)                                | °C   |
|                                     | ACTemperature             | AC side temperature (inverter)                                | °C   |
| **Settings**                        | ActivePowerRegulation     | Active power regulation setting                               | %    |
