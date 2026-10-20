TankStation tank = new TankStation();

try
{
    // 1. Afișare stare inițială
    Console.WriteLine("\n=== 1. Stare inițială ===");
    tank.AfisareStare();

    // 2. Umplere până la 80%
    Console.WriteLine("\n=== 2. Umplere până la 80% ===");
    tank.umplereTinta(80);
    tank.AfisareStare();

    // 3. Golire până la 20%
    Console.WriteLine("\n=== 3. Golire până la 20% ===");
    tank.golireTinta(20);
    tank.AfisareStare();

    // 4. Verificare alarme
    Console.WriteLine("\n=== 4. Verificare alarme ===");
    tank.AfisareStare();
    tank.ResetareDefecte();
}
catch (Exception ex)
{
    Console.WriteLine($"Eroare: {ex.Message}");
}
finally
{
    tank.Disconnect();
}