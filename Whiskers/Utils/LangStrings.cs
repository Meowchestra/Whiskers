/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Text.RegularExpressions;

namespace Whiskers.Utils;

public static class LangStrings
{
    public static readonly List<Regex> LfgPatterns =
    [
        new Regex(@"Join .* party\?"),
        new Regex(@".*のパーティに参加します。よろしいですか？"),
        new Regex(@"Der Gruppe von .* beitreten\?"),
        new Regex(@"Rejoindre l'équipe de .*\?")
    ];

    public static readonly List<Regex> PromotePatterns =
    [
        new Regex(@"Promote .* to party leader\?"),
        new Regex(@".* zum Gruppenanführer machen\?")
    ];

    internal static readonly List<Regex> Entrance =
    [
        new Regex(@"ハウスへ入る"),
        new Regex(@"进入房屋"),
        new Regex(@"進入房屋"),
        new Regex(@"Eingang"),
        new Regex(@"Entrée"),
        new Regex(@"Entrance")
    ];

    internal static readonly List<Regex> ConfirmHouseEntrance =
    [
        new Regex(@"「ハウス」へ入りますか\？"),
        new Regex(@"要进入这间房屋吗\？"),
        new Regex(@"要進入這間房屋嗎\？"),
        new Regex(@"Das Gebäude betreten\?"),
        new Regex(@"Entrer dans la maison \?"),
        new Regex(@"Enter the estate hall\?")
    ];

    internal static readonly List<Regex> ConfirmGroupTeleport =
    [
        new Regex(@"Accept Teleport to .*\？"),
        new Regex(@"Zum Ätheryten .* teleportieren lassen\?")
    ];
}