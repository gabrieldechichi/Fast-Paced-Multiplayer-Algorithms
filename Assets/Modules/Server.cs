using System;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour, IServer
{
    [SerializeField] float updatesPerSecond = 5;
    [SerializeField] LagNetwork network;
    [SerializeField] WorldSpace serverSpace;

    Dictionary<string, Connection> connections = new Dictionary<string, Connection>();
    Dictionary<string, ServerEntity> entities = new Dictionary<string, ServerEntity>();

    float nextUpdateTime;

    private void Awake()
    {
        network.Listen(Constants.ServerAddress, this);
    }

    private void LateUpdate()
    {
        if (Time.time > nextUpdateTime)
        {
            nextUpdateTime = Time.time + 1 / updatesPerSecond;
            UpdateServer();
        }
    }

    void UpdateServer()
    {
        ProcessClientMessages();
        SendWorldState();
    }

    void ProcessClientMessages()
    {
        foreach (var entityId in connections.Keys)
        {
            var conn = connections[entityId];
            var entityData = entities[entityId];

            var msgs = network.Receive(conn.SourceId);
            for (int i = 0; i < msgs.Length; i++)
            {
                entityData.ProcessMessage(msgs[i]);
            }
        }
    }

    void SendWorldState()
    {
        var worldStateMessages = new Message[entities.Count];
        int count = 0;
        foreach (var serverEntity in entities.Values)
        {
            worldStateMessages[count++] = new Message(serverEntity.LastProcessedSequenceNumber, serverEntity.Entity.Id, serverEntity.Entity.BuildCurrentState());
        }

        foreach (var entityId in connections.Keys)
        {
            var conn = connections[entityId];
            network.Send(conn.DestinationId, worldStateMessages);
        }
    }

    public void Connect(Connection conn, Action<bool, EntityState> onConnected)
    {
        var entity = serverSpace.InstantiateEntity(connections.Count.ToString(), false);
        entities.Add(entity.Id, new ServerEntity(entity));

        connections.Add(entity.Id, conn);

        onConnected(true, entity.BuildCurrentState());
    }
}

class ServerEntity
{
    IEntity entity;
    int lastInputSequenceNumber = -1;

    public IEntity Entity { get { return entity; } }
    public int LastProcessedSequenceNumber { get { return lastInputSequenceNumber; } }

    public ServerEntity(IEntity entity)
    {
        this.entity = entity;
    }

    public void ProcessMessage(Message msg)
    {
        lastInputSequenceNumber = msg.SequenceNumber;
        entity.ProcessMessage(msg);
    }
}
