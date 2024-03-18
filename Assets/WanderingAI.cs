using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WanderingAI : MonoBehaviour
{

    public float speed = 3.0f;
    public float obstacleRange = 5.0f;

    private bool isAlive = true;

    private int bulletLayer;

    private NavMeshAgent agent;

    [SerializeField]
    private GameObject fireBallPrefab;

    private GameObject fireBall;

    public float wanderRadius;
    public float wanderTimer;
    private float timer;

    private void Start()
    {
        bulletLayer = LayerMask.NameToLayer("Bullets");

        agent = GetComponent<NavMeshAgent>();
        if(agent)
        {
            agent.enabled = true;
        }

        timer = wanderTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (isAlive)
        {
            Ray ray = new(transform.position, transform.forward);

            if (timer >= wanderTimer)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                timer = 0f;
            }

            if (Physics.SphereCast(ray, 5f, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    agent.destination = hit.transform.position;

                    if(!fireBall)
                    {
                        fireBall = Instantiate<GameObject>(fireBallPrefab);

                        fireBall.transform.position = transform.TransformPoint(Vector3.forward * 1.5f);
                        fireBall.transform.rotation = transform.rotation;

                        //fireBall.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 1000, ForceMode.Force);
                    }
                }
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDir = Random.insideUnitSphere * dist;

        randDir += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDir, out navHit, dist, layerMask);

        return navHit.position;
    }

    public void SetAlive(bool alive)
    {
        isAlive = alive;

        if (!isAlive)
            agent.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;

            Ray ray = new(transform.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, 5f, bulletLayer))
            {
                if(hit.transform.CompareTag("Player") && agent.isActiveAndEnabled)
                    agent.destination = other.gameObject.transform.position;
            }
        }
    }

    public void MakeAwareOfPlayer(Vector3 position)
    {
        if(agent.isActiveAndEnabled)
            agent.SetDestination(position);
    }
}

