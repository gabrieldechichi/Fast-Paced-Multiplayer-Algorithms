using System.Collections.Generic;
using System;
using System.Linq;
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

    public ClientOptions Options { get { return options; } }
    public string LocalEntityId { get { return thisEntity.Id; } }

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

    private void LateUpdate()
    {
        ProcessServerMessages();
    }

    private void OnValidate()
    {
        //TODO: Move this to editor script?
        if (options.useClientPrediction && options.useReconciliation)
        {
            options.useReconciliation = true;
        }
        else if (options.useReconciliation && !options.useClientPrediction)
        {
            options.useClientPrediction = true;
        }
    }

    void ProcessServerMessages()
    {
        ProcessMessages(network.Receive(connection.SourceId));
    }

    void ProcessLocalMessages(Message[] msgs)
    {
        if (msgs.Any(m => m.EntityId != thisEntity.Id))
        {
            Debug.LogError("Process local message called for remote entity: This");
            return;
        }
        ProcessMessages(msgs);
    }

    void ProcessMessages(Message[] msgs)
    {
        for (int i = 0; i < msgs.Length; i++)
        {
            var msg = msgs[i];
            var entity = entities[msg.EntityId];
            entity.ProcessMessage(msg);
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
    public struct ClientOptions
    {
        public bool useClientPrediction;
        public bool useReconciliation;
    }
}

public static class Constants
{
    public const string ServerAddress = "server!";
}
