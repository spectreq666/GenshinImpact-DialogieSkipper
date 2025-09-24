using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace GenshinImpactDialogueSkipper;

static class Program
{
    // --- WinAPI ---
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const int KEYEVENTF_KEYDOWN = 0x0000;
    private const int KEYEVENTF_KEYUP = 0x0002;
    private const byte VK_F = 0x46;

    private static string Status = "pause";
    private static bool IsLoggingEnabled = false;
    private static int ScreenWidth;
    private static int ScreenHeight;

    private static readonly string EnvFile = ".env";
    private static readonly Random Rnd = new();

    static void Main()
    {
        Console.Clear();
        Console.WriteLine("Welcome to Genshin Impact Dialogue Skipper by spectreq\n");

        LoadDotEnv();

        Console.WriteLine("-------------");
        Console.WriteLine("F8 to start");
        Console.WriteLine("F9 to pause");
        Console.WriteLine("F12 to quit");
        Console.WriteLine("F10 Enable logging");
        Console.WriteLine("-------------");
        
        Task.Run(KeyListener);
        
        Task.Run(MainLoop).Wait();
    }

    private static void SaveDotEnv()
    {
        File.WriteAllText(EnvFile, $"WIDTH={ScreenWidth}\nHEIGHT={ScreenHeight}\nENABLE_LOGGING={IsLoggingEnabled}");
    }
    
    private static void LoadDotEnv()
    {
        if (File.Exists(EnvFile))
        {
            foreach (var line in File.ReadAllLines(EnvFile))
            {
                if (line.StartsWith("WIDTH="))
                    ScreenWidth = int.Parse(line.Split('=')[1]);
                if (line.StartsWith("HEIGHT="))
                    ScreenHeight = int.Parse(line.Split('=')[1]);
                if (line.StartsWith("ENABLE_LOGGING="))
                    IsLoggingEnabled = bool.Parse(line.Split('=')[1]);
            }
        }

        if (ScreenWidth == 0 || ScreenHeight == 0)
        {
            ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            ScreenHeight = Screen.PrimaryScreen.Bounds.Height;

            Console.WriteLine($"Detected Resolution: {ScreenWidth}x{ScreenHeight}");
            Console.WriteLine("Is the resolution correct? (y/n)");
            var response = Console.ReadLine();
            if (response?.ToLower().StartsWith("n") == true)
            {
                Console.Write("Enter resolution width: ");
                ScreenWidth = int.Parse(Console.ReadLine() ?? "1920");
                Console.Write("Enter resolution height: ");
                ScreenHeight = int.Parse(Console.ReadLine() ?? "1080");
                Console.WriteLine($"New resolution set to {ScreenWidth}x{ScreenHeight}");
            }

            SaveDotEnv();
        }
    }

    private static bool IsGenshinActive()
    {
        IntPtr handle = GetForegroundWindow();
        var buffer = new System.Text.StringBuilder(256);
        GetWindowText(handle, buffer, buffer.Capacity);
        return buffer.ToString() == "Genshin Impact";
    }

    private static void PressF()
    {
        keybd_event(VK_F, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        Thread.Sleep(30);
        keybd_event(VK_F, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        
        if (IsLoggingEnabled)
            Console.WriteLine("Pressed F");
    }

    private static double RandomFInterval() =>
        Rnd.Next(1, 7) == 6 ? Rnd.NextDouble() * 0.02 + 0.18 : Rnd.NextDouble() * 0.06 + 0.12;

    private static bool ShouldTakeBreak() => Rnd.Next(1, 26) == 1;

    private static double TakeRandomBreak()
    {
        double duration = Rnd.NextDouble() * 5 + 3;
        
        if (IsLoggingEnabled)
            Console.WriteLine($"Taking a {duration:F1} second break...");
        
        return duration;
    }

    private static async Task MainLoop()
    {
        double lastFPress = 0;
        double nextFInterval = RandomFInterval();
        Stopwatch sw = Stopwatch.StartNew();
        double lastBreakCheck = sw.Elapsed.TotalSeconds;

        string dialogueTemplatePath = "templates/play0.jpg";

        while (true)
        {
            if (Status == "exit")
            {
                Console.WriteLine("Exiting...");
                break;
            }

            if (Status == "pause")
            {
                await Task.Delay(500);
                lastFPress = sw.Elapsed.TotalSeconds;
                continue;
            }

            if (!IsGenshinActive())
            {
                await Task.Delay(500);
                continue;
            }

            bool dialogueActive = IsDialogueActive(dialogueTemplatePath);

            if (!dialogueActive)
            {
                await Task.Delay(100);
                continue;
            }

            double now = sw.Elapsed.TotalSeconds;

            if (now - lastBreakCheck > 30)
            {
                lastBreakCheck = now;
                if (ShouldTakeBreak())
                {
                    double breakDur = TakeRandomBreak();
                    await Task.Delay(TimeSpan.FromSeconds(breakDur));
                    lastFPress = sw.Elapsed.TotalSeconds;
                    nextFInterval = RandomFInterval();
                    continue;
                }
            }

            if (now - lastFPress >= nextFInterval)
            {
                PressF();
                lastFPress = now;
                nextFInterval = RandomFInterval();
            }

            await Task.Delay(50);
        }
    }

    private static void KeyListener()
    {
        while (true)
        {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.F8)
            {
                if (!IsGenshinActive())
                {
                    Console.WriteLine("[Error] Cannot start capture.\nGenshin Impact is not launched");
                    continue;
                }
                
                Status = "run";
                Console.WriteLine("RUNNING - Auto F-key pressing enabled");
            }
            else if (key == ConsoleKey.F9)
            {
                if (Status == "pause")
                {
                    Console.WriteLine("[Error] Skipper is not running");
                    continue;
                }
                
                Status = "pause";
                Console.WriteLine("PAUSED - Auto F-key pressing disabled");
            }
            else if (key == ConsoleKey.F12)
            {
                Status = "exit";
                break;
            }
            else if (key == ConsoleKey.F10)
            {
                IsLoggingEnabled = !IsLoggingEnabled;

                Console.WriteLine(IsLoggingEnabled ? "[Logging] Enabled" : "[Logging] Disabled");
                
                SaveDotEnv();
            }
        }
    }

    static bool IsDialogueActive(string templatePath)
    {
        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"Template file not found: {templatePath}");
            return false;
        }
        
        using Bitmap screenBmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                            Screen.PrimaryScreen.Bounds.Height);
        using (Graphics g = Graphics.FromImage(screenBmp))
        {
            g.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
        }
        
        using Mat screenMat = BitmapConverter.ToMat(screenBmp);
        using Mat template = Cv2.ImRead(templatePath, ImreadModes.Color);

        if (template.Empty())
        {
            Console.WriteLine($"Template not loaded: {templatePath}");
            return false;
        }
        
        if (screenMat.Channels() == 4)
            Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.BGRA2BGR);
        
        using Mat screenGray = new Mat();
        using Mat templateGray = new Mat();
        Cv2.CvtColor(screenMat, screenGray, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(template, templateGray, ColorConversionCodes.BGR2GRAY);
        
        using Mat result = new Mat();
        Cv2.MatchTemplate(screenGray, templateGray, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);
        
        bool isActive = maxVal >= 0.9;
        
        if (IsLoggingEnabled) 
            Console.WriteLine($"Template match value: {maxVal:F3} -> DialogueActive: {isActive}");

        return isActive;
    }
}