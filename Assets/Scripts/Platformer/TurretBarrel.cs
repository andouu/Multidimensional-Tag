using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBarrel : MonoBehaviour
{
    public Transform shootPoint;
    public GameObject bulletPrefab;

    public void Shoot(Vector3 direction)
    {
        Instantiate(bulletPrefab, shootPoint.position, Quaternion.LookRotation(direction));
    }
}
