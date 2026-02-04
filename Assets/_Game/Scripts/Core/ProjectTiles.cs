using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour 
{
    public float speed = 5f;
    public int damage = 1;
    private Vector3 moveDir;

    public void Launch(Vector3 direction) 
    {
        // bắt đầu bay bay...
    }

    void Update() 
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}

