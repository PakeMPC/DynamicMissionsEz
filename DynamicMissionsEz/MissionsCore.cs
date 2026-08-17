using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using Microsoft.Xna.Framework;

namespace DynamicMissionsEz
{
    [ApiVersion(2, 1)]
    public class MissionsCore : TerrariaPlugin
    {
        public override string Name => "DynamicMissionsEz";
        public override Version Version => new Version(1, 0, 1);
        public override string Author => "PakeMPC";
        public override string Description => Missionsi18n.GetString("PluginDesc");

        public MissionsCore(Main game) : base(game)
        {
        }

        private void ApplyLanguage()
        {
            try
            {
                var culture = Terraria.Localization.GameCulture.FromName(MissionsJSON.Config.DefaultLang);
                if (culture != null)
                {
                    Terraria.Localization.LanguageManager.Instance.SetLanguage(culture);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(Missionsi18n.GetString("LogLanguageError", MissionsJSON.Config.DefaultLang, ex.Message));
            }
        }

        public override void Initialize()
        {
            MissionsJSON.LoadAll();
            MissionsDB.Connect();
            ApplyLanguage();

            Commands.ChatCommands.Add(new Command("missions.admin", GuildCommands, "createguild", "assignguild", "removeguild", "deleteguild"));
            Commands.ChatCommands.Add(new Command("missions.user", MissionCommands, "mission"));

            ServerApi.Hooks.NpcKilled.Register(this, MissionsTypes.HandleKillMission);
            GetDataHandlers.TileEdit += MissionsTypes.HandleTileEdit;
            ServerApi.Hooks.NetGetData.Register(this, MissionsTypes.OnGetData);
            ServerApi.Hooks.NpcStrike.Register(this, MissionsTypes.OnNpcStrike);
            TShockAPI.Hooks.RegionHooks.RegionEntered += OnRegionEntered;
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += OnReload;
            TShockAPI.Hooks.PlayerHooks.PlayerCommand += OnPlayerCommand;
            ServerApi.Hooks.GameUpdate.Register(this, MissionsTypes.OnGameUpdate);
            MissionsMisc.StartTimeSystem();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcKilled.Deregister(this, MissionsTypes.HandleKillMission);
                GetDataHandlers.TileEdit -= MissionsTypes.HandleTileEdit;
                ServerApi.Hooks.NetGetData.Deregister(this, MissionsTypes.OnGetData);
                ServerApi.Hooks.NpcStrike.Deregister(this, MissionsTypes.OnNpcStrike);
                TShockAPI.Hooks.RegionHooks.RegionEntered -= OnRegionEntered;
                TShockAPI.Hooks.GeneralHooks.ReloadEvent -= OnReload;
                TShockAPI.Hooks.PlayerHooks.PlayerCommand -= OnPlayerCommand;
                ServerApi.Hooks.GameUpdate.Deregister(this, MissionsTypes.OnGameUpdate);
            }
            base.Dispose(disposing);
        }

        private void OnReload(TShockAPI.Hooks.ReloadEventArgs e)
        {
            try
            {
                MissionsJSON.LoadAll();
                MissionsMisc.SetNextBoardRefresh();
                ApplyLanguage();
                e.Player.SendSuccessMessage(Missionsi18n.GetString("CmdReloadSuccess"));
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(Missionsi18n.GetString("LogReloadError", ex.Message));
                e.Player.SendErrorMessage(Missionsi18n.GetString("CmdReloadError"));
            }
        }

        private void OnPlayerCommand(TShockAPI.Hooks.PlayerCommandEventArgs args)
        {
            if (args.CommandName.ToLower() == "tpnpc")
            {
                using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName FROM ActiveMissions WHERE AccountName = @0", args.Player.Account.Name))
                {
                    while (reader.Read())
                    {
                        var mission = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName == reader.Get<string>("MissionName"));
                        if (mission != null && mission.TypeMission.ToLower() == "find")
                        {
                            args.Player.SendErrorMessage(Missionsi18n.GetString("AntiTpFind"));
                            args.Handled = true;
                            return;
                        }
                    }
                }
            }
        }

