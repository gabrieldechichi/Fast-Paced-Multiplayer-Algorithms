using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class WorldSpace : MonoBehaviour {

    [SerializeField] Entity entityPrefab;

    RectTransform rect;
    RectTransform Rect { get { return rect ?? (rect = GetComponent<RectTransform>()); } }
    Client client;
    Client Client { get { return client ?? (client = FindObjectOfType<Client>()); } }

	public IEntity InstantiateEntity(string id)
    {
        var entity = Instantiate(entityPrefab).GetComponent<Entity>();
        entity.Setup(id, Client);
        entity.transform.SetParent(Rect, false);
        entity.transform.localPosition = Vector2.zero;
        return entity;
    }
}
