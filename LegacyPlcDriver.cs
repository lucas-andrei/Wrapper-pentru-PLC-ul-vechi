// ============================================================================
//  LegacyPlcDriver – driver livrat de furnizorul PLC-ului (anul 2009).
//  NU MODIFICAȚI ACEST FIȘIER. Considerați că este o bibliotecă compilată
//  pe care nu o puteți schimba.
//
//  Instalația: un rezervor de 2,5 m înălțime, cu
//    - vană de intrare (alimentare),
//    - vană de ieșire + pompă de evacuare,
//    - senzor de nivel (ADC pe 12 biți: 0..4095 = 0..2,5 m),
//    - senzor de temperatură (valoare brută = (°C + 50) * 10).
//
//  Coduri de retur: 0 = OK, -1 = port inexistent, -2 = port ocupat,
//                   -3 = neconectat, -4 = adresă invalidă, -5 = valoare invalidă
//
//  SIMULARE: fiecare apel ReadRegister avansează timpul instalației cu 1 secundă.
// ============================================================================
public class LegacyPlcDriver
{
  public const int REG_LEVEL_RAW = 40001;   // citire: 0..4095
  public const int REG_TEMP_RAW = 40002;    // citire: (°C + 50) * 10
  public const int REG_VALVE_IN = 40010;    // scriere/citire: 1 = deschisă, 0 = închisă
  public const int REG_VALVE_OUT = 40011;   // scriere/citire: 1 = deschisă, 0 = închisă
  public const int REG_PUMP = 40012;        // scriere/citire: 1 = pornită, 0 = oprită
  public const int REG_STATUS = 40020;      // citire: bit0 = preaplin, bit1 = defect pompă
  public const int REG_RESET = 40030;       // scriere 1 = resetare defecte

  private bool connected;
  private double levelPercent = 35.0;
  private double temperature = 21.5;
  private int valveIn, valveOut, pump;
  private int secondsPumpAgainstClosedValve;
  private bool overflow, pumpFault;
  private int tick;

  public int Connect(string port)
  {
    if (port != "COM3") return -1;
    if (connected) return -2;
    connected = true;
    return 0;
  }

  public void Disconnect()
  {
    connected = false;
  }

  public int ReadRegister(int address, out int value)
  {
    value = 0;
    if (!connected) return -3;
    Simulate();

    switch (address)
    {
      case REG_LEVEL_RAW: value = (int)Math.Round(levelPercent / 100.0 * 4095); return 0;
      case REG_TEMP_RAW: value = (int)Math.Round((temperature + 50) * 10); return 0;
      case REG_VALVE_IN: value = valveIn; return 0;
      case REG_VALVE_OUT: value = valveOut; return 0;
      case REG_PUMP: value = pump; return 0;
      case REG_STATUS: value = (overflow ? 1 : 0) | (pumpFault ? 2 : 0); return 0;
      default: return -4;
    }
  }

  public int WriteRegister(int address, int value)
  {
    if (!connected) return -3;
    if (value != 0 && value != 1) return -5;

    switch (address)
    {
      case REG_VALVE_IN: valveIn = value; return 0;
      case REG_VALVE_OUT: valveOut = value; return 0;
      case REG_PUMP:
        if (pumpFault && value == 1) return -5;   // pompa defectă nu pornește
        pump = value;
        return 0;
      case REG_RESET:
        if (value == 1) { pumpFault = false; overflow = levelPercent >= 98; secondsPumpAgainstClosedValve = 0; }
        return 0;
      default: return -4;
    }
  }

  private void Simulate()
  {
    tick++;
    if (valveIn == 1) levelPercent += 0.8;
    if (pump == 1 && valveOut == 1) levelPercent -= 1.2;
    levelPercent = Math.Clamp(levelPercent, 0, 100);

    if (pump == 1 && valveOut == 0)
    {
      secondsPumpAgainstClosedValve++;
      if (secondsPumpAgainstClosedValve >= 3) { pumpFault = true; pump = 0; }
    }
    else
    {
      secondsPumpAgainstClosedValve = 0;
    }

    if (pump == 1 && levelPercent <= 0) { pumpFault = true; pump = 0; }   // mers în gol
    if (levelPercent >= 98) overflow = true;

    temperature = 21.5 + 0.3 * Math.Sin(tick / 10.0);
  }
}
