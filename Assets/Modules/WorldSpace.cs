using UnityEngine;

public class WorldSpace : MonoBehaviour {

    [SerializeField] Entity entityPrefab;

    Client client;
    Client Client { get { return client ?? (client = FindObjectOfType<Client>()); } }

	public IEntity InstantiateEntity(string id)
    {
        var entity = Instantiate(entityPrefab).GetComponent<Entity>();
        entity.Setup(id, Client);
        entity.transform.SetParent(transform, false);
        entity.transform.localPosition = Vector2.zero;
        return entity;
    }
}
