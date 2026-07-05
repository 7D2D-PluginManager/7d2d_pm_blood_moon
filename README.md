# BloodMoonPlugin

Chat command `/bm` — tells players how many days are left until the next Blood Moon.

Reads the next Blood Moon day through the `IGameUtil` provider (`GetBloodMoonDay`, `GetCurrentDay`, `IsBloodMoonActive`); the plugin never touches the game directly.

Localization: `static/lang/en.json`, `static/lang/ru.json`.
