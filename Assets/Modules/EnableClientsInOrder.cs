using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableClientsInOrder : MonoBehaviour {

    [SerializeField] Client[] clients;

    private IEnumerator Start()
    {
        foreach (var c in clients)
        {
            c.gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
        }
    }
}
