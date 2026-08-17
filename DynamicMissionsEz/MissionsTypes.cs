using System;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace DynamicMissionsEz
{
    public static class MissionsTypes
    {
        public static void HandleKillMission(NpcKilledEventArgs args)
        {
            NPC npc = args.npc;
            if (npc == null || !npc.active) return;

            foreach (var player in TShock.Players)
            {
                if (player == null || !player.Active || !player.IsLoggedIn) continue;

                float distance = Microsoft.Xna.Framework.Vector2.Distance(player.TPlayer.Center, npc.Center);
                if (distance > 2000f) continue;

                var missionsToUpdate = new System.Collections.Generic.List<Tuple<string, int>>();

                using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName, Progress FROM ActiveMissions WHERE AccountName = @0", player.Account.Name))
                {
                    while (reader.Read())
                    {
                        missionsToUpdate.Add(Tuple.Create(reader.Get<string>("MissionName"), reader.Get<int>("Progress")));
                    }
                }

                foreach (var activeMission in missionsToUpdate)
                {
                    string mName = activeMission.Item1;
                    int currentProgress = activeMission.Item2;

                    var missionData = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName == mName);

                    if (missionData != null && missionData.TypeMission.ToLower() == "kill")
                    {
                        string[] objParts = missionData.MissionObj.Split(':');
                        string targetNpc = objParts[0].ToLower();
                        int targetAmount = objParts.Length > 1 ? int.Parse(objParts[1]) : 1;

                        bool isTarget = false;
                        if (int.TryParse(targetNpc, out int npcId) && npc.type == npcId) isTarget = true;
                        else if (npc.FullName.ToLower().Contains(targetNpc)) isTarget = true;

                        if (isTarget)
                        {
                            currentProgress++;

                            if (currentProgress >= targetAmount)
                            {
                                if (MissionsJSON.Config.NeedGuild)
                                {
                                    MissionsDB.Db.Query("UPDATE ActiveMissions SET Progress = @0 WHERE AccountName = @1 AND MissionName = @2", currentProgress, player.Account.Name, mName);
                                    player.SendSuccessMessage(Missionsi18n.GetString("KillGuildSuccess", mName));
                                }
                                else
                                {
                                    MissionsDB.Db.Query("DELETE FROM ActiveMissions WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, mName);

                                    string fullRewardData = $"{missionData.TypeReward}:{missionData.Reward}";
                                    MissionsDB.Db.Query("INSERT INTO PendingRewards (AccountName, RewardString) VALUES (@0, @1)", player.Account.Name, fullRewardData);

                                    if (missionData.GlobalQuant)
                                    {
                                        int currentComps = 0;
                                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM GlobalMissions WHERE MissionName = @0", mName))
                                        {
                                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                        }
                                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO GlobalMissions (MissionName, CompletedCount) VALUES (@0, 1)", mName);
                                        else MissionsDB.Db.Query("UPDATE GlobalMissions SET CompletedCount = @0 WHERE MissionName = @1", currentComps + 1, mName);
                                    }
                                    else
                                    {
                                        int currentComps = 0;
                                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM PlayerMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, mName))
                                        {
                                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                        }
                                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO PlayerMissionsCompleted (AccountName, MissionName, CompletedCount) VALUES (@0, @1, 1)", player.Account.Name, mName);
                                        else MissionsDB.Db.Query("UPDATE PlayerMissionsCompleted SET CompletedCount = @0 WHERE AccountName = @1 AND MissionName = @2", currentComps + 1, player.Account.Name, mName);
                                    }

                                    player.SendSuccessMessage(Missionsi18n.GetString("MissionSuccess", mName));
                                }
                            }
                            else
                            {
                                MissionsDB.Db.Query("UPDATE ActiveMissions SET Progress = @0 WHERE AccountName = @1 AND MissionName = @2", currentProgress, player.Account.Name, mName);
                                player.SendInfoMessage(Missionsi18n.GetString("KillProgress", mName, currentProgress, targetAmount));
                            }
                        }
                    }
                }
            }
        }
        public static void HandleTileEdit(object sender, GetDataHandlers.TileEditEventArgs args)
        {
            if (args.Player == null || !args.Player.IsLoggedIn) return;

            if (args.Action == GetDataHandlers.EditAction.KillTile || args.Action == GetDataHandlers.EditAction.KillTileNoItem)
            {
                ITile tile = Main.tile[args.X, args.Y];

                bool wasPlacedByPlayer = CheckIfTileWasPlaced(args.X, args.Y);

                if (wasPlacedByPlayer)
                {
                    RemovePlacedTileFromDB(args.X, args.Y);
                    NetMessage.SendData(
                        (int)PacketTypes.CreateCombatTextExtended,
                        args.Player.Index, -1,
                        Terraria.Localization.NetworkText.FromLiteral(Missionsi18n.GetString("TileCheat")),
                        (int)new Microsoft.Xna.Framework.Color(255, 100, 100).PackedValue,
                        args.Player.TPlayer.position.X, args.Player.TPlayer.position.Y - 32f
                    );
                    return;
                }

                var missionsToUpdate = new System.Collections.Generic.List<Tuple<string, int>>();
                using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName, Progress FROM ActiveMissions WHERE AccountName = @0", args.Player.Account.Name))
                {
                    while (reader.Read()) missionsToUpdate.Add(Tuple.Create(reader.Get<string>("MissionName"), reader.Get<int>("Progress")));
                }

                foreach (var activeMission in missionsToUpdate)
                {
                    string mName = activeMission.Item1;
                    int currentProgress = activeMission.Item2;
                    var missionData = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName == mName);

                    if (missionData != null && (missionData.TypeMission.ToLower() == "mine" || missionData.TypeMission.ToLower() == "collect"))
                    {
                        string[] objParts = missionData.MissionObj.Split(':');
                        int targetAmount = objParts.Length > 1 ? int.Parse(objParts[1]) : 1;

                        if (IsTileMatch(tile, missionData.TargetTileId))
                        {
                            if (currentProgress >= targetAmount) continue;

                            currentProgress++;
                            MissionsDB.Db.Query("UPDATE ActiveMissions SET Progress = @0 WHERE AccountName = @1 AND MissionName = @2", currentProgress, args.Player.Account.Name, mName);

                            if (currentProgress >= targetAmount) args.Player.SendSuccessMessage(Missionsi18n.GetString("MineGuildSuccess", mName));
                            else if (currentProgress % 5 == 0 || currentProgress == targetAmount - 1) args.Player.SendInfoMessage(Missionsi18n.GetString("MineProgress", mName, currentProgress, targetAmount));
                        }
                    }
                }
            }
            else if (args.Action == GetDataHandlers.EditAction.PlaceTile)
            {
                int tilePlaced = args.EditData;
                bool shouldRegister = false;

                foreach (var mission in MissionsJSON.MissionsList)
                {
                    if (mission.TypeMission.ToLower() == "mine" || mission.TypeMission.ToLower() == "collect")
                    {
                        if (IsTileIdInString(tilePlaced, mission.TargetTileId))
                        {
                            using (var reader = MissionsDB.Db.QueryReader("SELECT 1 FROM ActiveMissions WHERE MissionName = @0", mission.MissionName))
                            {
                                if (reader.Read())
                                {
                                    shouldRegister = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (shouldRegister) RegisterPlacedTile(args.X, args.Y, args.Player.Account.Name);
            }
        }
        public static void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled) return;

            if (args.MsgID == PacketTypes.NpcTalk)
            {
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    byte playerId = reader.ReadByte();
                    short npcIndex = reader.ReadInt16();

                    if (npcIndex < 0 || npcIndex >= Main.maxNPCs) return;

                    TSPlayer player = TShock.Players[args.Msg.whoAmI];
                    if (player == null || !player.IsLoggedIn) return;

                    NPC npc = Main.npc[npcIndex];

                    if (npc.active && npc.GivenName == "Objetivo")
                    {
                        using (var dbReader = MissionsDB.Db.QueryReader("SELECT MissionName FROM ActiveMissions WHERE AccountName = @0", player.Account.Name))
                        {
                            bool hasFindMission = false;
                            string missionToComplete = "";
                            string rewardString = "";
                            string typeReward = "";
                            bool isGlobal = false;

                            while (dbReader.Read())
                            {
                                string mName = dbReader.Get<string>("MissionName");

                                var missionData = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName == mName);

                                if (missionData != null && missionData.TypeMission.ToLower() == "find")
                                {
                                    bool isTarget = false;
                                    string targetStr = missionData.MissionObj.ToLower();

                                    if (int.TryParse(targetStr, out int reqId) && reqId == npc.type) isTarget = true;
                                    else if (npc.FullName.ToLower().Contains(targetStr)) isTarget = true;

                                    if (isTarget)
                                    {
                                        hasFindMission = true;
                                        missionToComplete = missionData.MissionName;
                                        rewardString = missionData.Reward;
                                        typeReward = missionData.TypeReward;
                                        isGlobal = missionData.GlobalQuant;
                                        break;
                                    }
                                }
                            }

                            if (hasFindMission)
                            {
                                npc.GivenName = "";
                                int npcIndexToKill = npc.whoAmI;
                                NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npcIndexToKill);

                                System.Threading.Tasks.Task.Run(async () =>
                                {
                                    await System.Threading.Tasks.Task.Delay(20000);

                                    if (Main.npc[npcIndexToKill].active && Main.npc[npcIndexToKill].type != 0)
                                    {
                                        Main.npc[npcIndexToKill].active = false;
                                        Main.npc[npcIndexToKill].type = 0;
                                        NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npcIndexToKill);
                                    }
                                });

                                if (MissionsJSON.Config.NeedGuild)
                                {
                                    MissionsDB.Db.Query("UPDATE ActiveMissions SET Progress = 1 WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete);
                                    player.SendSuccessMessage(Missionsi18n.GetString("FindGuildSuccess"));
                                }
                                else
                                {
                                    MissionsDB.Db.Query("DELETE FROM ActiveMissions WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete);

                                    if (isGlobal)
                                    {
                                        int currentComps = 0;
                                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM GlobalMissions WHERE MissionName = @0", missionToComplete))
                                        {
                                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                        }
                                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO GlobalMissions (MissionName, CompletedCount) VALUES (@0, 1)", missionToComplete);
                                        else MissionsDB.Db.Query("UPDATE GlobalMissions SET CompletedCount = @0 WHERE MissionName = @1", currentComps + 1, missionToComplete);
                                    }
                                    else
                                    {
                                        int currentComps = 0;
                                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM PlayerMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete))
                                        {
                                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                        }
                                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO PlayerMissionsCompleted (AccountName, MissionName, CompletedCount) VALUES (@0, @1, 1)", player.Account.Name, missionToComplete);
                                        else MissionsDB.Db.Query("UPDATE PlayerMissionsCompleted SET CompletedCount = @0 WHERE AccountName = @1 AND MissionName = @2", currentComps + 1, player.Account.Name, missionToComplete);
                                    }

                                    string fullRewardData = $"{typeReward}:{rewardString}";
                                    MissionsDB.Db.Query("INSERT INTO PendingRewards (AccountName, RewardString) VALUES (@0, @1)", player.Account.Name, fullRewardData);

                                    bool isLifetimeCompleted = false;
                                    using (var lReader = MissionsDB.Db.QueryReader("SELECT 1 FROM LifetimeMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete))
                                    {
                                        if (lReader.Read()) isLifetimeCompleted = true;
                                    }
                                    if (!isLifetimeCompleted)
                                    {
                                        MissionsDB.Db.Query("INSERT INTO LifetimeMissionsCompleted (AccountName, MissionName) VALUES (@0, @1)", player.Account.Name, missionToComplete);
                                    }

                                    MissionsMisc.CheckAndCelebrate100Percent(player);

                                    player.SendSuccessMessage(Missionsi18n.GetString("MissionSuccess", missionToComplete));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool CheckIfTileWasPlaced(int x, int y)
        {
            try
            {
                using (var reader = MissionsDB.Db.QueryReader("SELECT 1 FROM PlacedBlocks WHERE X = @0 AND Y = @1", x, y))
                {
                    return reader.Read();
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error CheckIfTileWasPlaced: {ex.Message}");
                return false;
            }
        }

        private static void RegisterPlacedTile(int x, int y, string accountName)
        {
            try
            {
                MissionsDB.Db.Query("INSERT INTO PlacedBlocks (X, Y, AccountName) VALUES (@0, @1, @2)", x, y, accountName);
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error RegisterPlacedTile: {ex.Message}");
            }
        }

        private static void RemovePlacedTileFromDB(int x, int y)
        {
            try
            {
                MissionsDB.Db.Query("DELETE FROM PlacedBlocks WHERE X = @0 AND Y = @1", x, y);
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error RemovePlacedTileFromDB: {ex.Message}");
            }
        }
        public static void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (args.Npc == null || !args.Npc.active) return;

            TSPlayer tsPlayer = TShock.Players[args.Player.whoAmI];

            if (tsPlayer == null || !tsPlayer.Active || !tsPlayer.IsLoggedIn) return;

            NPC npc = args.Npc;

            if (npc.GivenName == "Objetivo")
            {
                using (var dbReader = MissionsDB.Db.QueryReader("SELECT MissionName FROM ActiveMissions WHERE AccountName = @0", tsPlayer.Account.Name))
                {
                    bool hasFindMission = false;
                    string missionToComplete = "";
                    string rewardString = "";
                    string typeReward = "";
                    bool isGlobal = false;

                    while (dbReader.Read())
                    {
                        string mName = dbReader.Get<string>("MissionName");
                        var missionData = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName == mName);

                        if (missionData != null && missionData.TypeMission.ToLower() == "find")
                        {
                            bool isTarget = false;
                            string targetStr = missionData.MissionObj.ToLower();

                            if (int.TryParse(targetStr, out int reqId) && reqId == npc.type) isTarget = true;
                            else if (npc.FullName.ToLower().Contains(targetStr)) isTarget = true;

                            if (isTarget)
                            {
                                hasFindMission = true;
                                missionToComplete = missionData.MissionName;
                                rewardString = missionData.Reward;
                                typeReward = missionData.TypeReward;
                                isGlobal = missionData.GlobalQuant;
                                break;
                            }
                        }
                    }

                    if (hasFindMission)
                    {
                        args.Handled = true;

                        npc.GivenName = "";
                        int npcIndexToKill = npc.whoAmI;
                        NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npcIndexToKill);

                        System.Threading.Tasks.Task.Run(async () =>
                        {
                            await System.Threading.Tasks.Task.Delay(20000);

                            if (Main.npc[npcIndexToKill].active && Main.npc[npcIndexToKill].type != 0)
                            {
                                Main.npc[npcIndexToKill].active = false;
                                Main.npc[npcIndexToKill].type = 0;
                                NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npcIndexToKill);
                            }
                        });

                        if (MissionsJSON.Config.NeedGuild)
                        {
                            MissionsDB.Db.Query("UPDATE ActiveMissions SET Progress = 1 WHERE AccountName = @0 AND MissionName = @1", tsPlayer.Account.Name, missionToComplete);
                            tsPlayer.SendSuccessMessage(Missionsi18n.GetString("FindGuildSuccess"));
                        }
                        else
                        {
                            MissionsDB.Db.Query("DELETE FROM ActiveMissions WHERE AccountName = @0 AND MissionName = @1", tsPlayer.Account.Name, missionToComplete);

                            if (isGlobal)
                            {
                                int currentComps = 0;
                                using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM GlobalMissions WHERE MissionName = @0", missionToComplete))
                                {
                                    if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                }
                                if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO GlobalMissions (MissionName, CompletedCount) VALUES (@0, 1)", missionToComplete);
                                else MissionsDB.Db.Query("UPDATE GlobalMissions SET CompletedCount = @0 WHERE MissionName = @1", currentComps + 1, missionToComplete);
                            }
                            else
                            {
                                int currentComps = 0;
                                using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM PlayerMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", tsPlayer.Account.Name, missionToComplete))
                                {
                                    if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                }
                                if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO PlayerMissionsCompleted (AccountName, MissionName, CompletedCount) VALUES (@0, @1, 1)", tsPlayer.Account.Name, missionToComplete);
                                else MissionsDB.Db.Query("UPDATE PlayerMissionsCompleted SET CompletedCount = @0 WHERE AccountName = @1 AND MissionName = @2", currentComps + 1, tsPlayer.Account.Name, missionToComplete);
                            }

                            string fullRewardData = $"{typeReward}:{rewardString}";
                            MissionsDB.Db.Query("INSERT INTO PendingRewards (AccountName, RewardString) VALUES (@0, @1)", tsPlayer.Account.Name, fullRewardData);

                            bool isLifetimeCompleted = false;
                            using (var lReader = MissionsDB.Db.QueryReader("SELECT 1 FROM LifetimeMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", tsPlayer.Account.Name, missionToComplete))
                            {
                                if (lReader.Read()) isLifetimeCompleted = true;
                            }
                            if (!isLifetimeCompleted)
                            {
                                MissionsDB.Db.Query("INSERT INTO LifetimeMissionsCompleted (AccountName, MissionName) VALUES (@0, @1)", tsPlayer.Account.Name, missionToComplete);
                            }

                            MissionsMisc.CheckAndCelebrate100Percent(tsPlayer);

                            tsPlayer.SendSuccessMessage(Missionsi18n.GetString("MissionSuccess", missionToComplete));
                        }
                    }
                }
            }
        }
        private static int proximityTick = 0;

        public static void OnGameUpdate(EventArgs args)
        {
            proximityTick++;
            if (proximityTick >= 30)
            {
                proximityTick = 0;

                foreach (var player in TShock.Players)
                {
                    if (player == null || !player.Active || !player.IsLoggedIn) continue;

                    var activeFindMissions = new System.Collections.Generic.List<string>();
                    using (var dbReader = MissionsDB.Db.QueryReader("SELECT MissionName FROM ActiveMissions WHERE AccountName = @0", player.Account.Name))
                    {
                        while (dbReader.Read()) activeFindMissions.Add(dbReader.Get<string>("MissionName"));
                    }

                    if (activeFindMissions.Count == 0) continue;

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];

                        if (!npc.active || npc.GivenName != "Objetivo") continue;

                        float distance = Microsoft.Xna.Framework.Vector2.Distance(player.TPlayer.Center, npc.Center);
                        if (distance <= 300f)
                        {
                            bool hasFindMission = false;
                            string missionToComplete = "";
                            string rewardString = "";
                            string typeReward = "";
                            bool isGlobal = false;

                            foreach (var mName in activeFindMissions)
                            {
                                var missionData = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName == mName);
                                if (missionData != null && missionData.TypeMission.ToLower() == "find")
                                {
                                    bool isTarget = false;
                                    string targetStr = missionData.MissionObj.ToLower();

                                    if (int.TryParse(targetStr, out int reqId) && reqId == npc.type) isTarget = true;
                                    else if (npc.FullName.ToLower().Contains(targetStr)) isTarget = true;

                                    if (isTarget)
                                    {
                                        hasFindMission = true;
                                        missionToComplete = missionData.MissionName;
                                        rewardString = missionData.Reward;
                                        typeReward = missionData.TypeReward;
                                        isGlobal = missionData.GlobalQuant;
                                        break;
                                    }
                                }
                            }

                            if (hasFindMission)
                            {
                                npc.GivenName = "";
                                int npcIndexToKill = npc.whoAmI;
                                NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npcIndexToKill);

                                System.Threading.Tasks.Task.Run(async () =>
                                {
                                    await System.Threading.Tasks.Task.Delay(20000);

                                    if (Main.npc[npcIndexToKill].active && Main.npc[npcIndexToKill].type != 0)
                                    {
                                        Main.npc[npcIndexToKill].active = false;
                                        Main.npc[npcIndexToKill].type = 0;
                                        NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npcIndexToKill);
                                    }
                                });

                                if (MissionsJSON.Config.NeedGuild)
                                {
                                    MissionsDB.Db.Query("UPDATE ActiveMissions SET Progress = 1 WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete);
                                    player.SendSuccessMessage(Missionsi18n.GetString("FindGuildSuccess"));
                                }
                                else
                                {
                                    MissionsDB.Db.Query("DELETE FROM ActiveMissions WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete);

                                    if (isGlobal)
                                    {
                                        int currentComps = 0;
                                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM GlobalMissions WHERE MissionName = @0", missionToComplete))
                                        {
                                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                        }
                                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO GlobalMissions (MissionName, CompletedCount) VALUES (@0, 1)", missionToComplete);
                                        else MissionsDB.Db.Query("UPDATE GlobalMissions SET CompletedCount = @0 WHERE MissionName = @1", currentComps + 1, missionToComplete);
                                    }
                                    else
                                    {
                                        int currentComps = 0;
                                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM PlayerMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete))
                                        {
                                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                                        }
                                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO PlayerMissionsCompleted (AccountName, MissionName, CompletedCount) VALUES (@0, @1, 1)", player.Account.Name, missionToComplete);
                                        else MissionsDB.Db.Query("UPDATE PlayerMissionsCompleted SET CompletedCount = @0 WHERE AccountName = @1 AND MissionName = @2", currentComps + 1, player.Account.Name, missionToComplete);
                                    }

                                    string fullRewardData = $"{typeReward}:{rewardString}";
                                    MissionsDB.Db.Query("INSERT INTO PendingRewards (AccountName, RewardString) VALUES (@0, @1)", player.Account.Name, fullRewardData);

                                    bool isLifetimeCompleted = false;
                                    using (var lReader = MissionsDB.Db.QueryReader("SELECT 1 FROM LifetimeMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", player.Account.Name, missionToComplete))
                                    {
                                        if (lReader.Read()) isLifetimeCompleted = true;
                                    }
                                    if (!isLifetimeCompleted)
                                    {
                                        MissionsDB.Db.Query("INSERT INTO LifetimeMissionsCompleted (AccountName, MissionName) VALUES (@0, @1)", player.Account.Name, missionToComplete);
                                    }

                                    MissionsMisc.CheckAndCelebrate100Percent(player);

                                    player.SendSuccessMessage(Missionsi18n.GetString("MissionSuccess", missionToComplete));
                                }
                            }
                        }
                    }
                }
            }
        }
        private static bool IsTileMatch(ITile tile, string targetTilesStr)
        {
            if (string.IsNullOrWhiteSpace(targetTilesStr) || targetTilesStr == "-1") return false;

            string[] targets = targetTilesStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var t in targets)
            {
                string[] parts = t.Trim().Split(':');
                if (parts.Length > 0 && int.TryParse(parts[0], out int targetId))
                {
                    if (tile.type == targetId)
                    {
                        if (parts.Length > 1 && int.TryParse(parts[1], out int targetSubId))
                        {
                            int styleX = tile.frameX / 18;
                            int styleY = tile.frameY / 18;

                            if (styleX == targetSubId || styleY == targetSubId) return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsTileIdInString(int tileId, string targetTilesStr)
        {
            if (string.IsNullOrWhiteSpace(targetTilesStr) || targetTilesStr == "-1") return false;
            string[] targets = targetTilesStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in targets)
            {
                string[] parts = t.Trim().Split(':');
                if (parts.Length > 0 && int.TryParse(parts[0], out int targetId))
                {
                    if (targetId == tileId) return true;
                }
            }
            return false;
        }
    }
}
