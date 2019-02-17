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

    #region Monobehaviour
    private void Start()
    {
        network.Connect(GetInstanceID().ToString(), Constants.ServerAddress, (s, entityState, conn) =>
        {
            if (s)
            {
                connection = conn;
                sequenceNumber = 0;
                thisEntity = NewEntity(entityState.EntityId, true);
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
    #endregion

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
            var entity = GetOrCreateEntity(msg.EntityId);
            entity.ProcessMessage(msg);
        }
    }

    IEntity GetOrCreateEntity(string entityId)
    {
        if (entities.ContainsKey(entityId)) { return entities[entityId]; }
        return NewEntity(entityId, false);
    }

    IEntity NewEntity(string entityId, bool isLocal)
    {
        var entity = clientSpace.InstantiateEntity(entityId, isLocal);
        entities.Add(entity.Id, entity);
        return entity;
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
        public bool useEntityInterpolation;
    }
}

public static class Constants
{
    public const string ServerAddress = "server!";
}
