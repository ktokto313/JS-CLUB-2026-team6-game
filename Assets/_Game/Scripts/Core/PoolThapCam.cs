using System.Collections.Generic;
using UnityEngine;

public class GlobalPoolManager : MonoBehaviour
{
    public static GlobalPoolManager Instance;
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject Get(GameObject prefab, Vector3 pos) {
        string key = prefab.name;
        if (!pools.ContainsKey(key)) pools[key] = new Queue<GameObject>();

        GameObject obj;
        if (pools[key].Count > 0) {
            obj = pools[key].Dequeue();
            obj.transform.SetParent(null);
            obj.transform.position = pos;
            obj.transform.rotation = prefab.transform.rotation;
            obj.transform.localScale = prefab.transform.localScale;
            ResetAllChildrenTransform(obj.transform, prefab.transform);
            Animator anim = obj.GetComponent<Animator>();
            if (anim != null) {
                anim.enabled = true; 
                anim.Rebind();       
                anim.Update(0f);     
            }
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;
                rb.simulated = false; 
            }
            
            obj.SetActive(true);
            if (rb != null) rb.simulated = true;
        } else {
            obj = Instantiate(prefab, pos, prefab.transform.rotation);
            obj.name = key;
        }
        return obj;
    }

    private void ResetAllChildrenTransform(Transform target, Transform source)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            if (i < source.childCount)
            {
                Transform targetChild = target.GetChild(i);
                Transform sourceChild = source.GetChild(i);
                targetChild.localPosition = sourceChild.localPosition;
                targetChild.localRotation = sourceChild.localRotation;
                targetChild.localScale = sourceChild.localScale;
                if (targetChild.childCount > 0)
                {
                    ResetAllChildrenTransform(targetChild, sourceChild);
                }
            }
        }
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        obj.transform.SetParent(null);
        obj.transform.position = new Vector3(9999, 9999, 0);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        obj.SetActive(false);
        if (pools.ContainsKey(obj.name)) pools[obj.name].Enqueue(obj);
    }
}