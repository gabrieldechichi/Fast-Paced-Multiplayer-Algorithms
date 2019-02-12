using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {

    [SerializeField] ClientOptions options;
    [SerializeField] LagNetwork network;

    Entity[] entities;
    Connection connection;
    int sequenceNumber;

    private void Start()
    {
        network.Connect(Constants.ServerAddress, (s, conn) =>
        {
            if (s)
            {
                connection = conn;
                sequenceNumber = 0;
                Send("Ok!");
            }
            Debug.Log("Connection result: " + s);
        });
    }

    public void Send(string log)
    {
        network.Send(connection.Id, new Message[] { NewInputMessage(log) });
    }

    InputMessage NewInputMessage(string log)
    {
        return new InputMessage(log, Vector2.zero, new Message(sequenceNumber++, connection.EntityId));
    }

    [System.Serializable]
    struct ClientOptions
    {
        public bool useClientPrediction;
    }
}

public class InputMessage : Message
{
    public string log;
    public Vector2 input;

    public InputMessage(string log, Vector2 input, Message msg) : base(msg)
    {
        this.log = log;
        this.input = input;
    }
}

public static class Constants
{
    public const string ServerAddress = "server!";
}
