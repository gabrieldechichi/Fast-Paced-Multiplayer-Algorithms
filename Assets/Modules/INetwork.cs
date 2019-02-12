using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public interface INetwork
{
    void Listen(string address, IServer server);
    void Connect(string address, Action<bool, EntitySetupData, Connection> onConnected);
    void Send(string connectionId, Message[] msgs);
    Message[] Receive(string connectionId);
}

public interface IServer
{
    void Connect(Action<bool, EntitySetupData, Connection> onConnected);
}

public class Connection
{
    public string Id;

    public Connection()
    {
        Id = Guid.NewGuid().ToString();
    }
}

public struct EntitySetupData
{
    public string EntityId;
    public Vector2 Position;

    public static EntitySetupData Empty { get { return new EntitySetupData(); } }
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