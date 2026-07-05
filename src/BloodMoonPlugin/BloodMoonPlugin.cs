using System.IO;
using PluginManager.Api;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Contracts;
using PluginManager.Api.Hooks;
using PluginManager.Config;
using PluginManager.Localization;

namespace BloodMoonPlugin;

public class BloodMoonPlugin : BasePlugin
{
    public override string ModuleName => "BloodMoonPlugin";
    public override string ModuleVersion => "1.1.1";
    public override string ModuleAuthor => "kotfoxtrot";
    public override string ModuleDescription => "Shows how many days are left until the next Blood Moon.";

    private IPlayerLocalization _localization;
    private IPlayerUtil _playerUtil;
    private IGameUtil _gameUtil;
    private IGameStatsUtil _statsUtil;
    private PluginConfig _config;

    private int _lastNotifiedDay = -1;

    protected override void OnLoad()
    {
        _localization = GetPlayerLocalization();
        _playerUtil = Capabilities.Get<IPlayerUtil>();
        _statsUtil = Capabilities.Get<IGameStatsUtil>();
        _gameUtil = Capabilities.Get<IGameUtil>();
        _config = ReadPluginConfig();

        RegisterCommand("bm", "Shows the number of days until the next Blood Moon", OnBloodMoon);
        RegisterEventHandler<GameUpdateEvent>(OnGameUpdate, HookMode.Post);
    }

    private void OnBloodMoon(ICommandContext ctx)
    {
        var bloodMoonDay = _statsUtil.GetInt(GameStats.BloodMoonDay);

        if (bloodMoonDay <= 0)
        {
            Reply(ctx.ClientInfo, "Disabled");
            return;
        }

        if (_gameUtil.IsBloodMoonActive())
        {
            Reply(ctx.ClientInfo, "Active");
            return;
        }

        var daysLeft = bloodMoonDay - _gameUtil.WorldTimeToDays(_gameUtil.GetWorldTime());

        if (daysLeft <= 0)
        {
            Reply(ctx.ClientInfo, "Coming tonight");
            return;
        }

        Reply(ctx.ClientInfo, "Days left", daysLeft);
    }

    private HookResult OnGameUpdate(GameUpdateEvent evt)
    {
        var bloodMoonDay = _statsUtil.GetInt(GameStats.BloodMoonDay);

        if (bloodMoonDay <= 0)
            return HookResult.Continue;

        var worldTime = _gameUtil.GetWorldTime();

        if (_gameUtil.WorldTimeToHours(worldTime) != _config.NotifyHour)
            return HookResult.Continue;

        var day = _gameUtil.WorldTimeToDays(worldTime);

        if (day == _lastNotifiedDay)
            return HookResult.Continue;

        _lastNotifiedDay = day;

        var daysLeft = bloodMoonDay - day;

        if (daysLeft <= 0)
        {
            Broadcast("Coming tonight");
        }
        else if (_config.NotifyEveryDays > 0 && daysLeft % _config.NotifyEveryDays == 0)
        {
            Broadcast("Days left", daysLeft);
        }

        return HookResult.Continue;
    }

    private void Broadcast(string key, params object[] args)
    {
        foreach (var clientInfo in _playerUtil.GetClientInfoList())
        {
            Reply(clientInfo, key, args);
        }
    }

    private void Reply(ClientInfo client, string key, params object[] args)
    {
        var tag = _localization.Translate(client.CrossplatformId, "Tag");
        var text = _localization.Translate(client.CrossplatformId, key, args);
        _playerUtil.PrintToChat(client.EntityId, $"{tag}{text}");
    }

    private PluginConfig ReadPluginConfig()
    {
        return new JsonConfigReader().Read<PluginConfig>(Path.Combine(ModulePath, "config.json"));
    }

    private IPlayerLocalization GetPlayerLocalization()
    {
        var playerLanguageStore = Capabilities.Get<IPlayerLanguageStore>();
        return new JsonPlayerLocalizationFactory(playerLanguageStore)
            .Create(Path.Combine(ModulePath, "lang"));
    }
}