# 🛡️ ESPAÑOL

**DynamicMissionsEz** es un plugin para servidores de **Terraria (TShock)** diseñado para añadir una capa de profundidad RPG y retención de jugadores mediante un sistema de misiones dinámicas, progresivas y altamente configurables.

### PD: Puedes ver la guía para creación de misiones aquí:
### [Guia para la creación de misiones](<Guia para la creación de misiones.md>)

## ✨ Características Principales

* **📈 Sistema de Progresión:** Rastrea el porcentaje total de misiones completadas por cada jugador. ¡Incentiva a completar el 100% del contenido!
* **🌟 Sistema de Rareza:** Las misiones aparecen con diferentes niveles de rareza, visualizados con estrellas (`[i:75]`) o Cristales Arcanos (`[i:5339]`) para misiones míticas.
* **🌍 Filtros de Mundo (Pre-HM / Hardmode):** Configura misiones exclusivas para el Hardmode o el Pre-Hardmode para mantener el balance del juego.
* **🎁 Multirecompensas:** Entrega múltiples ítems, dinero y buffs en una sola misión (ej: `"1g, 3093:5, 5:10m"`).
* **📡 Radar de Proximidad y Misiones "Find":** Sistema para encontrar NPCs generados aleatoriamente en el mundo con desaparición pacífica de 20 segundos.
* **🎆 Celebración de Maestría:** Al alcanzar el 100% de misiones, el servidor lanza fuegos artificiales, confeti y un anuncio arcoíris global.
* **🛡️ Anti-Exploit:** Validación de bloques colocados por jugadores para evitar trampas en misiones de minería.
* **📅 Rotación Automática:** El tablero de misiones se actualiza solo cada cierto tiempo (configurable).


## 🚀 Instalación

1.  Descarga el archivo `DynamicMissionsEz.dll`.
2.  Colócalo en la carpeta `ServerPlugins` de tu servidor TShock.
3.  Reinicia el servidor para generar los archivos de configuración.
4.  Configura tus misiones en `tshock/Dynamicmissions/missionslist.json`.


### 📋 Comandos del Jugador (missions.user)

| Comando | Descripción |
| :--- | :--- |
| `/mission [página]` | Abre el tablero de misiones del Gremio. |
| `/mission accept [nombre]` | Acepta una misión específica del tablero. |
| `/mission deliver` | Entrega los objetivos en el Gremio de Aventureros. |
| `/mission reward` | Abre el menú para reclamar recompensas pendientes. |
| `/mission cancel [nombre]` | Cancela una misión activa. |
| `/mission list` | Muestra tus misiones activas y su progreso. |

### 🛠️ Comandos de Administrador (missions.admin)
| Comando | Descripción |
| :--- | :--- |
|`/assignguild [región]` | Vincula una región de TShock como "Gremio de Aventureros".
|`/createguild set [1/2] [nombre]`| Crea y vincula una región a un gremio de aventureros.
|`/removeguild [región]`| Quita el gremio de aventureros de una región conservando la región.
|`/deleteguild [región]`| Quita el gremio de aventureros de una región eliminando la región.
|`/reload`| Recarga las misiones y la configuración desde el JSON.

---

### 🛠️ Configuración (`missionsconfig.json`)

```json
{
  "DefaultLang": "es-ES",           (Idiomas del Plugin disponibles: en-US y es-ES)
  "RandomMission": true,            (Determina si las misiones aparecerán de manera aleatoria basandose en su rareza)
  "ChangeMissionsTime": "1h",       (Determina cada cuanto tiempo aparecerán nuevas misiones),
  "MissionsPerPage": 3,             (Determina cuántas misiones seleccionables aparecerán por página en el chat)
  "AvailableMissionsNumber": 5,     (Determina cuantas misiones estarán disponibles)
  "NeedGuild": true,                (Determina si es necesario un gremio para tomar y entregar misiones)
  "MaxActiveMissionsPerPlayer": 3   (Determina cuantas misiones a la vez puede tomar un jugador)
}
```

---
# 🛡️ ENGLISH

**DynamicMissionsEz** is a plugin for **Terraria (TShock)** servers designed to add a layer of RPG depth and player retention through a dynamic, progressive, and highly configurable quest system.

### PS: You can check the quest creation guide here:
### [Quest creation guide](<Quest creation guide.md>)

## ✨ Key Features

* **📈 Progression System:** Tracks the total percentage of quests completed by each player. Incentivizes completing 100% of the content!
* **🌟 Rarity System:** Quests appear with different rarity tiers, displayed with stars (`[i:75]`) or Arcane Crystals (`[i:5339]`) for mythical quests.
* **🌍 World Progression Filters (Pre-HM / Hardmode):** Configure exclusive quests for Hardmode or Pre-Hardmode to maintain game balance.
* **🎁 Multi-Rewards:** Grants multiple items, coins, and buffs in a single quest (e.g., `"1g, 3093:5, 5:10m"`).
* **📡 Proximity Radar & "Find" Quests:** System to locate randomly spawned NPCs across the world with a peaceful 20-second despawn timer.
* **🎆 Mastery Celebration:** Upon reaching 100% quest completion, the server triggers fireworks, confetti, and a global rainbow broadcast.
* **🛡️ Anti-Exploit:** Validates player-placed blocks to prevent exploits in mining/harvesting quests.
* **📅 Automatic Rotation:** The quest board refreshes automatically after a set interval (configurable).


## 🚀 Installation

1. Download the `DynamicMissionsEz.dll` file.
2. Place it into the `ServerPlugins` folder of your TShock server.
3. Restart the server to generate configuration files.
4. Configure your quests in `tshock/Dynamicmissions/missionslist.json`.


### 📋 Player Commands (missions.user)

| Command | Description |
| :--- | :--- |
| `/mission [page]` | Opens the Guild quest board. |
| `/mission accept [name]` | Accepts a specific quest from the board. |
| `/mission deliver` | Turns in objectives at the Adventurer's Guild. |
| `/mission reward` | Opens the menu to claim pending rewards. |
| `/mission cancel [name]` | Cancels an active quest. |
| `/mission list` | Displays your active quests and their progress. |

### 🛠️ Admin Commands (missions.admin)

| Command | Description |
| :--- | :--- |
| `/assignguild [region]` | Links an existing TShock region as an "Adventurer's Guild". |
| `/createguild set [1/2] [name]` | Creates and links a new region as an Adventurer's Guild. |
| `/removeguild [region]` | Removes Adventurer's Guild status while keeping the region intact. |
| `/deleteguild [region]` | Removes Adventurer's Guild status and permanently deletes the region from TShock. |
| `/reload` | Reloads quests and configuration from the JSON files. |

---

### 🛠️ Configuration (`missionsconfig.json`)

```json
{
  "DefaultLang": "en-US",           (Plugin language: available options are en-US and es-ES)
  "RandomMission": true,            (Determines whether quests appear randomly based on rarity weights)
  "ChangeMissionsTime": "1h",       (Determines how often new quests refresh on the board),
  "MissionsPerPage": 3,             (Determines how many selectable quests appear per chat page)
  "AvailableMissionsNumber": 5,     (Determines how many quests will be available at once)
  "NeedGuild": true,                (Determines if being inside a guild region is required to accept and turn in quests)
  "MaxActiveMissionsPerPlayer": 3   (Determines how many concurrent quests a single player can take)
}
```
