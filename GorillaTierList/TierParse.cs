using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GorillaTierList.Models;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaTierList
{
    public class TierParse
    {
        public static TierData CurrentData { get; set; }
        private static string FilePath;

        public static async Task LoadData()
        {
            CurrentData = new TierData();
            FilePath = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "TierConfig.txt");

            // Create our data if it already doesn't exist
            if (!File.Exists(FilePath)) await SaveData();

            var asyncList = await File.ReadAllLinesAsync(FilePath);
            List<string> tempString = asyncList.ToList();

            if (tempString.Count < 2)
            {
                Debug.LogWarning("Tierlist must contain a header and at least one option");
                return;
            }

            CurrentData.DropperNames.Clear();
            CurrentData.DropperName = tempString[0];
            if (CurrentData.DropperName.Length >= 22) CurrentData.DropperName.Substring(0, 21);

            for (int i = 0; i < tempString.Count; i++)
            {
                if (i != 0 && i <= 48)
                {
                    string name = tempString[i];
                    if (name.Length >= 61) name.Substring(0, 60);
                    CurrentData.DropperNames.Add(name);
                }
            }
        }

        public static async Task SaveData()
        {
            string OutputtedList = string.Join(Environment.NewLine, CurrentData.DropperNames);
            string OutputtedText = $"{CurrentData.DropperName}\n{OutputtedList}";
            await File.WriteAllTextAsync(FilePath, OutputtedText);
        }
    }
}
