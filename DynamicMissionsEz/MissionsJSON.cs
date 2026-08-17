using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TShockAPI;

namespace DynamicMissionsEz
{
    public class MissionsConfig
    {
        public string DefaultLang { get; set; } = "en-US";
        public bool RandomMission { get; set; } = true;
        public string ChangeMissionsTime { get; set; } = "1h";
        public int MissionsPerPage { get; set; } = 3;
        public int AvailableMissionsNumber { get; set; } = 5;
        public bool NeedGuild { get; set; } = true;
        public int MaxActiveMissionsPerPlayer { get; set; } = 3;
    }

    public class MissionData
    {
        public string MissionName { get; set; } = "Example mission";
        public string MissionDescription { get; set; } = "Description";
        public string TypeMission { get; set; } = "mine";
        public string TypeReward { get; set; } = "money";
        public string MissionObj { get; set; } = "13";
        public string TargetTileId { get; set; } = "-1";
        public string Reward { get; set; } = "5g";
        public int Rarity { get; set; } = 1;
        public string Time { get; set; } = "1h";
        public int Quant { get; set; } = 1;
        public bool GlobalQuant { get; set; } = false;
        public bool RemoveItem { get; set; } = true;
        public bool OnlyHardMode { get; set; } = false;
        public bool OnlyPreHardMode { get; set; } = false;
    }

    public class MissionsJSON
    {
        public static MissionsConfig Config;
        public static List<MissionData> MissionsList = new List<MissionData>();
        public static List<MissionData> ActiveBoardMissions = new List<MissionData>();

        private static string ConfigDirectory = Path.Combine(TShock.SavePath, "Dynamicmissions");
        private static string ConfigPath = Path.Combine(ConfigDirectory, "missionsconfig.json");
        private static string MissionsPath = Path.Combine(ConfigDirectory, "missionslist.json");

        public static void LoadAll()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);

                if (!File.Exists(ConfigPath))
                {
                    Config = new MissionsConfig();
                    SaveConfig();
                }
                else
                {
                    Config = JsonConvert.DeserializeObject<MissionsConfig>(File.ReadAllText(ConfigPath));
                }

                if (!File.Exists(MissionsPath))
                {
                    MissionsList = new List<MissionData> { new MissionData() };
                    SaveMissions();
                    ActiveBoardMissions = new List<MissionData>(MissionsList);
                }
                else
                {
                    MissionsList = JsonConvert.DeserializeObject<List<MissionData>>(File.ReadAllText(MissionsPath));

                    bool isHardMode = Terraria.Main.hardMode;

                    var validMissions = MissionsList.Where(m => {
                        if (m.OnlyHardMode && !isHardMode) return false;

                        if (m.OnlyPreHardMode && isHardMode) return false;

                        return true;
                    }).ToList();

                    var tempList = new List<MissionData>(validMissions);

                    if (Config.RandomMission && tempList.Count > 0)
                    {
                        ActiveBoardMissions.Clear();
                        var rng = new Random();

                        tempList = tempList.OrderBy(x => rng.Next()).ToList();

                        int needed = Math.Min(Config.AvailableMissionsNumber, tempList.Count);

                        for (int i = 0; i < needed; i++)
                        {
                            int totalWeight = tempList.Sum(m => Math.Max(1, 101 - Math.Clamp(m.Rarity, 1, 100)));
                            int randomVal = rng.Next(0, totalWeight);

                            int currentSum = 0;
                            MissionData selectedMission = null;

                            foreach (var m in tempList)
                            {
                                currentSum += Math.Max(1, 101 - Math.Clamp(m.Rarity, 1, 100));
                                if (randomVal < currentSum)
                                {
                                    selectedMission = m;
                                    break;
                                }
                            }

                            if (selectedMission != null)
                            {
                                ActiveBoardMissions.Add(selectedMission);
                                tempList.Remove(selectedMission);
                            }
                        }
                    }
                    else
                    {
                        ActiveBoardMissions = tempList.Take(Config.AvailableMissionsNumber).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(Missionsi18n.GetString("LogJsonError", ex.Message));
            }
        }

        public static void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }

        public static void SaveMissions()
        {
            File.WriteAllText(MissionsPath, JsonConvert.SerializeObject(MissionsList, Formatting.Indented));
        }
    }
}
