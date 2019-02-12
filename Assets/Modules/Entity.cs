using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    string Id { get; }
    void ProcessMessage(Message msg);
    EntitySetupData GetSetupData();
}

public class Entity : MonoBehaviour, IEntity
{
    string id;
    Client client;

    public float speed;
    public Vector2[] positionBuffer;

    public string Id { get { return id; } }

    public void Setup(string id, Client client)
    {
        this.id = id;
        this.client = client;
    }

    public void ProcessMessage(Message msg)
    {
        var inputMessage = msg.GetPayload<InputMessage>();
        if (inputMessage != null)
        {
            transform.position += new Vector3(inputMessage.delta.x, inputMessage.delta.y, 0);
        }
    }

    public EntitySetupData GetSetupData()
    {
        return new EntitySetupData { EntityId = Id, Position = transform.position };
    }

    public void Move(Vector2 delta)
    {
        client.Send(new InputMessage(delta));
    }
}

public class InputMessage
{
    public Vector2 delta;

    public InputMessage(Vector2 delta)
    {
        this.delta = delta;
    }
}
