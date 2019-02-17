using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class EntityController : MonoBehaviour {

    [SerializeField] float speed = 5;
    [SerializeField] bool useAWSD = false;

    Entity entity;
    Entity Entity { get { return entity ?? (entity = GetComponent<Entity>()); } }

    private void Update()
    {
        Entity.Move(GetInput()*speed*Time.deltaTime);
    }

    Vector2 GetInput()
    {
        return useAWSD ? GetInputWithKeys(KeyCode.A, KeyCode.D) : GetInputWithKeys(KeyCode.LeftArrow, KeyCode.RightArrow);
    }

    Vector2 GetInputWithKeys(KeyCode leftKey, KeyCode rightKey)
    {
        float input = Input.GetKey(leftKey) ? -1 : 0;
        input += (Input.GetKey(rightKey)) ? 1 : 0;
        return new Vector2(input, 0);
    }
}
