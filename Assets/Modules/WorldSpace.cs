using UnityEngine;

public class WorldSpace : MonoBehaviour {

    [SerializeField] Entity localEntityPrefab;
    [SerializeField] Entity remoteEntityPrefab;
    [SerializeField] Client client;

	public IEntity InstantiateEntity(string id, bool isLocal)
    {
        var entityPrefab = isLocal ? localEntityPrefab : remoteEntityPrefab;
        var entity = Instantiate(entityPrefab).GetComponent<Entity>();
        entity.Setup(id, client, ChooseColor(id));
        entity.transform.SetParent(transform, false);
        entity.transform.localPosition = Vector2.zero;
        return entity;
    }

    Color ChooseColor(string id)
    {
        var intId = int.Parse(id);
        return intId % 2 == 0 ? Color.blue : Color.green;
    }
}
