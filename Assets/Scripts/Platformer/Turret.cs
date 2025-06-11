using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public TurretBarrel barrel;
    public float shootDelay = 1f;
    public Transform playerTransform;
    public Vector2 zBounds = new Vector2(124f, 162f);
    public LineRenderer laserRenderer;

    private bool canShoot = true;

    private IEnumerator Shoot()
    {
        canShoot = false;
        barrel.Shoot(playerTransform.position - barrel.shootPoint.position);
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        bool inRange = playerTransform.position.z <= zBounds.y && playerTransform.position.z >= zBounds.x;

        // aim at the player
        if (inRange)
        {
            transform.LookAt(playerTransform);
            
            if (canShoot)
            {
                StartCoroutine(Shoot());
            }
            else
            {
                laserRenderer.SetPositions(new Vector3[2]
                {
                    barrel.shootPoint.position, playerTransform.position
                });
            }
        }
    }
}
