using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

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
    Server server;

    public float speed;
    public List<Message> unacknoledgedInputs = new List<Message>();
    public EntityStateInterpolator stateInterpolator = new EntityStateInterpolator();

    public string Id { get { return id; } }

    #region Monobehaviour
    void Update()
    {
        if (client == null || IsLocal()) { return; }
        InterpolateState();
    }
    #endregion

    #region IEntity
    public void Setup(string id, Client client, Server server, Color color)
    {
        this.id = id;
        this.client = client;
        this.server = server;
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
            if (IsLocal())
            {
                ProcessStateAsLocal(stateMessage, msg.SequenceNumber);
            }
            else
            {
                ProcessStateAsRemote(stateMessage);
            }
        }
    }

    public EntityState BuildCurrentState()
    {
        return new EntityState { EntityId = Id, Position = transform.position };
    }

    //This is local
    void ProcessInputMessage(InputMessage msg)
    {
        transform.position += new Vector3(msg.delta.x, msg.delta.y, 0);
    }

    void ProcessStateAsLocal(EntityState stateMessage, int msgSequenceNumber)
    {
        SetPosition(stateMessage.Position);

        if (client.Options.useReconciliation)
        {
            var pendingUnacknoledgedInputs = unacknoledgedInputs.Where(m => m.SequenceNumber > msgSequenceNumber);
            foreach (var pendingInput in pendingUnacknoledgedInputs)
            {
                ProcessInputMessage(pendingInput.GetPayload<InputMessage>());
            }
            unacknoledgedInputs.RemoveAll(m => m.SequenceNumber <= msgSequenceNumber);
        }
        else
        {
            unacknoledgedInputs.Clear();
        }
    }

    void ProcessStateAsRemote(EntityState state)
    {
        stateInterpolator.RecordState(state);
    }
    #endregion

    //TODO: Remove this
    bool IsLocal()
    {
        return client.LocalEntityId == id;
    }

    void SetPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    void InterpolateState()
    {
        var renderTimeStamp = DateTime.UtcNow.AddSeconds(-server.TimeStep);
        var entityState = stateInterpolator.GetInterpolatedState(renderTimeStamp, client.Options.useEntityInterpolation);
        if (entityState != null)
        {
            SetPosition(entityState.Position);
        }
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

public struct RemoteEntityState
{
    public EntityState State;
    public DateTime TimeStamp;

    public RemoteEntityState(EntityState state, DateTime timeStamp)
    {
        State = state;
        TimeStamp = timeStamp;
    }

    public bool Invalid()
    {
        return State == null;
    }
}

public class EntityStateInterpolator
{
    RemoteEntityState mostRecent;
    RemoteEntityState secondMostRecent;

    public void RecordState(EntityState state)
    {
        var mostRecentTemp = mostRecent;
        mostRecent = new RemoteEntityState(state, DateTime.UtcNow);
        secondMostRecent = mostRecentTemp;
    }

    public EntityState GetInterpolatedState(DateTime renderTimeStamp, bool useInterpolation)
    {
        if (!useInterpolation || secondMostRecent.Invalid() || mostRecent.TimeStamp < renderTimeStamp)
        {
            return mostRecent.State;
        }

        var startTime = (double) secondMostRecent.TimeStamp.Ticks;
        var endTime = (double) mostRecent.TimeStamp.Ticks;
        var renderTime = (double)renderTimeStamp.Ticks;

        var t = (float) ((renderTime - startTime) / (endTime - startTime));

        return new EntityState { EntityId = mostRecent.State.EntityId, Position = Vector2.Lerp(secondMostRecent.State.Position, mostRecent.State.Position, t) };
    }
}
