using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LagNetwork : MonoBehaviour, INetwork
{
    [SerializeField] int lagMs;
    Dictionary<string, ConnectionDelayer> connectionDelayers = new Dictionary<string, ConnectionDelayer>();
    Dictionary<string, IServer> servers = new Dictionary<string, IServer>();

    public void Listen(string address, IServer server)
    {
        servers.Add(address, server);
    }

    public void Connect(string address, Action<bool, EntitySetupData, Connection> onConnected)
    {
        if (!servers.ContainsKey(address))
        {
            onConnected(false, EntitySetupData.Empty, null);
            return;
        }
        var server = servers[address];
        server.Connect((s, setupData, conn) =>
        {
            if (s)
            {
                connectionDelayers.Add(conn.Id, new ConnectionDelayer(conn));
            }
            if (onConnected != null) { onConnected(s, setupData, conn); }
        });
    }

    public void Send(string connectionId, Message[] msgs)
    {
        if (!connectionDelayers.ContainsKey(connectionId)) { return; }
        var conn = connectionDelayers[connectionId];
        conn.pendingMessages.AddRange(msgs.Select(m => new DelayedMessage(GetReceiveTime(), m)));
    }

    public Message[] Receive(string connectionId)
    {
        if (!connectionDelayers.ContainsKey(connectionId)) { return new Message[0]; }
        var conn = connectionDelayers[connectionId];

        //TODO: Temporal bug here
        var msgsReceived = conn.pendingMessages
            .Where(ShouldBeDelivered)
            .Select(d => d.Msg)
            .ToArray();

        conn.pendingMessages.RemoveAll(ShouldBeDelivered);

        return msgsReceived;
    }

    DateTime GetReceiveTime()
    {
        return DateTime.UtcNow.AddMilliseconds(lagMs);
    }

    bool ShouldBeDelivered(DelayedMessage msg)
    {
        return msg.ReceiveTime <= DateTime.UtcNow;
    }

    public class ConnectionDelayer
    {
        public Connection connection;
        public List<DelayedMessage> pendingMessages = new List<DelayedMessage>();

        public ConnectionDelayer(Connection conn)
        {
            connection = conn;
        }
    }

    public class DelayedMessage
    {
        public DateTime ReceiveTime;
        public Message Msg;

        public DelayedMessage(DateTime receiveTime, Message msg)
        {
            ReceiveTime = receiveTime;
            Msg = msg;
        }
    }
}

