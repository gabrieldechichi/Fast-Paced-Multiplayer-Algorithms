using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    string Id { get; }
    void ProcessMessage(Message msg);
}

public class Entity : MonoBehaviour, IEntity
{
    string id;
    public Vector2 position;
    public float speed;
    public Vector2[] positionBuffer;

    public string Id { get { return id; } }

    public void ProcessMessage(Message msg)
    {
        var inputMessage = msg as InputMessage;
        if (inputMessage != null)
        {
            Debug.Log(inputMessage.log);
        }
    }

    public static IEntity NewEntity(int number)
    {
        var entity = new GameObject("Entity " + number).AddComponent<Entity>();
        entity.id = number.ToString();
        return entity;
    }
}