        private void OnRegionEntered(TShockAPI.Hooks.RegionHooks.RegionEnteredEventArgs args)
        {
            if (args.Player == null || !args.Player.IsLoggedIn) return;

            using (var reader = MissionsDB.Db.QueryReader("SELECT 1 FROM GuildRegions WHERE RegionName = @0", args.Region.Name))
            {
                if (reader.Read())
                {
                    NetMessage.SendData(
                        (int)PacketTypes.CreateCombatTextExtended,
                        args.Player.Index, -1,
                        Terraria.Localization.NetworkText.FromLiteral(Missionsi18n.GetString("GuildFloating")),
                        (int)new Microsoft.Xna.Framework.Color(255, 215, 0).PackedValue,
                        args.Player.TPlayer.position.X,
                        args.Player.TPlayer.position.Y - 32f
                    );

                    args.Player.SendMessage(Missionsi18n.GetString("GuildWelcome"), 255, 215, 0);
                }
            }
        }

        private void GuildCommands(CommandArgs args)
        {
            string cmd = args.Message.Split(' ')[0].ToLower();

            switch (cmd)
            {
                case "createguild":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdCreateGuildUsage"));
                        return;
                    }

                    string action = args.Parameters[0].ToLower();
                    string argValue = args.Parameters[1];

                    if (action == "set")
                    {
                        if (argValue == "1")
                        {
                            args.Player.AwaitingTempPoint = 1;
                            args.Player.SendInfoMessage(Missionsi18n.GetString("CmdCreateGuildP1"));
                        }
                        else if (argValue == "2")
                        {
                            args.Player.AwaitingTempPoint = 2;
                            args.Player.SendInfoMessage(Missionsi18n.GetString("CmdCreateGuildP2"));
                        }
                    }
                    else if (action == "define")
                    {
                        if (!args.Player.TempPoints.Any(p => p != Point.Zero))
                        {
                            args.Player.SendErrorMessage(Missionsi18n.GetString("CmdCreateGuildErrPoints"));
                            return;
                        }

                        string regionName = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                        var x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        var y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        var width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        var height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);

                        if (TShock.Regions.AddRegion(x, y, width, height, regionName, args.Player.Account.Name, Main.worldID.ToString(), 0))
                        {
                            MissionsDB.Db.Query("INSERT INTO GuildRegions (RegionName) VALUES (@0)", regionName);
                            args.Player.SendSuccessMessage(Missionsi18n.GetString("CmdCreateGuildSuccess", regionName));
                        }
                        else
                        {
                            args.Player.SendErrorMessage(Missionsi18n.GetString("CmdCreateGuildErrFail", regionName));
                        }
                    }
                    break;

