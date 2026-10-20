public class TankStation
{
    private int rc;
    public TankStation()
    {
        Connect("COM3");
    }
    private Dictionary<string, int> registers = new(){
    {"REG_LEVEL", 40001},
    {"REG_TEMP", 40002},
    {"REG_VALVE_IN", 40010},
    {"REG_VALVE_OUT", 40011},
    {"REG_PUMP", 40012},
    {"REG_STATUS", 40020},
    {"REG_RESET", 40030}
    };

    private LegacyPlcDriver plcDriver=new LegacyPlcDriver();
    private void Connect(string port)
    {
        rc=plcDriver.Connect(port);
        if(rc != 0)
        {
            Console.WriteLine("Eroare conectare " + rc);
        }
        else
        {
            Console.WriteLine("Conectat la portul " + port);
        }
    }

    public void Disconnect()
    {
        plcDriver.Disconnect();
        Console.WriteLine("Deconectat de la portul ");
    }

    private void ReadValue(string registerName, out int value)
    {
        rc=plcDriver.ReadRegister(registers[registerName], out value);
        if(rc==-3||rc==-4)
        {
            Console.WriteLine("Eroare citire registru sau pompa nu e conectata");
            return;
        }
        switch(registerName)
        {
            case "REG_LEVEL":
                value = (int)(value / 40.95);
                break;
            case "REG_TEMP":
                value = value / 10 - 50;
                break;
        }
    }
    public void AfisareStare()
    {
        ReadValue("REG_LEVEL", out int level);
        Console.WriteLine($"Nivel: {level} %");
        ReadValue("REG_TEMP", out int temp);
        Console.WriteLine($"Temperatura: {temp} °C");
        ReadValue("REG_STATUS", out int status);
        if ((status & 1) == 1) Console.WriteLine("ALARMĂ PREAPLIN");
        if ((status & 2) == 2) Console.WriteLine("ALARMĂ POMPĂ");
    }
    public void ResetareDefecte()
    {
        if(plcDriver.WriteRegister(registers["REG_RESET"], 1) != 0)
        {
            Console.WriteLine("Eroare resetare defecte");
        }
        else
        {
            Console.WriteLine("Defectele au fost resetate");
        }
    }

    private void SetareStare(string registerName, int value)
    {

        plcDriver.WriteRegister(registers[registerName], value);    
    }

    public void umplereTinta(int tinta)
    {

        tinta=Math.Clamp(tinta, 0, 100);
        if(tinta>95)
        {
            Console.WriteLine("Nu se poate umple rezervorul la mai mult de 95%");
            return;
        }
        SetareStare("REG_VALVE_IN", 1);
            ReadValue("REG_LEVEL", out int level);
            int pct = 0;
            while (pct < tinta)
            {
                ReadValue("REG_LEVEL", out pct);
            }
        SetareStare("REG_VALVE_IN", 0);

    }
    public void golireTinta(int tinta)
    {
        tinta=Math.Clamp(tinta, 0, 100);
        SetareStare("REG_PUMP", 1);
        SetareStare("REG_VALVE_OUT", 1);
            double level = 100;
            while (level > tinta)
            {
                ReadValue("REG_LEVEL", out int value);
                level = value;
                ReadValue("REG_STATUS", out int st);
                if ((st & 2) != 0) { Console.WriteLine("DEFECT POMPĂ!"); break; }
            }
        SetareStare("REG_PUMP", 0);
        SetareStare("REG_VALVE_OUT", 0);
        
    }

}