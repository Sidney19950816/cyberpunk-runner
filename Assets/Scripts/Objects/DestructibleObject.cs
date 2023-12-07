using Assets.Scripts.Abstractions;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField] private float slowDownPercentage;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.CompareTag(UtilConstants.PLAYER))
        {
            Rigidbody rb = other.transform.root.GetComponent<Rigidbody>();
            rb.velocity -= rb.velocity * (slowDownPercentage / 100);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
