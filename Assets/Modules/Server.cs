using System;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour, IServer
{
    [SerializeField] LagNetwork network;
    [SerializeField] WorldSpace serverSpace;

    Dictionary<string, Connection> connections = new Dictionary<string, Connection>();
    Dictionary<string, IEntity> entities = new Dictionary<string, IEntity>();

    private void Awake()
    {
        network.Listen(Constants.ServerAddress, this);
    }

    private void Update()
    {
        ProcessClientMessages();
        SendWorldState();
    }

    void ProcessClientMessages()
    {
        foreach (var entityId in connections.Keys)
        {
            var conn = connections[entityId];
            var entity = entities[entityId];

            var msgs = network.Receive(conn.SourceId);
            for (int i = 0; i < msgs.Length; i++)
            {
                entity.ProcessMessage(msgs[i]);
            }
        }
    }

    void SendWorldState ()
    {
        var worldStateMessages = new Message[entities.Count];
        int count = 0;
        foreach (var entity in entities.Values)
        {
            worldStateMessages[count++] = new Message(0, entity.Id, entity.BuildCurrentState());
        }

        foreach (var entityId in connections.Keys)
        {
            var conn = connections[entityId];
            network.Send(conn.DestinationId, worldStateMessages);
        }
    }

    public void Connect(Connection conn, Action<bool, EntityState> onConnected)
    {
        var entity = serverSpace.InstantiateEntity(connections.Count.ToString());
        entities.Add(entity.Id, entity);

        connections.Add(entity.Id, conn);

        onConnected(true, entity.BuildCurrentState());
    }
}
