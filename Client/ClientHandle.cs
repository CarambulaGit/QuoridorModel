using System;
using System.Collections;
using System.Collections.Generic;
using Project.Classes.Field;
using Project.Classes.Player;
using UnityEngine;
using NetworkPlayer = Project.Classes.Player.NetworkPlayer;

public class ClientHandle {
    public static void Welcome(Packet _packet) {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();
    }

    public static void StartGame(Packet _packet) {
        int _firstPlayerId = _packet.ReadInt();
        int _secondPlayerId = _packet.ReadInt();
        if (Client.instance.myId == _firstPlayerId) {
            Client.instance.GameManager.CreateNetworkPlayerVsPlayer(_firstPlayerId, _secondPlayerId, true);
        }
        else {
            Client.instance.GameManager.CreateNetworkPlayerVsPlayer(_secondPlayerId, _firstPlayerId, false);
        }
    }

    public static void MakeMove(Packet _packet) {
        int _playerId = _packet.ReadInt();
        var moveType = (Player.MoveType) _packet.ReadInt();
        var pos = _packet.ReadPoint();
        var game = Client.instance.GameManager.Game;
        var player = (NetworkPlayer) game.FindPlayerWithNetworkId(_playerId);
        switch (moveType) {
            case Player.MoveType.PlacingWall:
                var wallType = (Wall.Type) _packet.ReadInt();
                player.SetWallByServer(new Wall(pos, wallType));
                break;
            case Player.MoveType.Moving:
                player.MovePawnByServer(pos);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void RestartGame(Packet _packet)
    {
        Client.instance.GameManager.Restart();
    }
}