/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Whiskers.Offsets;

namespace Whiskers.GameFunctions;

public static class MiscGameFunctions
{
    public static void CharacterLogout()
    {
        Party.Instance.AcceptPartyInviteEnable();
        Api.Framework?.RunOnTick(delegate
        {
            Chat.SendMessage("/logout");
        }, default, 10);
    }

    public static void GameShutdown()
    {
        Party.Instance.AcceptPartyInviteEnable();
        Api.Framework?.RunOnTick(delegate
        {
            Chat.SendMessage("/shutdown");
        }, default, 10);
    }
}