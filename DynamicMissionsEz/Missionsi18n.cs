using System.Collections.Generic;

namespace DynamicMissionsEz
{
    public static class Missionsi18n
    {
        public static string CurrentLang => MissionsJSON.Config.DefaultLang;

        private static readonly Dictionary<string, Dictionary<string, string>> Texts = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "es-ES", new Dictionary<string, string>
                {
                    // --- Generales ---
                    { "NotLoggedIn", "Debes iniciar sesión para usar el sistema de misiones." },
                    { "NotInGuild", "Debes estar en un gremio de aventureros para ver o tomar misiones." },
                    { "GuildWelcome", "Estás en la zona del Gremio. Usa /mission para ver el tablero." },
                    { "GuildFloating", "¡Gremio de Aventureros!" },

                    // --- Tablero de Misiones ---
                    { "BoardTitle", "[c/808080:▬▬▬Tablero de Misiones (Pág {0}/{1})▬▬▬]" },
                    { "NoMissions", "No hay misiones disponibles en este momento." },
                    { "MissionStatus", "   ({0} - Gremio completado {1}%)" },
                    { "StatusRepeated", "Repetida" },
                    { "StatusNew", "Nueva" },
                    { "MissionExhausted", "{0} (AGOTADA)" },
                    { "BoardTime", "[i:3099]Tiempo: {0}" },
                    { "BoardReward", "[i:855]Recompensa: {0}" },
                    { "BoardAvailable", "{0} disponible {1}" },
                    { "BoardLimitGlobal", "(global)" },
                    { "BoardLimitPersonal", "(para ti)" },
                    { "BoardRarity", "Rareza: {0}" },
                    { "BoardNextPage", "Escribe /mission {0} para ver la siguiente página." },

                    // --- Menú de Recompensas ---
                    { "NoRewards", "No tienes recompensas pendientes por reclamar." },
                    { "PendingTitle", "=== Tus Recompensas Pendientes ===" },
                    { "RewardFormat", "{0}. Tipo: {1} - Disponible: {2}" },
                    { "RewardSelect", "Escribe /mission reward <número> para reclamar una." },
                    { "RewardInvalid", "Número inválido. Tienes {0} recompensas pendientes." },
                    { "TypeMoney", "Dinero" },
                    { "TypeItem", "Objeto" },
                    { "TypeBuff", "Buff" },

                    // --- Base de Datos ---
                    { "LogDbLoaded", "[DynamicMissionsEz] Base de datos SQLite cargada correctamente." },
                    { "LogDbError", "[DynamicMissionsEz] Error al crear la base de datos: {0}" },

                    // --- JSON ---
                    { "LogJsonError", "[DynamicMissionsEz] ERROR GRAVE AL CARGAR JSON: {0}. ¡Revisa que no te falten comas o comillas en missionslist.json!" },

                    // --- Misc & Recompensas ---
                    { "RewardMoney", "Has reclamado tu recompensa de misión: dinero." },
                    { "RewardItem", "Has reclamado tu recompensa de misión: objeto(s)." },
                    { "RewardBuff", "Has recibido el buff(s) de tu misión." },
                    { "RewardError", "Hubo un error al reclamar esta recompensa. Contacta a un administrador." },
                    { "TimeExpired", "¡El tiempo para la misión '{0}' se ha agotado y ha sido cancelada!" },
                    { "BoardRefreshed", "¡El Gremio ha publicado nuevas misiones! Usa /mission para ver el tablero actualizado." },
                    { "MasteryAchieved", "{0} ha completado el gremio al 100%!!!" },

                    // --- Logs de Consola ---
                    { "LogNpcTeleport", "[DynamicMissionsEz] NPC existente teletransportado a X:{0} Y:{1}" },
                    { "LogNpcSpawn", "[DynamicMissionsEz] NPC de misión generado en X:{0} Y:{1}" },
                    { "LogRewardError", "[DynamicMissionsEz] Error al procesar recompensa '{0}' para {1}: {2}" },
                    { "LogTimeError", "[DynamicMissionsEz] Error en el Sistema de Tiempo: {0}" },
                    { "LogPartyError", "[DynamicMissionsEz] Error en la celebración: {0}" },
                    { "LogAcceptError", "[DynamicMissionsEz] Error al aceptar misión: {0}" },
                    { "LogLanguageError", "[DynamicMissionsEz] El idioma '{0}' no es válido. Usa formatos como es-ES o en-US. Error: {1}" },

                    // --- Gameplay (MissionsTypes) ---
                    { "KillGuildSuccess", "¡Has derrotado a todos los objetivos de '{0}'! Ve al gremio a entregar tu misión." },
                    { "MissionSuccess", "¡Misión '{0}' cumplida! Usa /mission reward para reclamar tu premio." },
                    { "KillProgress", "Progreso de misión '{0}': {1}/{2}" },
                    { "TileCheat", "Bloque no elegible para la misión" },
                    { "MineGuildSuccess", "¡Has recolectado todo para '{0}'! Usa /mission deliver para entregar los objetos y completar la misión." },
                    { "MineProgress", "Progreso de '{0}': {1}/{2}" },
                    { "FindGuildSuccess", "¡Has encontrado al objetivo! Ahora debes ir al gremio de aventureros a entregar la misión." },

                    // --- Admin Commands ---
                    { "CmdCreateGuildUsage", "Uso: /createguild set 1 | /createguild set 2 | /createguild define <Nombre>" },
                    { "CmdCreateGuildP1", "Golpea un bloque para establecer el punto 1 de la región del gremio." },
                    { "CmdCreateGuildP2", "Golpea un bloque para establecer el punto 2 de la región del gremio." },
                    { "CmdCreateGuildErrPoints", "Debes establecer los puntos 1 y 2 primero (/createguild set 1|2)." },
                    { "CmdCreateGuildSuccess", "Se ha creado la región '{0}' y se ha marcado como Gremio de Aventureros." },
                    { "CmdCreateGuildErrFail", "No se pudo crear la región '{0}'. Quizás ya existe o el nombre es inválido." },
                    { "CmdAssignGuildUsage", "Uso: /assignguild <NombreDeLaRegionExistente>" },
                    { "CmdAssignGuildErrNotFound", "No se encontró ninguna región llamada '{0}' en el servidor." },
                    { "CmdAssignGuildSuccess", "La región '{0}' ha sido asignada como Gremio de Aventureros." },
                    { "CmdAssignGuildErrAlready", "La región '{0}' ya estaba asignada como gremio." },
                    { "CmdRemoveGuildUsage", "Uso: /removeguild <NombreDeRegion>" },
                    { "CmdRemoveGuildSuccess", "La región '{0}' ya no es un Gremio (La región sigue existiendo)." },
                    { "CmdRemoveGuildErrNotGuild", "La región '{0}' no era un Gremio." },
                    { "CmdDeleteGuildUsage", "Uso: /deleteguild <NombreDeRegion>" },
                    { "CmdDeleteGuildSuccess", "El Gremio y la región '{0}' han sido eliminados por completo." },
                    { "CmdDeleteGuildErrFail", "No se pudo eliminar la región '{0}' de TShock." },
                    { "CmdReloadSuccess", "[DynamicMissionsEz] Configuración, misiones y temporizador recargados exitosamente." },
                    { "CmdReloadError", "[DynamicMissionsEz] Hubo un error al recargar las misiones. Revisa la consola del servidor." },

                    // --- User Commands ---
                    { "AntiTpFind", "El Gremio prohíbe el uso de magia de teletransporte (/tpnpc) mientras buscas a un objetivo." },
                    { "CmdAcceptUsage", "Uso: /mission accept Nombre de la Misión" },
                    { "CmdErrNotFound", "No se encontró ninguna misión que coincida con '{0}'." },
                    { "CmdErrMultipleMatches", "Se encontraron varias misiones: {0}. Por favor, sé más específico." },
                    { "CmdAcceptErrAlreadyActive", "Ya tienes esta misión activa y en progreso." },
                    { "CmdAcceptErrMaxReached", "Ya tienes el máximo de misiones activas ({0}). Termina o cancela una primero." },
                    { "CmdAcceptErrGlobalMax", "Esta misión global ya ha sido completada por otros jugadores y no está disponible." },
                    { "CmdAcceptErrPersonalMax", "Ya has completado esta misión el máximo de veces permitido para ti." },
                    { "CmdAcceptSuccess", "¡Has aceptado la misión: {0}!" },
                    { "CmdAcceptObjective", "Objetivo: {0}" },
                    { "CmdAcceptFindSpawn", "¡El objetivo ha aparecido en algún lugar del mundo!" },
                    { "CmdAcceptErrInternal", "Hubo un error interno al intentar aceptar la misión." },
                    { "CmdDeliverUsage", "Uso: /mission deliver Nombre de la Misión" },
                    { "CmdDeliverErrNotActive", "No tienes ninguna misión activa que coincida con '{0}'." },
                    { "CmdDeliverErrNoConfig", "Esta misión ya no existe en la configuración." },
                    { "CmdDeliverErrNotInGuild", "Debes ir a un gremio de aventureros para entregar la misión." },
                    { "CmdDeliverErrNotEnoughMine", "Aún no has picado/recolectado lo suficiente. Llevas {0}/{1}." },
                    { "CmdDeliverErrNoItems", "No tienes suficientes objetos en tu inventario. Necesitas {0} en total para entregar." },
                    { "CmdDeliverErrNotEnoughKill", "Aún no has completado los objetivos de esta misión." },
                    { "CmdCancelUsage", "Uso: /mission cancel Nombre de la Misión" },
                    { "CmdCancelActiveList", "Tus misiones activas: {0}" },
                    { "CmdCancelNoActive", "Actualmente no tienes misiones activas." },
                    { "CmdCancelSuccess", "Has cancelado la misión '{0}'. Ya puedes tomar otra en su lugar." },
                    { "CmdCancelErrFail", "Hubo un error al intentar cancelar la misión." },
                    { "CmdUnknown", "Comando no reconocido. Usa /mission o /mission <página>." },

                    // --- Board Format ---
                    { "TypeFormatMine", "PICA Y TRAE" },
                    { "TypeFormatCollect", "RECOLECTA" },
                    { "TypeFormatKill", "ELIMINA" },
                    { "TypeFormatFind", "ENCUENTRA A" },

                    // --- Extra ---
                    { "PluginDesc", "Sistema de misiones personalizadas con recompensas y gremios." },
                    { "LogReloadError", "[DynamicMissionsEz] Error al recargar el JSON: {0}" }
                }
            },
            {
                "en-US", new Dictionary<string, string>
                {
                    // --- General ---
                    { "NotLoggedIn", "You must be logged in to use the mission system." },
                    { "NotInGuild", "You must be in an adventurer's guild to view or take missions." },
                    { "GuildWelcome", "You are in the Guild area. Type /mission to view the board." },
                    { "GuildFloating", "Adventurer's Guild!" },

                    // --- Mission Board ---
                    { "BoardTitle", "[c/808080:▬▬▬Mission Board (Page {0}/{1})▬▬▬]" },
                    { "NoMissions", "There are no missions available right now." },
                    { "MissionStatus", "   ({0} - Guild completed {1}%)" },
                    { "StatusRepeated", "Repeated" },
                    { "StatusNew", "New" },
                    { "MissionExhausted", "{0} (EXHAUSTED)" },
                    { "BoardTime", "[i:3099]Time: {0}" },
                    { "BoardReward", "[i:855]Reward: {0}" },
                    { "BoardAvailable", "{0} available {1}" },
                    { "BoardLimitGlobal", "(global)" },
                    { "BoardLimitPersonal", "(for you)" },
                    { "BoardRarity", "Rarity: {0}" },
                    { "BoardNextPage", "Type /mission {0} to see the next page." },

                    // --- Rewards Menu ---
                    { "NoRewards", "You have no pending rewards to claim." },
                    { "PendingTitle", "=== Your Pending Rewards ===" },
                    { "RewardFormat", "{0}. Type: {1} - Available: {2}" },
                    { "RewardSelect", "Type /mission reward <number> to claim one." },
                    { "RewardInvalid", "Invalid number. You have {0} pending rewards." },
                    { "TypeMoney", "Money" },
                    { "TypeItem", "Item" },
                    { "TypeBuff", "Buff" },

                    // --- Database ---
                    { "LogDbLoaded", "[DynamicMissionsEz] SQLite database loaded successfully." },
                    { "LogDbError", "[DynamicMissionsEz] Error creating database: {0}" },

                    // --- JSON ---
                    { "LogJsonError", "[DynamicMissionsEz] CRITICAL ERROR LOADING JSON: {0}. Check for missing commas or quotes in missionslist.json!" },

                    // --- Misc & Rewards ---
                    { "RewardMoney", "You have claimed your mission reward: money." },
                    { "RewardItem", "You have claimed your mission reward: item(s)." },
                    { "RewardBuff", "You have received your mission buff(s)." },
                    { "RewardError", "There was an error claiming this reward. Contact an administrator." },
                    { "TimeExpired", "Time for the mission '{0}' has run out and it has been canceled!" },
                    { "BoardRefreshed", "The Guild has posted new missions! Use /mission to view the updated board." },
                    { "MasteryAchieved", "{0} has completed the guild 100%!!!" },

                    // --- Console Logs ---
                    { "LogNpcTeleport", "[DynamicMissionsEz] Existing NPC teleported to X:{0} Y:{1}" },
                    { "LogNpcSpawn", "[DynamicMissionsEz] Mission NPC spawned at X:{0} Y:{1}" },
                    { "LogRewardError", "[DynamicMissionsEz] Error processing reward '{0}' for {1}: {2}" },
                    { "LogTimeError", "[DynamicMissionsEz] Error in Time System: {0}" },
                    { "LogPartyError", "[DynamicMissionsEz] Error in celebration: {0}" },
                    { "LogAcceptError", "[DynamicMissionsEz] Error accepting mission: {0}" },
                    { "LogLanguageError", "[DynamicMissionsEz] The language '{0}' is invalid. Use formats like es-ES or en-US. Error: {1}" },

                    // --- Gameplay (MissionsTypes) ---
                    { "KillGuildSuccess", "You have defeated all targets for '{0}'! Go to the guild to deliver your mission." },
                    { "MissionSuccess", "Mission '{0}' accomplished! Use /mission reward to claim your prize." },
                    { "KillProgress", "Mission '{0}' progress: {1}/{2}" },
                    { "TileCheat", "Block not eligible for mission" },
                    { "MineGuildSuccess", "You have collected everything for '{0}'! Use /mission deliver to turn in the items and complete the mission." },
                    { "MineProgress", "'{0}' progress: {1}/{2}" },
                    { "FindGuildSuccess", "You have found the target! Now you must go to the adventurer's guild to deliver the mission." },

                    // --- Admin Commands ---
                    { "CmdCreateGuildUsage", "Usage: /createguild set 1 | /createguild set 2 | /createguild define <Name>" },
                    { "CmdCreateGuildP1", "Hit a block to set point 1 of the guild region." },
                    { "CmdCreateGuildP2", "Hit a block to set point 2 of the guild region." },
                    { "CmdCreateGuildErrPoints", "You must set points 1 and 2 first (/createguild set 1|2)." },
                    { "CmdCreateGuildSuccess", "Region '{0}' has been created and marked as an Adventurer's Guild." },
                    { "CmdCreateGuildErrFail", "Could not create region '{0}'. It might already exist or the name is invalid." },
                    { "CmdAssignGuildUsage", "Usage: /assignguild <ExistingRegionName>" },
                    { "CmdAssignGuildErrNotFound", "Could not find any region named '{0}' on the server." },
                    { "CmdAssignGuildSuccess", "Region '{0}' has been assigned as an Adventurer's Guild." },
                    { "CmdAssignGuildErrAlready", "Region '{0}' was already assigned as a guild." },
                    { "CmdRemoveGuildUsage", "Usage: /removeguild <RegionName>" },
                    { "CmdRemoveGuildSuccess", "Region '{0}' is no longer a Guild (the region still exists)." },
                    { "CmdRemoveGuildErrNotGuild", "Region '{0}' was not a Guild." },
                    { "CmdDeleteGuildUsage", "Usage: /deleteguild <RegionName>" },
                    { "CmdDeleteGuildSuccess", "The Guild and region '{0}' have been completely deleted." },
                    { "CmdDeleteGuildErrFail", "Could not delete region '{0}' from TShock." },
                    { "CmdReloadSuccess", "[DynamicMissionsEz] Configuration, missions, and timer successfully reloaded." },
                    { "CmdReloadError", "[DynamicMissionsEz] There was an error reloading missions. Check the server console." },

                    // --- User Commands ---
                    { "AntiTpFind", "The Guild forbids the use of teleportation magic (/tpnpc) while searching for a target." },
                    { "CmdAcceptUsage", "Usage: /mission accept Mission Name" },
                    { "CmdErrNotFound", "Could not find any mission matching '{0}'." },
                    { "CmdErrMultipleMatches", "Multiple missions found: {0}. Please be more specific." },
                    { "CmdAcceptErrAlreadyActive", "You already have this mission active and in progress." },
                    { "CmdAcceptErrMaxReached", "You already have the maximum active missions ({0}). Finish or cancel one first." },
                    { "CmdAcceptErrGlobalMax", "This global mission has already been completed by other players and is not available." },
                    { "CmdAcceptErrPersonalMax", "You have already completed this mission the maximum number of times allowed for you." },
                    { "CmdAcceptSuccess", "You have accepted the mission: {0}!" },
                    { "CmdAcceptObjective", "Objective: {0}" },
                    { "CmdAcceptFindSpawn", "The target has spawned somewhere in the world!" },
                    { "CmdAcceptErrInternal", "There was an internal error trying to accept the mission." },
                    { "CmdDeliverUsage", "Usage: /mission deliver Mission Name" },
                    { "CmdDeliverErrNotActive", "You don't have any active mission matching '{0}'." },
                    { "CmdDeliverErrNoConfig", "This mission no longer exists in the configuration." },
                    { "CmdDeliverErrNotInGuild", "You must go to an adventurer's guild to deliver the mission." },
                    { "CmdDeliverErrNotEnoughMine", "You haven't mined/collected enough yet. You have {0}/{1}." },
                    { "CmdDeliverErrNoItems", "You don't have enough items in your inventory. You need {0} in total to deliver." },
                    { "CmdDeliverErrNotEnoughKill", "You haven't completed the objectives for this mission yet." },
                    { "CmdCancelUsage", "Usage: /mission cancel Mission Name" },
                    { "CmdCancelActiveList", "Your active missions: {0}" },
                    { "CmdCancelNoActive", "You currently have no active missions." },
                    { "CmdCancelSuccess", "You have canceled the mission '{0}'. You can now take another one." },
                    { "CmdCancelErrFail", "There was an error trying to cancel the mission." },
                    { "CmdUnknown", "Unknown command. Use /mission or /mission <page>." },

                    // --- Board Format ---
                    { "TypeFormatMine", "MINE AND BRING" },
                    { "TypeFormatCollect", "COLLECT" },
                    { "TypeFormatKill", "ELIMINATE" },
                    { "TypeFormatFind", "FIND" },

                    // --- Extra ---
                    { "PluginDesc", "Custom mission system with rewards and guilds." },
                    { "LogReloadError", "[DynamicMissionsEz] Error reloading JSON: {0}" }
                }
            }
        };

        public static string GetString(string key, params object[] args)
        {
            string lang = Texts.ContainsKey(CurrentLang) ? CurrentLang : "es-ES";

            if (Texts[lang].TryGetValue(key, out string text))
            {
                if (args != null && args.Length > 0)
                    return string.Format(text, args);
                return text;
            }

            return $"[{key}]";
        }
    }
}
