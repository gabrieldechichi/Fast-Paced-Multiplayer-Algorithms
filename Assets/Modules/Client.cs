using UnityEngine;

public class Client : MonoBehaviour
{
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
            }
            Debug.Log("Connection result: " + s);
        });
    }

    public void Send(object payload)
    {
        network.Send(connection.Id, new Message[] { NewInputMessage(payload) });
    }

    Message NewInputMessage(object payload)
    {
        return new Message(sequenceNumber++, connection.EntityId, payload);
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
