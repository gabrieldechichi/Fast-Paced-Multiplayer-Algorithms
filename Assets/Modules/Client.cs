using System.Collections.Generic;
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

    private void Start()
    {
        network.Connect(Constants.ServerAddress, (s, setupData, conn) =>
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
        //ProcessServerMessages();
    }

    //void ProcessServerMessages()
    //{
    //    var msgs = network.Receive(connection.Id);
    //    for (int i = 0; i < msgs.Length; i++)
    //    {
    //        var msg = msgs[i];
    //        var entity = entities[msg.EntityId];
    //        entity.ProcessMessage(msg);
    //    }
    //}

    public void Send(object payload)
    {
        network.Send(connection.Id, new Message[] { NewInputMessage(payload) });
    }

    Message NewInputMessage(object payload)
    {
        return new Message(sequenceNumber++, thisEntity.Id, payload);
    }

    [System.Serializable]
    struct ClientOptions
    {
        public bool useClientPrediction;
    }
}

public static class Constants
{
    public const string ServerAddress = "server!";
}
