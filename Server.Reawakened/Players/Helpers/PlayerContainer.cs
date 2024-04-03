﻿namespace Server.Reawakened.Players.Helpers;

public class PlayerContainer
{
    public static object Lock => new();

    private readonly List<Player> _playerList = [];

    public void AddPlayer(Player player)
    {
        lock (Lock)
            _playerList.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        lock (Lock)
            _playerList.Remove(player);
    }

    public List<Player> GetAllPlayers()
    {
        lock (Lock)
            return [.. _playerList];
    }

    public int GetPlayerCount()
    {
        lock (Lock)
            return _playerList.Count;
    }

    public IEnumerable<Player> GetPlayersByFriend(int friendId)
    {
        lock (Lock)
            return _playerList.ToList().Where(p => p.Character.Data.Friends.Contains(friendId));
    }

    public IEnumerable<Player> GetPlayersByCharacterId(int characterId)
    {
        lock (Lock)
            return _playerList.ToList().Where(p => p.CharacterId == characterId);
    }

    public IEnumerable<Player> GetPlayersByUserId(int playerId)
    {
        lock (Lock)
            return _playerList.ToList().Where(p => p.UserId == playerId);
    }

    public bool AnyPlayersByUserId(int playerId)
    {
        lock (Lock)
            return _playerList.ToList().Any(p => p.UserId == playerId);
    }

    public Player GetPlayerByName(string characterName)
    {
        lock (Lock)
            return _playerList.ToList().Find(p => p.CharacterName == characterName);
    }
}
