using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LagNetwork : MonoBehaviour, INetwork
{
    [SerializeField] int lagMs;
    Dictionary<string, NetworkChannel> networkChannels = new Dictionary<string, NetworkChannel>();
    Dictionary<string, IServer> servers = new Dictionary<string, IServer>();

    public void Listen(string address, IServer server)
    {
        servers.Add(address, server);
    }

    public void Connect(string clientAddress, string serverAddress, Action<bool, EntityState, Connection> onConnected)
    {
        if (!servers.ContainsKey(serverAddress))
        {
            onConnected(false, null, null);
            return;
        }
        var server = servers[serverAddress];
        var serverConn = new Connection(serverAddress, clientAddress);
        server.Connect(serverConn, (s, setupData) =>
        {
            if (s)
            {
                networkChannels.Add(clientAddress, new NetworkChannel());
                networkChannels.Add(serverAddress, new NetworkChannel());
            }

            var clientConn = new Connection(clientAddress, serverAddress);
            if (onConnected != null) { onConnected(s, setupData, clientConn); }
        });
    }

    public void Send(string toId, Message[] msgs)
    {
        if (!networkChannels.ContainsKey(toId)) { return; }
        var channel = networkChannels[toId];
        channel.Messages.AddRange(msgs.Select(m => new DelayedMessage(GetReceiveTime(), m)));
    }

    public Message[] Receive(string fromId)
    {
        if (!networkChannels.ContainsKey(fromId)) { return new Message[0]; }
        var channel = networkChannels[fromId];

        //TODO: Temporal bug here
        var msgsReceived = channel.Messages
            .Where(ShouldBeDelivered)
            .Select(d => d.Msg)
            .ToArray();

        channel.Messages.RemoveAll(ShouldBeDelivered);

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

    public class NetworkChannel
    {
        public List<DelayedMessage> Messages = new List<DelayedMessage>();
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