                case "assignguild":
                    if (args.Parameters.Count < 1)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAssignGuildUsage"));
                        return;
                    }

                    string existingRegion = string.Join(" ", args.Parameters);
                    var region = TShock.Regions.GetRegionByName(existingRegion);
                    if (region == null)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAssignGuildErrNotFound", existingRegion));
                        return;
                    }

                    try
                    {
                        MissionsDB.Db.Query("INSERT INTO GuildRegions (RegionName) VALUES (@0)", region.Name);
                        args.Player.SendSuccessMessage(Missionsi18n.GetString("CmdAssignGuildSuccess", region.Name));
                    }
                    catch
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAssignGuildErrAlready", region.Name));
                    }
                    break;

                case "removeguild":
                    if (args.Parameters.Count < 1)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdRemoveGuildUsage"));
                        return;
                    }
                    string regionToRemove = string.Join(" ", args.Parameters);

                    int rowsDeleted = MissionsDB.Db.Query("DELETE FROM GuildRegions WHERE RegionName = @0", regionToRemove);
                    if (rowsDeleted > 0)
                        args.Player.SendSuccessMessage(Missionsi18n.GetString("CmdRemoveGuildSuccess", regionToRemove));
                    else
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdRemoveGuildErrNotGuild", regionToRemove));
                    break;

                case "deleteguild":
                    if (args.Parameters.Count < 1)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeleteGuildUsage"));
                        return;
                    }
                    string regionToDelete = string.Join(" ", args.Parameters);

                    MissionsDB.Db.Query("DELETE FROM GuildRegions WHERE RegionName = @0", regionToDelete);

                    if (TShock.Regions.DeleteRegion(regionToDelete))
                        args.Player.SendSuccessMessage(Missionsi18n.GetString("CmdDeleteGuildSuccess", regionToDelete));
                    else
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeleteGuildErrFail", regionToDelete));
                    break;
            }
        }

        private void MissionCommands(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage(Missionsi18n.GetString("NotLoggedIn"));
                return;
            }

            string subCmd = args.Parameters.Count > 0 ? args.Parameters[0].ToLower() : "1";

            switch (subCmd)
            {
                case "reward":
                    var pendingRewards = new List<Tuple<int, string>>();
                    using (var reader = MissionsDB.Db.QueryReader("SELECT ID, RewardString FROM PendingRewards WHERE AccountName = @0", args.Player.Account.Name))
                    {
                        while (reader.Read())
                        {
                            pendingRewards.Add(Tuple.Create(reader.Get<int>("ID"), reader.Get<string>("RewardString")));
                        }
                    }

                    if (args.Parameters.Count == 1)
                    {
                        if (pendingRewards.Count == 0)
                        {
                            args.Player.SendInfoMessage(Missionsi18n.GetString("NoRewards"));
                            return;
                        }

                        args.Player.SendInfoMessage(Missionsi18n.GetString("PendingTitle"));
                        for (int i = 0; i < pendingRewards.Count; i++)
                        {
                            string[] parts = pendingRewards[i].Item2.Split(new char[] { ':' }, 2);
                            string rawType = parts[0].ToLower();
                            string rawVal = parts.Length > 1 ? parts[1].Trim() : "";

                            string displayType = rawType;
                            if (rawType == "money") displayType = Missionsi18n.GetString("TypeMoney");
                            else if (rawType == "item") displayType = Missionsi18n.GetString("TypeItem");
                            else if (rawType == "buff") displayType = Missionsi18n.GetString("TypeBuff");

                            string displayVal = "";
                            if (rawType == "money")
                            {
                                string[] coins = rawVal.Split(',');
                                foreach (string coin in coins)
                                {
                                    string rStr = coin.Trim().ToLower();
                                    if (rStr.EndsWith("g")) displayVal += $"[i/s{rStr.TrimEnd('g')}:73] ";
                                    else if (rStr.EndsWith("s")) displayVal += $"[i/s{rStr.TrimEnd('s')}:72] ";
                                    else if (rStr.EndsWith("c")) displayVal += $"[i/s{rStr.TrimEnd('c')}:71] ";
                                    else if (rStr.EndsWith("p")) displayVal += $"[i/s{rStr.TrimEnd('p')}:74] ";
                                }
                            }
                            else if (rawType == "item")
                            {
                                string[] items = rawVal.Split(',');
                                foreach (string itm in items)
                                {
                                    string[] rP = itm.Split(':');
                                    int itemId = int.TryParse(rP[0].Trim(), out int parsedRewId) ? parsedRewId : 0;
                                    int amount = rP.Length > 1 ? (int.TryParse(rP[1].Trim(), out int pAmt) ? pAmt : 1) : 1;
                                    displayVal += $"[i/s{amount}:{itemId}] ";
                                }
                            }
                            else if (rawType == "buff")
                            {
                                string[] buffs = rawVal.Split(',');
                                foreach (string b in buffs)
                                {
                                    string[] rP = b.Split(':');
                                    int bId = int.TryParse(rP[0].Trim(), out int parsedBId) ? parsedBId : 0;
                                    string bTime = rP.Length > 1 ? rP[1].Trim() : "";
                                    displayVal += $"{Lang.GetBuffName(bId)} ({bTime}) ";
                                }
                            }
                            displayVal = displayVal.TrimEnd();

                            args.Player.SendInfoMessage(Missionsi18n.GetString("RewardFormat", i + 1, displayType, displayVal));
                        }
                        args.Player.SendInfoMessage(Missionsi18n.GetString("RewardSelect"));
                    }
                    else if (args.Parameters.Count == 2)
                    {
                        if (int.TryParse(args.Parameters[1], out int index) && index > 0 && index <= pendingRewards.Count)
                        {
                            var selectedReward = pendingRewards[index - 1];
                            int dbId = selectedReward.Item1;
                            string fullReward = selectedReward.Item2;

                            string[] parts = fullReward.Split(new char[] { ':' }, 2);
                            if (parts.Length == 2)
                            {
                                MissionsMisc.ClaimReward(args.Player, parts[0], parts[1]);
                                MissionsDB.Db.Query("DELETE FROM PendingRewards WHERE ID = @0", dbId);
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage(Missionsi18n.GetString("RewardInvalid", pendingRewards.Count));
                        }
                    }
                    break;

                case "accept":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAcceptUsage"));
                        return;
                    }

                    string requestedMission = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

                    var matches = MissionsJSON.ActiveBoardMissions
                        .Where(m => m.MissionName.StartsWith(requestedMission, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (matches.Count == 0)
                    {
                        matches = MissionsJSON.ActiveBoardMissions
                            .Where(m => m.MissionName.IndexOf(requestedMission, StringComparison.OrdinalIgnoreCase) >= 0)
                            .ToList();
                    }

                    if (matches.Count == 0)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdErrNotFound", requestedMission));
                        return;
                    }
                    else if (matches.Count > 1)
                    {
                        string matchNames = string.Join(", ", matches.Select(m => m.MissionName));
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdErrMultipleMatches", matchNames));
                        return;
                    }

                    var missionToAccept = matches[0];

                    if (MissionsJSON.Config.NeedGuild && !MissionsMisc.IsInGuildRegion(args.Player))
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("NotInGuild"));
                        return;
                    }

                    int activeCount = 0;
                    bool alreadyHasMission = false;

                    using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName FROM ActiveMissions WHERE AccountName = @0", args.Player.Account.Name))
                    {
                        while (reader.Read())
                        {
                            activeCount++;
                            if (reader.Get<string>("MissionName").Equals(missionToAccept.MissionName, StringComparison.OrdinalIgnoreCase))
                            {
                                alreadyHasMission = true;
                            }
                        }
                    }

                    if (alreadyHasMission)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAcceptErrAlreadyActive"));
                        return;
                    }

                    if (activeCount >= MissionsJSON.Config.MaxActiveMissionsPerPlayer)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAcceptErrMaxReached", MissionsJSON.Config.MaxActiveMissionsPerPlayer));
                        return;
                    }

                    if (missionToAccept.GlobalQuant)
                    {
                        using (var reader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM GlobalMissions WHERE MissionName = @0", missionToAccept.MissionName))
                        {
                            if (reader.Read())
                            {
                                int completed = reader.Get<int>("CompletedCount");
                                if (completed >= missionToAccept.Quant)
                                {
                                    args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAcceptErrGlobalMax"));
                                    return;
                                }
                            }
                        }
                    }

                    if (!missionToAccept.GlobalQuant)
                    {
                        using (var reader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM PlayerMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", args.Player.Account.Name, missionToAccept.MissionName))
                        {
                            if (reader.Read())
                            {
                                int completed = reader.Get<int>("CompletedCount");
                                if (completed >= missionToAccept.Quant)
                                {
                                    args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAcceptErrPersonalMax"));
                                    return;
                                }
                            }
                        }
                    }

                    try
                    {
                        string startTime = DateTime.UtcNow.ToString("o");

                        MissionsDB.Db.Query("INSERT INTO ActiveMissions (AccountName, MissionName, Progress, StartTime) VALUES (@0, @1, @2, @3)",
                            args.Player.Account.Name, missionToAccept.MissionName, 0, startTime);

                        args.Player.SendSuccessMessage(Missionsi18n.GetString("CmdAcceptSuccess", missionToAccept.MissionName));
                        args.Player.SendInfoMessage(Missionsi18n.GetString("CmdAcceptObjective", missionToAccept.MissionDescription));

                        if (missionToAccept.TypeMission.ToLower() == "find" && int.TryParse(missionToAccept.MissionObj, out int npcId))
                        {
                            MissionsMisc.SpawnMissionNPC(npcId);
                            args.Player.SendSuccessMessage(Missionsi18n.GetString("CmdAcceptFindSpawn"));
                        }
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.ConsoleError(Missionsi18n.GetString("LogAcceptError", ex.Message));
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdAcceptErrInternal"));
                    }
                    break;

                case "deliver":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeliverUsage"));
                        return;
                    }

                    string missionToDeliver = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

                    string exactMissionToDeliver = null;
                    int progress = -1;
                    using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName, Progress FROM ActiveMissions WHERE AccountName = @0", args.Player.Account.Name))
                    {
                        while (reader.Read())
                        {
                            string activeName = reader.Get<string>("MissionName");
                            if (activeName.StartsWith(missionToDeliver, StringComparison.OrdinalIgnoreCase) ||
                                activeName.IndexOf(missionToDeliver, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                exactMissionToDeliver = activeName;
                                progress = reader.Get<int>("Progress");
                                break;
                            }
                        }
                    }

                    if (exactMissionToDeliver == null)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeliverErrNotActive", missionToDeliver));
                        return;
                    }

                    var mData = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName.Equals(exactMissionToDeliver, StringComparison.OrdinalIgnoreCase));
                    if (mData == null)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeliverErrNoConfig"));
                        return;
                    }

                    if (MissionsJSON.Config.NeedGuild && !MissionsMisc.IsInGuildRegion(args.Player))
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeliverErrNotInGuild"));
                        return;
                    }

                    string typeMiss = mData.TypeMission.ToLower();
                    string[] objParts = mData.MissionObj.Split(':');
                    int requiredAmount = objParts.Length > 1 ? int.Parse(objParts[1]) : 1;

                    if (typeMiss == "mine" || typeMiss == "collect")
                    {
                        int requiredItemId = int.Parse(objParts[0]);

                        if (progress < requiredAmount)
                        {
                            args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeliverErrNotEnoughMine", progress, requiredAmount));
                            return;
                        }

                        int playerItemCount = 0;
                        foreach (var item in args.Player.TPlayer.inventory)
                        {
                            if (item != null && item.type == requiredItemId)
                                playerItemCount += item.stack;
                        }

                        if (playerItemCount < requiredAmount)
                        {
                            args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeliverErrNoItems", requiredAmount));
                            return;
                        }

                        if (mData.RemoveItem)
                        {
                            int itemsToRemove = requiredAmount;
                            for (int i = 0; i < 58; i++)
                            {
                                if (itemsToRemove <= 0) break;

                                var item = args.Player.TPlayer.inventory[i];
                                if (item != null && item.type == requiredItemId)
                                {
                                    if (item.stack <= itemsToRemove)
                                    {
                                        itemsToRemove -= item.stack;
                                        item.stack = 0;
                                        item.netDefaults(0);
                                    }
                                    else
                                    {
                                        item.stack -= itemsToRemove;
                                        itemsToRemove = 0;
                                    }
                                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, args.Player.Index, i);
                                }
                            }
                        }
                    }
                    else if (typeMiss == "kill" || typeMiss == "find")
                    {
                        int targetCheck = (typeMiss == "find") ? 1 : requiredAmount;

                        if (progress < targetCheck)
                        {
                            args.Player.SendErrorMessage(Missionsi18n.GetString("CmdDeliverErrNotEnoughKill"));
                            return;
                        }
                    }

                    MissionsDB.Db.Query("DELETE FROM ActiveMissions WHERE AccountName = @0 AND MissionName = @1", args.Player.Account.Name, mData.MissionName);

                    string finalReward = $"{mData.TypeReward}:{mData.Reward}";
                    MissionsDB.Db.Query("INSERT INTO PendingRewards (AccountName, RewardString) VALUES (@0, @1)", args.Player.Account.Name, finalReward);

                    if (mData.GlobalQuant)
                    {
                        int currentComps = 0;
                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM GlobalMissions WHERE MissionName = @0", mData.MissionName))
                        {
                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                        }
                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO GlobalMissions (MissionName, CompletedCount) VALUES (@0, 1)", mData.MissionName);
                        else MissionsDB.Db.Query("UPDATE GlobalMissions SET CompletedCount = @0 WHERE MissionName = @1", currentComps + 1, mData.MissionName);
                    }
                    else
                    {
                        int currentComps = 0;
                        using (var compReader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM PlayerMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", args.Player.Account.Name, mData.MissionName))
                        {
                            if (compReader.Read()) currentComps = compReader.Get<int>("CompletedCount");
                        }
                        if (currentComps == 0) MissionsDB.Db.Query("INSERT INTO PlayerMissionsCompleted (AccountName, MissionName, CompletedCount) VALUES (@0, @1, 1)", args.Player.Account.Name, mData.MissionName);
                        else MissionsDB.Db.Query("UPDATE PlayerMissionsCompleted SET CompletedCount = @0 WHERE AccountName = @1 AND MissionName = @2", currentComps + 1, args.Player.Account.Name, mData.MissionName);
                    }

                    bool isLifetimeCompleted = false;
                    using (var reader = MissionsDB.Db.QueryReader("SELECT 1 FROM LifetimeMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", args.Player.Account.Name, exactMissionToDeliver))
                    {
                        if (reader.Read()) isLifetimeCompleted = true;
                    }

                    if (!isLifetimeCompleted)
                    {
                        MissionsDB.Db.Query("INSERT INTO LifetimeMissionsCompleted (AccountName, MissionName) VALUES (@0, @1)", args.Player.Account.Name, exactMissionToDeliver);
                    }

                    MissionsMisc.CheckAndCelebrate100Percent(args.Player);

                    args.Player.SendSuccessMessage(Missionsi18n.GetString("MissionSuccess", mData.MissionName));
                    break;

                case "cancel":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdCancelUsage"));

                        var activeMissionsList = new System.Collections.Generic.List<string>();
                        using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName FROM ActiveMissions WHERE AccountName = @0", args.Player.Account.Name))
                        {
                            while (reader.Read()) activeMissionsList.Add(reader.Get<string>("MissionName"));
                        }

                        if (activeMissionsList.Count > 0)
                        {
                            args.Player.SendInfoMessage(Missionsi18n.GetString("CmdCancelActiveList", string.Join(", ", activeMissionsList)));
                        }
                        else
                        {
                            args.Player.SendInfoMessage(Missionsi18n.GetString("CmdCancelNoActive"));
                        }
                        return;
                    }

                    string missionToCancel = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

                    string exactMissionToCancel = null;
                    using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName FROM ActiveMissions WHERE AccountName = @0", args.Player.Account.Name))
                    {
                        while (reader.Read())
                        {
                            string activeName = reader.Get<string>("MissionName");
                            if (activeName.StartsWith(missionToCancel, StringComparison.OrdinalIgnoreCase) ||
                                activeName.IndexOf(missionToCancel, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                exactMissionToCancel = activeName;
                                break;
                            }
                        }
                    }

                    if (exactMissionToCancel == null)
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdErrNotFound", missionToCancel));
                        return;
                    }

                    var mDataCancel = MissionsJSON.MissionsList.FirstOrDefault(m => m.MissionName.Equals(exactMissionToCancel, StringComparison.OrdinalIgnoreCase));
                    if (mDataCancel != null && mDataCancel.TypeMission.ToLower() == "find")
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc.active && npc.GivenName == "Objetivo")
                            {
                                string targetStr = mDataCancel.MissionObj.ToLower();
                                if ((int.TryParse(targetStr, out int reqId) && reqId == npc.type) || npc.FullName.ToLower().Contains(targetStr))
                                {
                                    npc.active = false;
                                    npc.type = 0;
                                    NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, i);
                                }
                            }
                        }
                    }

                    int rowsAffected = MissionsDB.Db.Query("DELETE FROM ActiveMissions WHERE AccountName = @0 AND MissionName = @1", args.Player.Account.Name, exactMissionToCancel);

                    if (rowsAffected > 0)
                    {
                        args.Player.SendSuccessMessage(Missionsi18n.GetString("CmdCancelSuccess", exactMissionToCancel));
                    }
                    else
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdCancelErrFail"));
                    }
                    break;

                default:
                    if (MissionsJSON.Config.NeedGuild && !MissionsMisc.IsInGuildRegion(args.Player))
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("NotInGuild"));
                        return;
                    }

                    int page = 1;
                    if (!int.TryParse(subCmd, out page))
                    {
                        args.Player.SendErrorMessage(Missionsi18n.GetString("CmdUnknown"));
                        return;
                    }

                    int missionsPerPage = MissionsJSON.Config.MissionsPerPage;
                    var allMissions = MissionsJSON.ActiveBoardMissions;

                    if (allMissions.Count == 0)
                    {
                        args.Player.SendInfoMessage(Missionsi18n.GetString("NoMissions"));
                        return;
                    }

                    int maxPages = (int)Math.Ceiling((double)allMissions.Count / missionsPerPage);
                    if (page < 1) page = 1;
                    if (page > maxPages) page = maxPages;

                    args.Player.SendMessage(Missionsi18n.GetString("BoardTitle", page, maxPages), 255, 215, 0);

                    int startIndex = (page - 1) * missionsPerPage;
                    int endIndex = Math.Min(startIndex + missionsPerPage, allMissions.Count);

                    int totalMissions = MissionsJSON.MissionsList.Count;
                    var lifetimeCompleted = new System.Collections.Generic.List<string>();

                    using (var reader = MissionsDB.Db.QueryReader("SELECT MissionName FROM LifetimeMissionsCompleted WHERE AccountName = @0", args.Player.Account.Name))
                    {
                        while (reader.Read()) lifetimeCompleted.Add(reader.Get<string>("MissionName"));
                    }

                    int percentage = totalMissions > 0 ? (lifetimeCompleted.Count * 100) / totalMissions : 0;

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        var m = allMissions[i];
                        string limitText = m.GlobalQuant ? Missionsi18n.GetString("BoardLimitGlobal") : Missionsi18n.GetString("BoardLimitPersonal");

                        string typeFormat = m.TypeMission.ToUpper();
                        string objFormat = m.MissionObj;

                        if (m.TypeMission.ToLower() == "mine" || m.TypeMission.ToLower() == "collect")
                        {
                            string[] objP = m.MissionObj.Split(':');
                            int itemId = int.TryParse(objP[0], out int parsedId) ? parsedId : 0;
                            int amount = objP.Length > 1 ? int.Parse(objP[1]) : 1;

                            typeFormat = (m.TypeMission.ToLower() == "mine") ? Missionsi18n.GetString("TypeFormatMine") : Missionsi18n.GetString("TypeFormatCollect");
                            objFormat = $"[i/s{amount}:{itemId}]";
                        }
                        else if (m.TypeMission.ToLower() == "kill")
                        {
                            string[] objP = m.MissionObj.Split(':');
                            string enemy = objP[0];
                            string amount = objP.Length > 1 ? objP[1] : "1";

                            typeFormat = Missionsi18n.GetString("TypeFormatKill");

                            if (int.TryParse(enemy, out int npcId))
                                objFormat = $"{Lang.GetNPCNameValue(npcId)} x{amount}";
                            else
                                objFormat = $"{enemy} x{amount}";
                        }
                        else if (m.TypeMission.ToLower() == "find")
                        {
                            typeFormat = Missionsi18n.GetString("TypeFormatFind");
                            if (int.TryParse(m.MissionObj, out int npcId))
                                objFormat = Lang.GetNPCNameValue(npcId);
                            else
                                objFormat = m.MissionObj;
                        }

                        string rewardFormat = "";
                        if (m.TypeReward.ToLower() == "money")
                        {
                            string[] coins = m.Reward.Split(',');
                            foreach (string coin in coins)
                            {
                                string rStr = coin.Trim().ToLower();
                                if (rStr.EndsWith("g")) rewardFormat += $"[i/s{rStr.TrimEnd('g')}:73] ";
                                else if (rStr.EndsWith("s")) rewardFormat += $"[i/s{rStr.TrimEnd('s')}:72] ";
                                else if (rStr.EndsWith("c")) rewardFormat += $"[i/s{rStr.TrimEnd('c')}:71] ";
                                else if (rStr.EndsWith("p")) rewardFormat += $"[i/s{rStr.TrimEnd('p')}:74] ";
                            }
                        }
                        else if (m.TypeReward.ToLower() == "item")
                        {
                            string[] items = m.Reward.Split(',');
                            foreach (string itm in items)
                            {
                                string[] rP = itm.Split(':');
                                int itemId = int.TryParse(rP[0].Trim(), out int parsedRewId) ? parsedRewId : 0;
                                int amount = rP.Length > 1 ? (int.TryParse(rP[1].Trim(), out int pAmt) ? pAmt : 1) : 1;
                                rewardFormat += $"[i/s{amount}:{itemId}] ";
                            }
                        }
                        else if (m.TypeReward.ToLower() == "buff")
                        {
                            string[] buffs = m.Reward.Split(',');
                            foreach (string b in buffs)
                            {
                                string[] rP = b.Split(':');
                                int buffId = int.TryParse(rP[0].Trim(), out int parsedRewId) ? parsedRewId : 0;
                                string bTime = rP.Length > 1 ? rP[1].Trim() : "";
                                rewardFormat += $"{Lang.GetBuffName(buffId)} ({bTime}) ";
                            }
                        }
                        rewardFormat = rewardFormat.TrimEnd();

                        bool isMaxedOut = false;
                        if (m.GlobalQuant)
                        {
                            using (var reader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM GlobalMissions WHERE MissionName = @0", m.MissionName))
                            {
                                if (reader.Read() && reader.Get<int>("CompletedCount") >= m.Quant) isMaxedOut = true;
                            }
                        }
                        else
                        {
                            using (var reader = MissionsDB.Db.QueryReader("SELECT CompletedCount FROM PlayerMissionsCompleted WHERE AccountName = @0 AND MissionName = @1", args.Player.Account.Name, m.MissionName))
                            {
                                if (reader.Read() && reader.Get<int>("CompletedCount") >= m.Quant) isMaxedOut = true;
                            }
                        }

                        string statusText = lifetimeCompleted.Contains(m.MissionName) ? Missionsi18n.GetString("StatusRepeated") : Missionsi18n.GetString("StatusNew");

                        args.Player.SendInfoMessage(Missionsi18n.GetString("MissionStatus", statusText, percentage));

                        int filledStars = 1;
                        bool isArcane = false;

                        if (m.Rarity >= 95) { filledStars = 5; isArcane = true; }
                        else if (m.Rarity >= 85) { filledStars = 5; }
                        else if (m.Rarity >= 65) { filledStars = 4; }
                        else if (m.Rarity >= 40) { filledStars = 3; }
                        else if (m.Rarity >= 20) { filledStars = 2; }
                        else { filledStars = 1; }

                        string rarityStars = "";
                        if (isArcane)
                        {
                            for (int s = 0; s < 5; s++) rarityStars += "[i:5339]";
                        }
                        else
                        {
                            for (int s = 0; s < filledStars; s++) rarityStars += "[i:75]";
                            for (int s = 0; s < 5 - filledStars; s++) rarityStars += "[i:109]";
                        }

                        if (isMaxedOut)
                        {
                            args.Player.SendMessage(Missionsi18n.GetString("MissionExhausted", m.MissionName), 80, 80, 80);
                            args.Player.SendMessage($"{m.MissionDescription}", 80, 80, 80);
                            args.Player.SendMessage($"{typeFormat}: {objFormat}", 80, 80, 80);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardTime", m.Time), 80, 80, 80);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardReward", rewardFormat), 80, 80, 80);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardAvailable", "0", limitText), 80, 80, 80);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardRarity", rarityStars), 80, 80, 80);
                            args.Player.SendMessage($"▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬", 80, 80, 80);
                        }
                        else
                        {
                            args.Player.SendMessage($"{m.MissionName}", 255, 215, 0);
                            args.Player.SendMessage($"{m.MissionDescription}", 255, 255, 255);
                            args.Player.SendMessage($"{typeFormat}: {objFormat}", 200, 200, 200);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardTime", m.Time), 86, 86, 255);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardReward", rewardFormat), 100, 255, 100);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardAvailable", m.Quant, limitText), 200, 200, 200);
                            args.Player.SendMessage(Missionsi18n.GetString("BoardRarity", rarityStars), 200, 200, 200);
                            args.Player.SendMessage($"▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬", 80, 80, 80);
                        }
                    }

                    if (page < maxPages)
                    {
                        args.Player.SendInfoMessage(Missionsi18n.GetString("BoardNextPage", page + 1));
                    }
                    break;
            }
        }
    }
}
