using System.IO;
using PluginManager.Api;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Localization;

namespace BloodMoonPlugin;

public class BloodMoonPlugin : BasePlugin
{
    public override string ModuleName => "BloodMoonPlugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "kotfoxtrot";
    public override string ModuleDescription => "Shows how many days are left until the next Blood Moon.";

    private IPlayerLocalization _localization;
    private IPlayerUtil _playerUtil;
    private IGameUtil _gameUtil;

    protected override void OnLoad()
    {
        _localization = GetPlayerLocalization();
        _playerUtil = Capabilities.Get<IPlayerUtil>();
        _gameUtil = Capabilities.Get<IGameUtil>();

        RegisterCommand("bm", "Shows the number of days until the next Blood Moon", OnBloodMoon);
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
            Reply(ctx, "Today");
            return;
        }

        Reply(ctx, "Days left", daysLeft);
    }

    private void Reply(ICommandContext ctx, string key, params object[] args)
    {
        var tag = _localization.Translate(ctx.ClientInfo.CrossplatformId, "Tag");
        var text = _localization.Translate(ctx.ClientInfo.CrossplatformId, key, args);
        _playerUtil.PrintToChat(ctx.ClientInfo.EntityId, $"{tag}{text}");
    }

    private IPlayerLocalization GetPlayerLocalization()
    {
        var playerLanguageStore = Capabilities.Get<IPlayerLanguageStore>();
        return new JsonPlayerLocalizationFactory(playerLanguageStore)
            .Create(Path.Combine(ModulePath, "lang"));
    }
}
