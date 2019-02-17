using System;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour, IServer
{
    [SerializeField] float updateRateSeconds = 5;
    [SerializeField] LagNetwork network;
    [SerializeField] WorldSpace serverSpace;

    Dictionary<string, Connection> connections = new Dictionary<string, Connection>();
    Dictionary<string, ServerEntity> entities = new Dictionary<string, ServerEntity>();

    float nextUpdateTime;

    public float TimeStep { get { return updateRateSeconds != 0 ? 1f/updateRateSeconds : 0; } }

    private void Awake()
    {
        network.Listen(Constants.ServerAddress, this);
    }

    private void LateUpdate()
    {
        if (Time.time > nextUpdateTime)
        {
            nextUpdateTime = Time.time + TimeStep;
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
        //TODO: Consider updating entities in steps < timeStep 
        foreach (var conn in connections.Values)
        {
            var msgs = network.Receive(conn.SourceId);
            foreach (var msg in msgs)
            {
                var entityData = entities[msg.EntityId];
                entityData.ProcessMessage(msg);
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
