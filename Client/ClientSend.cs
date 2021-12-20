using System.Collections;
using System.Collections.Generic;
using Project.Classes.Field;
using Project.Classes.Player;
using UnityEngine;

public class ClientSend {
    private static void SendTCPData(Packet _packet) {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    #region Packets

    public static void WelcomeReceived() {
        using Packet _packet = new Packet((int) ClientPackets.welcomeReceived);
        _packet.Write(Client.instance.myId);
        _packet.Write(""/*UIManager.instance.usernameField.text*/); // todo remove

        SendTCPData(_packet);
    }
    public static void SendMove(Point position) {
        using Packet _packet = new Packet((int) ClientPackets.sendMove);
        _packet.Write((int) Player.MoveType.Moving);
        _packet.Write(position);

        SendTCPData(_packet);
    }
    public static void SendMove(Wall wall) {
        using Packet _packet = new Packet((int) ClientPackets.sendMove);
        _packet.Write((int) Player.MoveType.PlacingWall);
        _packet.Write(wall.Pos);
        _packet.Write((int) wall.WallType);

        SendTCPData(_packet);
    }

    #endregion
}
