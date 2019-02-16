using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public interface IEntity
{
    string Id { get; }
    void ProcessMessage(Message msg);
    EntityState BuildCurrentState();
}

public class Entity : MonoBehaviour, IEntity
{
    [SerializeField] Image image;
    string id;
    Client client;

    public float speed;
    public List<Message> unacknoledgedInputs = new List<Message>();

    public string Id { get { return id; } }

    public void Setup(string id, Client client, Color color)
    {
        this.id = id;
        this.client = client;
        image.color = color;
    }

    public void ProcessMessage(Message msg)
    {
        //This is local
        var inputMessage = msg.GetPayload<InputMessage>();
        if (inputMessage != null)
        {
            ProcessInputMessage(inputMessage);
            unacknoledgedInputs.Add(msg);
        }

        //this is server
        var stateMessage = msg.GetPayload<EntityState>();
        if (stateMessage != null)
        {
            transform.position = new Vector3(stateMessage.Position.x, stateMessage.Position.y, transform.position.z);

            if (client.Options.useReconciliation)
            {
                var pendingUnacknoledgedInputs = unacknoledgedInputs.Where(m => m.SequenceNumber > msg.SequenceNumber);
                foreach (var pendingInput in pendingUnacknoledgedInputs)
                {
                    ProcessInputMessage(pendingInput.GetPayload<InputMessage>());
                }
                unacknoledgedInputs.RemoveAll(m => m.SequenceNumber <= msg.SequenceNumber);
            }
            else
            {
                unacknoledgedInputs.Clear();
            }
        }
    }

    //This is local
    void ProcessInputMessage(InputMessage msg)
    {
        transform.position += new Vector3(msg.delta.x, msg.delta.y, 0);
    }

    public EntityState BuildCurrentState()
    {
        return new EntityState { EntityId = Id, Position = transform.position };
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
