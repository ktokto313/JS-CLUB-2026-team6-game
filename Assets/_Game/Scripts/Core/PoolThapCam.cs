using System.Collections.Generic;
using UnityEngine;

public class GlobalPoolManager : MonoBehaviour {
    public static GlobalPoolManager Instance;
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake() { Instance = this; }

    public GameObject Get(GameObject prefab, Vector3 pos) {
        string key = prefab.name;
        if (!pools.ContainsKey(key)) pools[key] = new Queue<GameObject>();

        GameObject obj;
        if (pools[key].Count > 0) {
            obj = pools[key].Dequeue();
            obj.transform.position = pos;
            obj.SetActive(true);
        } else {
            obj = Instantiate(prefab, pos, Quaternion.identity);
            obj.name = key;
        }
        return obj;
    }

    public void Return(GameObject obj) {
        obj.SetActive(false);
        if (pools.ContainsKey(obj.name)) pools[obj.name].Enqueue(obj);
    }
}