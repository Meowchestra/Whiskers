/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Text.RegularExpressions;

namespace Whiskers.Utils;

public static class LangStrings
{
    public static readonly List<Regex> LfgPatterns =
    [
        new(@"Join .* party\?"),
        new(@".*のパーティに参加します。よろしいですか？"),
        new(@"Der Gruppe von .* beitreten\?"),
        new(@"Rejoindre l'équipe de .*\?")
    ];

    public static readonly List<Regex> LeavePartyPatterns =
    [
        new(@"Die Gruppe verlassen\?"),
        new(@"Leave party\?")
    ];

    public static readonly List<Regex> PromotePatterns =
    [
        new(@"Promote .* to party leader\?"),
        new(@".* zum Gruppenanführer machen\?"),
        new(@"Promouvoir .* au rang de chef d'équipe \?")
    ];

    internal static readonly List<Regex> Entrance =
    [
        new("ハウスへ入る"),
        new("进入房屋"),
        new("進入房屋"),
        new("Eingang"),
        new("Entrance"),
        new("Entrée")
    ];

    internal static readonly List<Regex> ConfirmHouseEntrance =
    [
        new(@"「ハウス」へ入りますか\？"),
        new(@"要进入这间房屋吗\？"),
        new(@"要進入這間房屋嗎\？"),
        new(@"Das Gebäude betreten\?"),
        new(@"Entrer dans la maison \?"),
        new(@"Enter the estate hall\?")
    ];

    internal static readonly List<Regex> ConfirmGroupTeleport =
    [
        new(@"Accept Teleport to .*\?"),
        new(@"Zum Ätheryten .* teleportieren lassen\?"),
        new(@"Voulez-vous vous téléporter vers la destination .* \?")
    ];

    internal static readonly List<Regex> ConfirmLogout =
    [
        new(@"Zum Titelbildschirm zurückkehren\?"),
        new(@"Se déconnecter et retourner à l'écram titre \?"),
        new(@"Log out and return to the title screen\?")
    ];

    internal static readonly List<Regex> ConfirmShutdown =
    [
        new(@"Das Spiel beenden\?"),
        new(@"Se déconnecter et quitter le jeu \?"),
        new(@"Log out and exit the game\?")
    ];
}