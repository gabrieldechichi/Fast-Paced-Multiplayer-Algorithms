using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class EntityController : MonoBehaviour {

    [SerializeField] float speed = 5;

    Entity entity;
    Entity Entity { get { return entity ?? (entity = GetComponent<Entity>()); } }

    private void Update()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Entity.Move(input*speed*Time.deltaTime);
    }
}
