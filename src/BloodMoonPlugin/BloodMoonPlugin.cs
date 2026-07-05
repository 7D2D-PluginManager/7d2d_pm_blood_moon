using System.IO;
using PluginManager.Api;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Hooks;
using PluginManager.Config;
using PluginManager.Localization;

namespace BloodMoonPlugin;

public class BloodMoonPlugin : BasePlugin
{
    public override string ModuleName => "BloodMoonPlugin";
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "kotfoxtrot";
    public override string ModuleDescription => "Shows how many days are left until the next Blood Moon.";

    private IPlayerLocalization _localization;
    private IPlayerUtil _playerUtil;
    private IGameUtil _gameUtil;
    private PluginConfig _config;

    private int _lastNotifiedDay = -1;

    protected override void OnLoad()
    {
        _localization = GetPlayerLocalization();
        _playerUtil = Capabilities.Get<IPlayerUtil>();
        _gameUtil = Capabilities.Get<IGameUtil>();
        _config = ReadPluginConfig();

        RegisterCommand("bm", "Shows the number of days until the next Blood Moon", OnBloodMoon);
        RegisterEventHandler<GameUpdateEvent>(OnGameUpdate, HookMode.Post);
    }

    private void OnBloodMoon(ICommandContext ctx)
    {
        var bloodMoonDay = _gameUtil.GetBloodMoonDay();

        if (bloodMoonDay <= 0)
        {
            Reply(ctx, "Disabled");
            return;
        }

        if (_gameUtil.IsBloodMoonActive())
        {
            Reply(ctx, "Active");
            return;
        }

        var daysLeft = bloodMoonDay - _gameUtil.GetCurrentDay();

        if (daysLeft <= 0)
        {
            Reply(ctx, "Coming tonight");
            return;
        }

        Reply(ctx, "Days left", daysLeft);
    }

    private HookResult OnGameUpdate(GameUpdateEvent evt)
    {
        var bloodMoonDay = _gameUtil.GetBloodMoonDay();
        if (bloodMoonDay <= 0)
            return HookResult.Continue;

        if (_gameUtil.GetCurrentHour() != _config.NotifyHour)
            return HookResult.Continue;

        var day = _gameUtil.GetCurrentDay();
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
            var tag = _localization.Translate(clientInfo.CrossplatformId, "Tag");
            var text = _localization.Translate(clientInfo.CrossplatformId, key, args);
            _playerUtil.PrintToChat(clientInfo.EntityId, $"{tag}{text}");
        }
    }

    private void Reply(ICommandContext ctx, string key, params object[] args)
    {
        var tag = _localization.Translate(ctx.ClientInfo.CrossplatformId, "Tag");
        var text = _localization.Translate(ctx.ClientInfo.CrossplatformId, key, args);
        _playerUtil.PrintToChat(ctx.ClientInfo.EntityId, $"{tag}{text}");
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
