using System.Collections.Generic;
using System;
using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] ClientOptions options;
    [SerializeField] LagNetwork network;
    [SerializeField] WorldSpace clientSpace;

    Dictionary<string, IEntity> entities = new Dictionary<string, IEntity>();
    IEntity thisEntity;
    Connection connection;
    int sequenceNumber;

    private delegate bool MessageFilter(IEntity entity, Message msg);

    private void Start()
    {
        network.Connect(GetInstanceID().ToString(), Constants.ServerAddress, (s, setupData, conn) =>
        {
            if (s)
            {
                connection = conn;
                sequenceNumber = 0;
                thisEntity = clientSpace.InstantiateEntity(setupData.EntityId);
                entities.Add(thisEntity.Id, thisEntity);
            }
            Debug.Log("Connection result: " + s);
        });
    }

    private void Update()
    {
        ProcessServerMessages();
    }

    void ProcessServerMessages()
    {
        var msgs = network.Receive(connection.SourceId);
        ProcessMessagesWithFilter(msgs, (entity, msg) =>
        {
            var overrideLocalEntityPosition = !options.useClientPrediction;
            return entity.Id != thisEntity.Id || overrideLocalEntityPosition;
        });
    }

    void ProcessLocalMessages(Message[] msgs)
    {
        ProcessMessagesWithFilter(msgs, (entity, msg) =>
        {
            return entity.Id == thisEntity.Id;
        });
    }

    void ProcessMessagesWithFilter(Message[] msgs, MessageFilter filter)
    {
        for (int i = 0; i < msgs.Length; i++)
        {
            var msg = msgs[i];
            var entity = entities[msg.EntityId];
            if (filter(entity, msg))
            {
                entity.ProcessMessage(msg);
            }
        }
    }

    public void Send(object payload)
    {
        var msgs = new Message[] { NewClientMessage(payload) };
        network.Send(connection.DestinationId, msgs);

        if (options.useClientPrediction)
        {
            ProcessLocalMessages(msgs);
        }
    }

    Message NewClientMessage(object payload)
    {
        return new Message(sequenceNumber++, thisEntity.Id, payload);
    }

    [Serializable]
    struct ClientOptions
    {
        public bool useClientPrediction;
    }
}

public static class Constants
{
    public const string ServerAddress = "server!";
}
