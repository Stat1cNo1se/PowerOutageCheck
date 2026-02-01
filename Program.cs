using System.Diagnostics;
using System.Text.Json;
using System.Text;

public class Entry
{
    public int ID {get; set;}
    public float duration {get; set;}
    public string? timeSpan {get; set;}
}

internal class Program
{
    public static DateTime startTimeSpan;
    public static DateTime endTimeSpan;
    public static DateTime lastDate = DateTime.Now.Date;
    public static Stopwatch sw = new Stopwatch();

    public static string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Entries");
    public static string filePath = string.Empty;

    public static bool markedDown = true;
    public static int id = 0;

    static void Main()
    {
        Console.WriteLine($"Date: {DateTime.Now}");

        var startTime = TimeSpan.Zero;
        var endTime = TimeSpan.FromSeconds(1);

        var timer = new Timer((e) =>
        {
            AddEntry(); 
        }, null, startTime, endTime);

        Console.WriteLine("Program Running. Press any key to exit...");
        Console.WriteLine();
        Console.Read();
    }

    public static void AddEntry()
    {
        Entry entry = new Entry();
        bool connected = CheckPower();

        if (DateTime.Now.Date > lastDate && markedDown == false)
        {
            sw.Stop();
    
            endTimeSpan = lastDate.AddDays(1).AddSeconds(-1);
    
            entry.ID = id;
            entry.duration = (float)(endTimeSpan - startTimeSpan).TotalSeconds;
            entry.timeSpan = $"{startTimeSpan:hh:mm:ss tt} - {endTimeSpan:hh:mm:ss tt}";
    
            var sbMidnight = new StringBuilder();
            var properties = typeof(Entry).GetProperties();
    
            string previousFile = Path.Combine(folderPath, $"{lastDate:dd-MM-yyyy}.csv");
    
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
    
            if (!File.Exists(previousFile))
            {
                using (var createFile = File.Create(previousFile)) { }
                sbMidnight.AppendLine(string.Join(",", properties.Select(p => p.Name)));
            }
    
            var valuesMidnight = properties.Select(p => p.GetValue(entry)?.ToString() ?? "");
            sbMidnight.AppendLine(string.Join(",", valuesMidnight));
            File.AppendAllText(previousFile, sbMidnight.ToString());
    
            startTimeSpan = lastDate.AddDays(1);
            sw.Restart();
            id = 0;
            lastDate = DateTime.Now.Date;
        }
    
        if (DateTime.Now.Date > lastDate)
        {
            lastDate = DateTime.Now.Date;
        }

        var sb = new StringBuilder();
        var propertiesCurrent = typeof(Entry).GetProperties();

        filePath = Path.Combine(folderPath, $"{DateTime.Now:dd-MM-yyyy}.csv");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        if (!File.Exists(filePath))
        {
            using (var creatFile = File.Create(filePath)) {};
            sb.AppendLine(string.Join(",", propertiesCurrent.Select(p => p.Name)));
            File.AppendAllText(filePath, sb.ToString());
        }

        if (!connected && markedDown)
        {
            sw.Restart();
            markedDown = false;
            startTimeSpan = DateTime.Now;

            Console.WriteLine("Device on battery");
        }
        
        if (connected == true && markedDown == false)
        {
            markedDown = true;
            sw.Stop();

            endTimeSpan = DateTime.Now;

            entry.ID = id;
            entry.duration = (float)sw.Elapsed.TotalSeconds;
            entry.timeSpan = $"{startTimeSpan:hh:mm:ss tt} - {endTimeSpan:hh:mm:ss tt}";

            Console.WriteLine("-------------------------------------");
            Console.WriteLine($"ID: {id}");
            Console.WriteLine($"Duration: {(float)sw.Elapsed.TotalSeconds}");
            Console.WriteLine($"Timespan: {entry.timeSpan}");

            var values = propertiesCurrent.Select(p => p.GetValue(entry)?.ToString() ?? "");
            sb.AppendLine(string.Join(",", values));
            File.AppendAllText(filePath, sb.ToString());

            sw.Restart();
            sb.Clear();
            id++;

            Console.WriteLine("Entry Added");
        }
    }

    public static bool CheckPower()
    {
        // Path may vary
        const string acPath = "/sys/class/power_supply/ADP1/online";

        if (!File.Exists(acPath))
        {
            Console.WriteLine("AC status not found");
            return false;
        }

        try
        {
            return File.ReadAllText(acPath).Trim() == "1";
        }
        catch (IOException)
        {
            Console.WriteLine("Failed to read AC status");
            return false;
        }
    }
}