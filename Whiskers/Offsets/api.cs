using System.Reflection;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// https://github.com/UnknownX7/DalamudRepoBrowser/blob/master/DalamudApi.cs

namespace Whiskers.Offsets;

public class Api
{
    [PluginService]
    public static DalamudPluginInterface? PluginInterface { get; private set; }

    [PluginService]
    public static IBuddyList? BuddyList { get; private set; }

    [PluginService]
    public static IChatGui? ChatGui { get; private set; }

    [PluginService]
    public static IClientState? ClientState { get; private set; }

    [PluginService]
    public static ICommandManager? CommandManager { get; private set; }

    [PluginService]
    public static ICondition? Condition { get; private set; }

    [PluginService] 
    public static IGameInteropProvider? GameInteropProvider { get; private set; }

    [PluginService]
    public static IDataManager? DataManager { get; private set; }

    [PluginService]
    public static IFateTable? FateTable { get; private set; }

    [PluginService]
    public static IFlyTextGui? FlyTextGui { get; private set; }

    [PluginService]
    public static IFramework? Framework { get; private set; }

    [PluginService]
    public static IGameGui? GameGui { get; private set; }

    [PluginService]
    public static IGameNetwork? GameNetwork { get; private set; }

    [PluginService]
    public static IJobGauges? JobGauges { get; private set; }

    [PluginService]
    public static IKeyState? KeyState { get; private set; }

    [PluginService]
    public static ILibcFunction? LibcFunction { get; private set; }

    [PluginService]
    public static IObjectTable? ObjectTable { get; private set; }

    [PluginService]
    public static IPartyFinderGui? PartyFinderGui { get; private set; }

    [PluginService]
    public static IPartyList? PartyList { get; private set; }

    [PluginService]
    public static ISigScanner? SigScanner { get; private set; }

    [PluginService]
    public static ITargetManager? TargetManager { get; private set; }

    [PluginService]
    public static IToastGui? ToastGui { get; private set; }

    [PluginService]
    public static IPluginLog? PluginLog { get; private set; }
    
    [PluginService] 
    public static IGameConfig? GameConfig { get; private set; }

    [PluginService]
    public static IGameLifecycle? GameLifecycle { get; private set; }

    private static PluginCommandManager<IDalamudPlugin>? _pluginCommandManager;

    public Api() { }

    public Api(IDalamudPlugin plugin) => _pluginCommandManager ??= new PluginCommandManager<IDalamudPlugin>(plugin);

    private Api(IDalamudPlugin plugin, DalamudPluginInterface? pluginInterface)
    {
        if (pluginInterface != null && !pluginInterface.Inject(this))
        {
            PluginLog?.Debug("Failed loading DalamudApi!");
            return;
        }

        _pluginCommandManager ??= new PluginCommandManager<IDalamudPlugin>(plugin);
    }

    public static Api operator +(Api container, object o)
    {
        foreach (var f in typeof(Api).GetProperties())
        {
            if (f.PropertyType != o.GetType()) continue;
            if (f.GetValue(container) != null) break;
            f.SetValue(container, o);
            return container;
        }
        throw new InvalidOperationException();
    }

    public static void Initialize(IDalamudPlugin plugin, DalamudPluginInterface? pluginInterface) => _ = new Api(plugin, pluginInterface);

    public static void Dispose() => _pluginCommandManager?.Dispose();
}

#region PluginCommandManager
public class PluginCommandManager<T> : IDisposable where T : IDalamudPlugin
{
    private readonly T _plugin;
    private readonly (string, CommandInfo)[] _pluginCommands;

    public PluginCommandManager(T plugin)
    {
        _plugin = plugin;
        _pluginCommands = _plugin.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
            .SelectMany(GetCommandInfoTuple)
            .ToArray();

        AddCommandHandlers();
    }

    private void AddCommandHandlers()
    {
        foreach (var (command, commandInfo) in _pluginCommands)
            Api.CommandManager?.AddHandler(command, commandInfo);
    }

    private void RemoveCommandHandlers()
    {
        foreach (var (command, _) in _pluginCommands)
            Api.CommandManager?.RemoveHandler(command);
    }

    private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(MethodInfo method)
    {
        var handlerDelegate = (CommandInfo.HandlerDelegate)Delegate.CreateDelegate(typeof(CommandInfo.HandlerDelegate), _plugin, method);

        var command = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>();
        var aliases = handlerDelegate.Method.GetCustomAttribute<AliasesAttribute>();
        var helpMessage = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();
        var doNotShowInHelp = handlerDelegate.Method.GetCustomAttribute<DoNotShowInHelpAttribute>();

        var commandInfo = new CommandInfo(handlerDelegate)
        {
            HelpMessage = helpMessage?.HelpMessage ?? string.Empty,
            ShowInHelp  = doNotShowInHelp == null
        };

        // Create list of tuples that will be filled with one tuple per alias, in addition to the base command tuple.
        var commandInfoTuples = new List<(string, CommandInfo)> { (command?.Command, commandInfo)! };
        if (aliases != null)
            commandInfoTuples.AddRange(aliases.Aliases.Select(alias => (alias, commandInfo)));

        return commandInfoTuples;
    }

    public void Dispose()
    {
        RemoveCommandHandlers();
        GC.SuppressFinalize(this);
    }
}
#endregion

#region Attributes
[AttributeUsage(AttributeTargets.Method)]
public abstract class AliasesAttribute : Attribute
{
    public string[] Aliases { get; }

    protected AliasesAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public abstract class CommandAttribute : Attribute
{
    public string Command { get; }

    protected CommandAttribute(string command)
    {
        Command = command;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public abstract class DoNotShowInHelpAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public abstract class HelpMessageAttribute : Attribute
{
    public string HelpMessage { get; }

    protected HelpMessageAttribute(string helpMessage)
    {
        HelpMessage = helpMessage;
    }
}
#endregion