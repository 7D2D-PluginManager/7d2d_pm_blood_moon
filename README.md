# BloodMoonPlugin

- Chat command `/bm` — tells the player how many days are left until the next Blood Moon.
- Automatic server-wide announcement once per game day at a configured game-time hour.

If the Blood Moon falls on the current night, the message becomes **"They are coming tonight!"**.

Reads Blood Moon state through the `IGameUtil` provider (`GetBloodMoonDay`, `GetCurrentDay`,
`GetCurrentHour`, `IsBloodMoonActive`); the plugin never touches the game directly. The periodic
check is driven by `GameUpdateEvent`, published each frame by the PluginManager core.

## Config (`config.json`)

| Key | Description |
| --- | --- |
| `NotifyEveryDays` | Announce when the number of days left is a multiple of this value (`1` = every day). |
| `NotifyHour` | Game-time hour (0–23) at which the daily announcement is sent. |

Localization: `static/lang/en.json`, `static/lang/ru.json`.
