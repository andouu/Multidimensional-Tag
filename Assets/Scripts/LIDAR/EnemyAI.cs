using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;


    public float timeBetweenAttacks;
    bool alreadyAttacked;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public GameObject loseText, loseTextDescription;
    public GameObject restartButton;
    private void Start()
    {
        loseText.SetActive(false);
        loseTextDescription.SetActive(false);
        restartButton.SetActive(false);
        Patroling();
        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer); // TODO (wfang): if i have time, instead of checksphere possibly use a raycast to check if player is in line of sight too - so palyer can hide behind walls
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            //print("Patrolling");
            Patroling();
        }
        if (playerInSightRange && !playerInAttackRange)
        {
            //print("Chasing");
            ChasePlayer();
        }
        if (playerInSightRange && playerInAttackRange && !alreadyAttacked)
        {
            //print("Attacking");
            AttackPlayer();
        }
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        // Display tagged messages or wathever
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        loseText.SetActive(true);
        restartButton.SetActive(true);
        loseTextDescription.SetActive(true);
        // Pause scene, set time scale to 0 to pause the game
        print("Setting timescale 0 from here1");
        Time.timeScale = 0f;

        // Below is logic in case we want the player to be able to take many attacks (although this is tag so not reallythat useful?)
        if (!alreadyAttacked)
        {
            // TODO (wfang): Add attacking logic. Maybe just teleport back to the start?
            //Debug.Log("Attacking Player!");
            //print("ATTACKING PLAYER!");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
