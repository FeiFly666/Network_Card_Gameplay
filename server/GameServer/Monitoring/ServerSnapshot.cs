using Common.Model;
using GameServer.Model;
using GameServer.Network;

namespace GameServer.Monitoring;

internal sealed record ServerSnapshot(
    DateTimeOffset CapturedAtUtc,
    OnlinePlayerSnapshot[] OnlinePlayers,
    BattleRoomSnapshot[] BattleRooms);

internal sealed record OnlinePlayerSnapshot(
    string UserId, string? Nickname, string? MainCharacterId, int ClientId);

internal sealed record BattleRoomSnapshot(
    int Id, int Round, string Phase, bool Started, BattlePlayerSnapshot[] Players);

internal sealed record BattlePlayerSnapshot(
    string UserId, string? Nickname, bool Loaded, int HandCount, CharacterSnapshot[] Characters);

internal sealed record CharacterSnapshot(
    int Id, string DataId, string? Name, bool IsMain, int Hp, int MaxHp,
    int Mp, int Attack, int Defense, int Speed, string[] Buffs);

internal static class MonitorSnapshotBuilder
{
    internal static OnlinePlayerSnapshot[] CaptureOnlinePlayers(KeyValuePair<string, ClientSession>[] sessions)
    {
        var players = new List<OnlinePlayerSnapshot>(sessions.Length);
        foreach (var (userId, session) in sessions)
        {
            if (!session.IsClosed && session.player is { } player)
                players.Add(new OnlinePlayerSnapshot(userId, player.nickname, player.mainCharcterID, session.ClientID));
        }

        return players.ToArray();
    }

    internal static BattleRoomSnapshot[] CaptureBattleRooms(IEnumerable<BattleRoom> rooms)
    {
        return rooms.Select(room => new BattleRoomSnapshot(
            room.Id,
            room.round,
            room.stateMachine.CurrentState.stateType.ToString(),
            room.isStarted,
            room.players.Select(entry => CaptureBattlePlayer(room, entry.Key, entry.Value)).ToArray())).ToArray();
    }

    private static BattlePlayerSnapshot CaptureBattlePlayer(BattleRoom room, string userId, Player player)
    {
        return new BattlePlayerSnapshot(
            userId,
            player.nickname,
            room.loadedPlayers.Contains(userId),
            player.battlePlayer.cards.Count,
            player.battlePlayer.characters.Values.Select(CaptureCharacter).ToArray());
    }

    private static CharacterSnapshot CaptureCharacter(Character character)
    {
        var buffs = character.Buffs.Select(buff => buff.Data.Name ?? buff.Data.DataID).ToArray();
        return new CharacterSnapshot(
            character.ID,
            character.data.DataID,
            character.data.Name,
            character.isMain,
            character.CurHP,
            character.CurMaxHP,
            character.CurMP,
            character.CurATK,
            character.CurDEF,
            character.CurSpeed,
            buffs);
    }
}
