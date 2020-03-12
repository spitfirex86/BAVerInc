using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BAVerInc
{
    class Program
    {
        static List<string> Params = new List<string>();
        static List<string> Files = new List<string>();

        static void Main(string[] args)
        {
            ParseArgs(args);

            if (Files.Count <= 0 || Params.Contains("?"))
            {
                DisplayHelp();
                return;
            }

            foreach (string file in Files)
            {
                ProcessFile(file);
            }
        }

        static void ParseArgs(string[] args)
        {
            foreach (string s in args)
            {
                if (s.StartsWith("/")) Params.Add(s.TrimStart('/').ToLower());
                else Files.Add(s);
            }
        }

        static void ProcessFile(string file)
        {
            if (!File.Exists(file)) return;

            List<string> lines = File.ReadAllLines(file).ToList();
            List<string> asmLines = lines.FindAll(s => s.StartsWith("[assembly:"));

            string searchFor = Params.Contains("a") ? "AssemblyVersion" : "AssemblyFileVersion";
            int asmIndex = asmLines.FindIndex(s => s.Contains(searchFor));

            if (asmIndex < 0) return;

            int index = lines.IndexOf(asmLines[asmIndex]);
            string line = lines[index];
            string[] parts = line.Split(new[] {'"'}, 3);

            if (parts.Length != 3) return;

            string[] numbers = parts[1].Split(new[] {'.'}, 4);

            if (numbers.Length != 4) return;

            if (Params.Contains("b"))
            {
                int value = int.Parse(numbers[2]) + 1;
                numbers[2] = value.ToString();
            }
            else if (Params.Contains("r"))
            {
                int value = int.Parse(numbers[3]) + 1;
                numbers[3] = value.ToString();
            }

            parts[1] = string.Join(".", numbers);
            lines[index] = string.Join("\"", parts);

            File.WriteAllLines(file, lines);
        }

        static void DisplayHelp()
        {
            string help = "Increments file version number in AssemblyInfo.cs\n\n" +
                          "BAVerInc.exe [/A] [/B | /R] filename\n\n" +
                          "/A - use AssemblyVersion instead of AssemblyFileVersion\n" +
                          "/B - increment Build\n" +
                          "/R - increment Revision\n\n" +
                          "/B or /R is required.";
            Console.WriteLine(help);
        }
    }
}
