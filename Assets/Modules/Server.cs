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
    }

    void ProcessClientMessages()
    {
        foreach (var entityId in connections.Keys)
        {
            var conn = connections[entityId];
            var entity = entities[entityId];

            var msgs = network.Receive(conn.Id);
            for (int i = 0; i < msgs.Length; i++)
            {
                entity.ProcessMessage(msgs[i]);
            }
        }
    }

    void SendWorldState()
    {

    }

    public void Connect(Action<bool, EntitySetupData, Connection> onConnected)
    {
        var entity = serverSpace.InstantiateEntity(connections.Count.ToString());
        entities.Add(entity.Id, entity);

        var conn = new Connection();
        connections.Add(entity.Id, conn);

        onConnected(true, entity.GetSetupData(), conn);
    }
}
