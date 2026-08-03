using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public float bloom;
    public float fireRate;
    private float lastShotTime = 0;
    private Rigidbody rb;
    private Renderer rend;
    private Material originalMaterial;
    public Material hitMat;
    public AudioClip shootingSFX;
    private NavMeshAgent agent;

    //AI settings
    public int currentPointIndex = 0;
    public Vector3 currentTarget;
    private int lastPatrolPointIndex = -1;
    public float positionThreshold;
    public float idleTime = 5f;
    public float attackDistance = 5f;
    public float maxVisionDistance = 20f;
    public float minChasingHealth = 30f;
    public Transform[] patrolPoints;
    private float idleTimeCounter;
    private Transform playerTransform;
    private bool canSeePlayer;
    private Vector3 lastKnownPlayerPosition;

    public enum State {Idle, Patrolling, Chasing, Attacking}
    public State state = State.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
        agent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();

        GameObject patrolPointParent = GameObject.FindWithTag("PatrolPoints");
        if (patrolPointParent != null)
        {
            patrolPoints = patrolPointParent.GetComponentsInChildren<Transform>()
                .Where(t => t != patrolPointParent.transform)
                .ToArray();
        }
        else
        {
            patrolPoints = Array.Empty<Transform>();
            Debug.LogWarning("No PatrolPoints parent found. Assign a parent object tagged 'PatrolPoints'.");
        }

        if (patrolPoints.Length > 0)
        {
            currentPointIndex = UnityEngine.Random.Range(0, patrolPoints.Length);
            lastPatrolPointIndex = currentPointIndex;
            currentTarget = patrolPoints[currentPointIndex].position;
        }
        else
        {
            currentTarget = transform.position;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Bullet>(out var bullet))
        {
            health -= bullet.damage;
            if (health <= 0f)
            {
                Die();
            }
            else
            {
                StartCoroutine(Blink());
            }
        }
    }
    private void Die()
    {
        Destroy(gameObject);
        UIManager.Instance.enemiesLeft--;
        UIManager.Instance.enemiesLeftText.text = "Enemies Left: " + UIManager.Instance.enemiesLeft;
        if (UIManager.Instance.enemiesLeft <= 0)
        {
            UIManager.Instance.EnableWinUI();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    IEnumerator Blink()
    {
        rend.material = hitMat;
        yield return new WaitForSeconds(0.1f);
        rend.material = originalMaterial;
    }
    private void Update()
    {
        LookForPlayer();
        switch (state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrolling:
                Patrolling();
                break;
            case State.Attacking:
                Attacking();
                break;
            case State.Chasing:
                Chasing();
                break;
        }
        rb.linearVelocity = Vector3.zero;
        LookAtPlayer();
        SetLastKnownPlayerPosition();
    }

    private void LookForPlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        if(Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, maxVisionDistance))
        {
            canSeePlayer = hit.transform == playerTransform;
            if(canSeePlayer && state != State.Attacking)
            {
                state = State.Chasing;
            }
        }
    }
    private void Idle()
    {
        agent.ResetPath();
        idleTimeCounter -= Time.deltaTime;
        if (idleTimeCounter <= 0)
        {
            state = State.Patrolling;
            idleTimeCounter = idleTime;
        }
    }
    private void Patrolling()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.ResetPath();
            return;
        }

        if (Vector3.Distance(currentTarget, transform.position) < positionThreshold)
        {
            float chance = UnityEngine.Random.Range(0, 100);
            if (chance < 10)
            {
                state = State.Idle;
                return;
            }

            int nextIndex = UnityEngine.Random.Range(0, patrolPoints.Length);
            while (nextIndex == lastPatrolPointIndex && patrolPoints.Length > 1)
            {
                nextIndex = UnityEngine.Random.Range(0, patrolPoints.Length);
            }

            currentPointIndex = nextIndex;
            lastPatrolPointIndex = currentPointIndex;
            currentTarget = patrolPoints[currentPointIndex].position;
        }
        else
        {
            agent.SetDestination(currentTarget);
        }
    }
    private void Attacking()
    {
        idleTimeCounter = idleTime;
        agent.ResetPath();
        Shoot();
        if(Vector3.Distance(transform.position, playerTransform.position) > attackDistance || !canSeePlayer)
        {
            if (health < minChasingHealth)
            {
                state = State.Patrolling; //Cautious
            }
            else
            {
                state = State.Chasing; 
            }
        }
    }
    private void Chasing()
    {
        idleTimeCounter = idleTime;
        agent.SetDestination(lastKnownPlayerPosition);
        if(health < minChasingHealth)
        {
            state = State.Patrolling;
        }
        else if(Vector3.Distance(transform.position, playerTransform.position)<= attackDistance && canSeePlayer)
        {
            state = State.Attacking;
        }
        else if(Vector3.Distance(transform.position, playerTransform.position)>maxVisionDistance)
        {
            state = State.Patrolling;
        }
        else if(Vector3.Distance(transform.position, playerTransform.position)< positionThreshold && !canSeePlayer)
        {
            state = State.Patrolling;
        }
    }
    private void LookAtPlayer()
    {
        if (canSeePlayer)
        {
            transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
        }
    }
    private void SetLastKnownPlayerPosition()
    {
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }
    }
    private void Shoot()
    {
        if (Time.time > lastShotTime + fireRate)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.Normalize();
            Quaternion bulletRotation = Quaternion.LookRotation(directionToPlayer);
            float maxInaccuracy = 10f;
            float currentInaccuracy = bloom * maxInaccuracy;
            float randomYaw = UnityEngine.Random.Range(-currentInaccuracy, currentInaccuracy);
            float randomPitch = UnityEngine.Random.Range(-currentInaccuracy, currentInaccuracy);
            bulletRotation *= Quaternion.Euler(randomPitch, randomYaw + 90, 0f);
            AudioManager.Instance.PlaySFX(shootingSFX, 0.25f);
            Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletRotation);
            Instantiate(weaponFlash, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            lastShotTime = Time.time;
        }
    }
}
