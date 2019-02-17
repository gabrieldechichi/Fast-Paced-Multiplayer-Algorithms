using UnityEngine;

public class WorldSpace : MonoBehaviour {

    [SerializeField] Entity localEntityPrefab;
    [SerializeField] Entity remoteEntityPrefab;
    [SerializeField] Client client;
    [SerializeField] Server server;
    [SerializeField] Transform[] initialPositions;

    int currentPositionIndex;

	public IEntity InstantiateEntity(string id, bool isLocal)
    {
        return InstantiateEntityInternal(id, isLocal);
    }

    Entity InstantiateEntityInternal(string id, bool isLocal)
    {
        var entityPrefab = isLocal ? localEntityPrefab : remoteEntityPrefab;
        var entity = Instantiate(entityPrefab).GetComponent<Entity>();
        entity.Setup(id, client, server, ChooseColor(id));
        entity.transform.SetParent(transform, false);
        entity.transform.position = GetNewEntityPosition();
        return entity;
    }

    Color ChooseColor(string id)
    {
        var intId = int.Parse(id);
        return intId % 2 == 0 ? Color.blue : Color.green;
    }

    Vector3 GetNewEntityPosition()
    {
        if (initialPositions.Length == 0)
        {
            return transform.TransformPoint(Vector3.zero);
        }
        var pos = initialPositions[currentPositionIndex % initialPositions.Length].position;
        currentPositionIndex++;
        return pos;
    }
}
