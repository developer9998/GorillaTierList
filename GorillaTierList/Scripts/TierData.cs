using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GorillaTierList.Models;
using WebSocketSharp;

namespace GorillaTierList.Scripts
{
    public class TierData
    {
        public static DropperData CurrentData { get; set; }
        public static string FilePath { get; set; } 

        public static void LoadData()
        {
            CurrentData = new DropperData();
            if (FilePath.IsNullOrEmpty())
            {
                FilePath = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "TierConfig.txt");
                if (!File.Exists(FilePath)) SaveData();
            }

            List<string> tempString = File.ReadAllLines(FilePath).ToList<string>();
            if (tempString.Count < 2 || tempString.Count > 30) return; // leave it at that

            CurrentData.DropperNames.Clear();
            CurrentData.DropperName = tempString[0];
            if (CurrentData.DropperName.Length >= 22) CurrentData.DropperName.Substring(0, 21);

            for (int i = 0; i < tempString.Count; i++)
            {
                if (i != 0 && i <= 31)
                {
                    string name = tempString[i];
                    if (name.Length >= 61) name.Substring(0, 60);
                    CurrentData.DropperNames.Add(name);
                }
            }
        }

        public static void SaveData()
        {
            string OutputtedList = string.Join(Environment.NewLine, CurrentData.DropperNames);
            string OutputtedText = $"{CurrentData.DropperName}\n{OutputtedList}";
            File.WriteAllText(FilePath, OutputtedText);
        }
    }
}
