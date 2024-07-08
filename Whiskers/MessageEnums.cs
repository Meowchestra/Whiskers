/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Collections.ObjectModel;

namespace Whiskers;

public enum MessageType
{
    None                  = 0,
    Handshake             = 1,
    Version               = 2,

    NameAndHomeWorld      = 5, //Get

    Instrument            = 20,
    NoteOn                = 21,
    NoteOff               = 22,
    ProgramChange         = 23,

    StartEnsemble         = 30, //Get<->Set
    AcceptReply           = 31,
    PerformanceModeState  = 32, //Get

    PartyInvite           = 40, //Set           (name;HomeWorldId)
    PartyInviteAccept     = 41,
    PartyPromote          = 42, //Set
    PartyEnterHouse       = 43,

    SetGfx                = 50, //Get<->Set
    MasterSoundState      = 60, //Set<->Get
    MasterVolume          = 61, //Set<->Get
    BgmSoundState         = 62, //Set<->Get
    BgmVolume             = 63, //Set<->Get
    EffectsSoundState     = 64, //Set<->Get
    EffectsVolume         = 65, //Set<->Get
    VoiceSoundState       = 66, //Set<->Get
    VoiceVolume           = 67, //Set<->Get
    AmbientSoundState     = 68, //Set<->Get
    AmbientVolume         = 69, //Set<->Get
    SystemSoundState      = 70, //Set<->Get
    SystemVolume          = 71, //Set<->Get
    PerformanceSoundState = 72, //Set<->Get
    PerformanceVolume     = 73, //Set<->Get

    Chat                  = 85,
    NetworkPacket         = 90,
    ExitGame              = 100
}

public readonly struct ChatMessageChannelType
{
    public static readonly  ChatMessageChannelType None        = new("None",         0x0000, "");
    private static readonly ChatMessageChannelType Say         = new("Say",          0x000A, "/s");
    private static readonly ChatMessageChannelType Yell        = new("Yell",         0x001E, "/y");
    private static readonly ChatMessageChannelType Shout       = new("Shout",        0x000B, "/sh");
    private static readonly ChatMessageChannelType Party       = new("Party",        0x000E, "/p");
    private static readonly ChatMessageChannelType FreeCompany = new("Free Company", 0x0018, "/fc");

    private static readonly IReadOnlyList<ChatMessageChannelType> All = new ReadOnlyCollection<ChatMessageChannelType>(new List<ChatMessageChannelType>
    {
        None,
        Say,
        Yell,
        Shout,
        Party,
        FreeCompany
    });

    private string Name { get; }
    private int ChannelCode { get; }
    public string ChannelShortCut { get; }

    private ChatMessageChannelType(string name, int channelCode, string channelShortCut)
    {
        Name            = name;
        ChannelCode     = channelCode;
        ChannelShortCut = channelShortCut;
    }

    public static ChatMessageChannelType ParseByChannelCode(int channelCode)
    {
        var _ = TryParseByChannelCode(channelCode, out var result);
        return result;
    }

    private static bool TryParseByChannelCode(int channelCode, out ChatMessageChannelType result)
    {
        if (All.Any(x => x.ChannelCode.Equals(channelCode)))
        {
            result = All.First(x => x.ChannelCode.Equals(channelCode));
            return true;
        }
        result = None;
        return false;
    }
}