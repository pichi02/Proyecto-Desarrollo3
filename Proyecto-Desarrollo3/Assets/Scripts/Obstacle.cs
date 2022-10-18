using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public delegate void CarCollided(int damage);
    public static CarCollided OnCarCollision;

    public static event System.Action OnStun;
    private float timer = 0;
    [SerializeField] private float maxTime = 3;
    [SerializeField] private int damage;
    private CarController carController;
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.transform.CompareTag("Chasis"))
        {
            OnCarCollision?.Invoke(damage);
            Destroy(gameObject);
        }
        if (transform.CompareTag("BearTrap"))
        {
            OnStun?.Invoke();
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > maxTime)
            Destroy(gameObject);
    }

  
}