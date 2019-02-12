using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public interface INetwork
{
    void Listen(string address, IServer server);
    void Connect(string address, Action<bool, Connection> onConnected);
    void Send(string connectionId, Message[] msgs);
    Message[] Receive(string connectionId);
}

public interface IServer
{
    void Connect(Action<bool, Connection> onConnected);
}

public class Connection
{
    public string Id;
    public string EntityId;

    public Connection(string entityId)
    {
        Id = Guid.NewGuid().ToString();
        EntityId = entityId;
    }
}

public class Message
{
    public int SequenceNumber;
    public string EntityId;
    object payload;

    public T GetPayload<T>() where T : class
    {
        return payload as T;
    }
    
    public Message(int sequenceNumber, string entityId, object payload)
    {
        SequenceNumber = sequenceNumber;
        EntityId = entityId;
        this.payload = payload;
    }
}