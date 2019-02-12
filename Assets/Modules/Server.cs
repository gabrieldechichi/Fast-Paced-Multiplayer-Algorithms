using System;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour, IServer
{
    [SerializeField] LagNetwork network;
    [SerializeField] Entity entityPrefab;
    List<Connection> connections = new List<Connection>();
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
        for (int i = 0; i < connections.Count; i++)
        {
            var msgs = network.Receive(connections[i].Id);
            var entity = entities[connections[i].EntityId];
            
            for (int j = 0; j < msgs.Length; j++)
            {
                entity.ProcessMessage(msgs[j]);
            }
        }
    }

    void SendWorldState()
    {

    }

    public void Connect(Action<bool, Connection> onConnected)
    {
        var entity = Entity.NewEntity(entityPrefab, connections.Count);
        entities.Add(entity.Id, entity);

        var conn = new Connection(entity.Id);
        connections.Add(conn);
        onConnected(true, conn);
    }
}
