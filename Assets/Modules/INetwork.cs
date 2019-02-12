using UnityEngine;
using System;

public interface INetwork
{
    void Listen(string address, IServer server);
    void Connect(string clientAddress, string serverAddress, Action<bool, EntityState, Connection> onConnected);
    void Send(string destinationId, Message[] msgs);
    Message[] Receive(string sourceId);
}

public interface IServer
{
    void Connect(Connection conn, Action<bool, EntityState> onConnected);
}

public class Connection
{
    public string SourceId;
    public string DestinationId;

    public Connection(string source, string destination)
    {
        SourceId = source;
        DestinationId = destination;
    }
}

public class EntityState
{
    public string EntityId;
    public Vector2 Position;
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