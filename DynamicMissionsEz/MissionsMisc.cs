using System;
using System.Linq;
using System.Timers;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace DynamicMissionsEz
{
    public static class MissionsMisc
    {
        public static void SpawnMissionNPC(int npcId)
        {
            int spawnX = -1;
            int spawnY = -1;

            for (int i = 0; i < 1000; i++)
            {
                int x = Main.rand.Next(100, Main.maxTilesX - 100);
                int y = Main.rand.Next(100, (int)Main.worldSurface);

                if (Main.tile[x, y] != null && Main.tile[x, y].active() && Main.tileSolid[Main.tile[x, y].type])
                {
                    if (!Main.tile[x, y - 1].active() && !Main.tile[x, y - 2].active() && !Main.tile[x, y - 3].active())
                    {
                        if (Main.tile[x, y - 1].liquid > 0 && Main.tile[x, y - 1].liquidType() == 1) continue;
                        spawnX = x;
                        spawnY = y - 1;
                        break;
                    }
                }
            }

            if (spawnX != -1 && spawnY != -1)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC existingNpc = Main.npc[i];
                    if (existingNpc.active && existingNpc.type == npcId && existingNpc.GivenName == "Objetivo")
                    {
                        existingNpc.position.X = spawnX * 16;
                        existingNpc.position.Y = spawnY * 16;
                        NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, i);
                        TShock.Log.ConsoleInfo(Missionsi18n.GetString("LogNpcTeleport", spawnX, spawnY));
                        return;
                    }
                }

                int index = NPC.NewNPC(NPC.GetBossSpawnSource(default), spawnX * 16, spawnY * 16, npcId);
                if (index < Main.maxNPCs)
                {
                    NPC npc = Main.npc[index];
                    npc.GivenName = "Objetivo";
                    npc.knockBackResist = 0f;

                    NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, index);
                    TShock.Log.ConsoleInfo(Missionsi18n.GetString("LogNpcSpawn", spawnX, spawnY));
                }
            }
        }

        public static void ClaimReward(TSPlayer player, string typeReward, string rewardString)
        {
            typeReward = typeReward.ToLower();
            try
            {
                switch (typeReward)
                {
                    case "money":
                        string[] coins = rewardString.Split(',');
                        foreach (string coin in coins)
                        {
                            string c = coin.Trim().ToLower();
                            int coinType = 0;
                            int coinAmount = 0;
                            if (c.EndsWith("g")) { coinType = 73; coinAmount = int.Parse(c.TrimEnd('g')); }
                            else if (c.EndsWith("s")) { coinType = 72; coinAmount = int.Parse(c.TrimEnd('s')); }
                            else if (c.EndsWith("c")) { coinType = 71; coinAmount = int.Parse(c.TrimEnd('c')); }
                            else if (c.EndsWith("p")) { coinType = 74; coinAmount = int.Parse(c.TrimEnd('p')); }

                            if (coinAmount > 0) player.GiveItem(coinType, coinAmount);
                        }
                        player.SendSuccessMessage(Missionsi18n.GetString("RewardMoney"));
                        break;

                    case "item":
                        string[] items = rewardString.Split(',');
                        foreach (string itm in items)
                        {
                            string[] itemData = itm.Split(':');
                            if (int.TryParse(itemData[0].Trim(), out int itemId))
                            {
                                int itemQuantity = itemData.Length > 1 && int.TryParse(itemData[1].Trim(), out int q) ? q : 1;
                                player.GiveItem(itemId, itemQuantity);
                            }
                        }
                        player.SendSuccessMessage(Missionsi18n.GetString("RewardItem"));
                        break;

                    case "buff":
                        string[] buffs = rewardString.Split(',');
                        foreach (string b in buffs)
                        {
                            string[] buffData = b.Split(':');
                            if (int.TryParse(buffData[0].Trim(), out int buffId))
                            {
                                string timeString = buffData.Length > 1 ? buffData[1].Trim().ToLower() : "1m";
                                int ticks = ParseTimeToTicks(timeString);
                                player.SetBuff(buffId, ticks);
                            }
                        }
                        player.SendSuccessMessage(Missionsi18n.GetString("RewardBuff"));
                        break;
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(Missionsi18n.GetString("LogRewardError", rewardString, player.Name, ex.Message));
                player.SendErrorMessage(Missionsi18n.GetString("RewardError"));
            }
        }

        public static bool IsInGuildRegion(TSPlayer player)
        {
            var currentRegions = TShock.Regions.InAreaRegion(player.TileX, player.TileY);
            if (currentRegions == null || !currentRegions.Any()) return false;

            foreach (var region in currentRegions)
            {
                using (var reader = MissionsDB.Db.QueryReader("SELECT 1 FROM GuildRegions WHERE RegionName = @0", region.Name))
                {
                    if (reader.Read()) return true;
                }
            }
            return false;
        }
        public static int ParseTimeToTicks(string timeString)
        {
            int timeValue = int.Parse(new string(timeString.Where(char.IsDigit).ToArray()));

            if (timeString.Contains("h")) return timeValue * 3600 * 60;
            if (timeString.Contains("m")) return timeValue * 60 * 60;
            if (timeString.Contains("s")) return timeValue * 60;

            return timeValue;
        }
        public static Timer TimeCheckTimer;
        public static DateTime NextBoardRefreshTime;

        public static void SetNextBoardRefresh()
        {
            TimeSpan refreshInterval = ParseTime(MissionsJSON.Config.ChangeMissionsTime);

            if (refreshInterval.TotalSeconds > 0) NextBoardRefreshTime = DateTime.UtcNow.Add(refreshInterval);
            else NextBoardRefreshTime = DateTime.MaxValue;
        }

        public static TimeSpan ParseTime(string timeStr)
        {
            if (string.IsNullOrWhiteSpace(timeStr)) return TimeSpan.Zero;

            timeStr = timeStr.Trim().ToLower();
            try
            {
                if (timeStr.EndsWith("s"))
                {
                    int seconds = int.Parse(timeStr.TrimEnd('s'));
                    return TimeSpan.FromSeconds(seconds);
                }
                else if (timeStr.EndsWith("m"))
                {
                    int minutes = int.Parse(timeStr.TrimEnd('m'));
                    return TimeSpan.FromMinutes(minutes);
                }
                else if (timeStr.EndsWith("hr"))
                {
                    int hours = int.Parse(timeStr.Replace("hr", ""));
                    return TimeSpan.FromHours(hours);
                }
                else if (timeStr.EndsWith("h"))
                {
                    int hours = int.Parse(timeStr.TrimEnd('h'));
                    return TimeSpan.FromHours(hours);
                }
                else if (int.TryParse(timeStr, out int pureNumber))
                {
                    return TimeSpan.FromMinutes(pureNumber);
                }
            }
            catch { }
            return TimeSpan.Zero;
        }

        public static void StartTimeSystem()
        {
            SetNextBoardRefresh();

            TimeCheckTimer = new Timer(60000);
            TimeCheckTimer.Elapsed += CheckExpiredMissions;
            TimeCheckTimer.AutoReset = true;
            TimeCheckTimer.Start();
        }

        private static void CheckExpiredMissions(object sender, ElapsedEventArgs e)
        {
            try
            {
                var expiredMissions = new System.Collections.Generic.List<Tuple<string, string>>();

                using (var reader = MissionsDB.Db.QueryReader("SELECT AccountName, MissionName, StartTime FROM ActiveMissions"))
                {
                    while (reader.Read())
                    {
                        string account = reader.Get<string>("AccountName");
                        string mName = reader.Get<string>("MissionName");
                        string startTimeStr = reader.Get<string>("StartTime");

                        var missionData = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName == mName);
                        if (missionData == null) continue;

                        if (DateTime.TryParse(startTimeStr, out DateTime parsedLocalTime))
                        {
                            DateTime startTime = parsedLocalTime.ToUniversalTime();
                            TimeSpan allowedTime = ParseTime(missionData.Time.ToString());

                            if (allowedTime.TotalSeconds > 0)
                            {
                                DateTime expirationTime = startTime.Add(allowedTime);

                                if (DateTime.UtcNow > expirationTime)
                                {
                                    expiredMissions.Add(Tuple.Create(account, mName));
                                }
                            }
                        }
                    }
                }

                foreach (var expired in expiredMissions)
                {
                    string accName = expired.Item1;
                    string missName = expired.Item2;

                    MissionsDB.Db.Query("DELETE FROM ActiveMissions WHERE AccountName = @0 AND MissionName = @1", accName, missName);

                    var player = TShock.Players.FirstOrDefault(p => p != null && p.Active && p.IsLoggedIn && p.Account.Name == accName);
                    if (player != null)
                    {
                        player.SendErrorMessage(Missionsi18n.GetString("TimeExpired", missName));
                    }
                }

                if (DateTime.UtcNow >= NextBoardRefreshTime)
                {
                    MissionsJSON.LoadAll();

                    MissionsDB.Db.Query("DELETE FROM PlayerMissionsCompleted");
                    MissionsDB.Db.Query("DELETE FROM GlobalMissions");

                    TSPlayer.All.SendMessage(Missionsi18n.GetString("BoardRefreshed"), 255, 215, 0);

                    SetNextBoardRefresh();
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(Missionsi18n.GetString("LogTimeError", ex.Message));
            }
        }
        public static void CheckAndCelebrate100Percent(TSPlayer player)
        {
            try
            {
                int totalMissions = MissionsJSON.MissionsList.Count;
                if (totalMissions == 0) return;

                int completed = 0;
                using (var reader = MissionsDB.Db.QueryReader("SELECT COUNT(*) AS Count FROM LifetimeMissionsCompleted WHERE AccountName = @0", player.Account.Name))
                {
                    if (reader.Read()) completed = reader.Get<int>("Count");
                }

                if (completed == totalMissions)
                {
                    string msg = Missionsi18n.GetString("MasteryAchieved", player.Name);
                    string[] colors = { "FF0000", "FF7F00", "FFFF00", "00FF00", "00ACFF", "9A00D4", "FE00FE" };
                    string rainbowMsg = "";
                    int cIdx = 0;

                    foreach (char c in msg)
                    {
                        if (c == ' ') { rainbowMsg += " "; }
                        else { rainbowMsg += $"[c/{colors[cIdx % colors.Length]}:{c}]"; cIdx++; }
                    }

                    TSPlayer.All.SendMessage(rainbowMsg, 255, 255, 255);

                    var source = NPC.GetBossSpawnSource(player.TPlayer.whoAmI);

                    int[] fireworks = { 167, 168, 169 };
                    for (int i = 0; i < 3; i++)
                    {
                        float speedX = Main.rand.Next(-3, 4);
                        float speedY = Main.rand.Next(-10, -6);

                        int proj = Projectile.NewProjectile(source, player.TPlayer.Center.X, player.TPlayer.Center.Y, speedX, speedY, fireworks[i], 0, 0, Main.myPlayer);
                        Main.projectile[proj].timeLeft = 40;
                        NetMessage.SendData((int)PacketTypes.ProjectileNew, -1, -1, null, proj);
                    }

                    for (int i = 0; i < 15; i++)
                    {
                        float speedX = Main.rand.Next(-6, 7);
                        float speedY = Main.rand.Next(-8, 0);
                        int conf = Projectile.NewProjectile(source, player.TPlayer.Center.X, player.TPlayer.Center.Y, speedX, speedY, 194, 0, 0, Main.myPlayer);
                        NetMessage.SendData((int)PacketTypes.ProjectileNew, -1, -1, null, conf);
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(Missionsi18n.GetString("LogPartyError", ex.Message));
            }
        }
    }
}
