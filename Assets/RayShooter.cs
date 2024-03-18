//#define PRINT_DEBUG_INFO

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RayShooter : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletSpawnPoint;

    private Camera cam;

    //in seconds
    private readonly int sphereDespawnTime = 10;

    private int bulletLayer;
    //private LayerMask bulletLayerMask;

    protected static CancellationTokenSource cancelSource;

    private readonly int screenDiv = 48;

    // Start is called before the first frame update
    void Start()
    {
        cancelSource = new();

        if (!cam)
            cam = GetComponent<Camera>();

        if(!bulletSpawnPoint)
        {
            bulletSpawnPoint = new GameObject("BulletSpawnPoint");
            bulletSpawnPoint.transform.parent = cam.transform.parent;
            bulletSpawnPoint.transform.position = cam.transform.position + new Vector3(0f, 0f, .5f);
        }

        bulletLayer = LayerMask.NameToLayer("Bullets");
        //bulletLayerMask = LayerMask.GetMask("Bullets");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private async void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            await PlayerStats.instance.ReloadGun();
        }

        if (PlayerStats.instance.playerGun != null)
        {
            if (PlayerStats.instance.playerGun.fireType == Gun.FireType.Semi)
            {
                if (Input.GetMouseButtonDown(0) && PlayerStats.instance.canFire)
                {
                    await PlayerStats.instance.FireGun();

                    Vector3 point = new(cam.pixelWidth / 2, cam.pixelHeight / 2, 0f);
                    Ray ray = cam.ScreenPointToRay(point);

                    if (Physics.Raycast(ray, out RaycastHit hit, 50f, bulletLayer))
                    {
                        GameObject hitObject = hit.transform.gameObject;
                        ReactiveTarget target = hitObject.GetComponent<ReactiveTarget>();

                        Vector3 direction = (hit.point - bulletSpawnPoint.transform.position);
                        direction.Normalize();

                        if (target)
                        {
                            target.ReactToHit(PlayerStats.instance.playerGun.damage, direction);
                            WanderingAI behavior = hitObject.GetComponent<WanderingAI>();
                            if (behavior)
                                behavior.MakeAwareOfPlayer(transform.position);
                        }

                        //Debug.Log($"Hit at point {hit.point}");
                        try
                        {
                            await SphereIndicator(bulletSpawnPoint.transform.position, hit.point, PlayerStats.instance.playerGun.bulletVelocity,hit.transform.gameObject, cancelSource.Token);
                        }
                        catch (System.OperationCanceledException)
                        {
#if PRINT_DEBUG_INFO
                    Debug.Log("Cancelation requested. Async ops exiting");
#endif
                            return;
                        }
                    }
                }
                else if(Input.GetMouseButtonDown(0) && !PlayerStats.instance.canFire)
                    await PlayerStats.instance.ReloadGun();
            }
            else if (PlayerStats.instance.playerGun.fireType == Gun.FireType.Auto)
            {
                if (Input.GetMouseButton(0) && PlayerStats.instance.canFire)
                {
                    await PlayerStats.instance.FireGun();

                    Vector3 point = new(cam.pixelWidth / 2, cam.pixelHeight / 2, 0f);
                    Ray ray = cam.ScreenPointToRay(point);

                    if (Physics.Raycast(ray, out RaycastHit hit, 50f, bulletLayer))
                    {
                        GameObject hitObject = hit.transform.gameObject;
                        ReactiveTarget target = hitObject.GetComponent<ReactiveTarget>();

                        Vector3 direction = (hit.point - bulletSpawnPoint.transform.position);
                        direction.Normalize();

                        if (target)
                        {
                            target.ReactToHit(PlayerStats.instance.playerGun.damage, direction);
                            WanderingAI behavior = hitObject.GetComponent<WanderingAI>();
                            if (behavior)
                                behavior.MakeAwareOfPlayer(transform.position);
                        }

                        //Debug.Log($"Hit at point {hit.point}");
                        try
                        {
                            await SphereIndicator(bulletSpawnPoint.transform.position, hit.point, PlayerStats.instance.playerGun.bulletVelocity, hit.transform.gameObject, cancelSource.Token);
                        }
                        catch (System.OperationCanceledException)
                        {
#if PRINT_DEBUG_INFO
                    Debug.Log("Cancelation requested. Async ops exiting");
#endif
                            return;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0) && !PlayerStats.instance.canFire)
                    await PlayerStats.instance.ReloadGun();
            }
            else if (PlayerStats.instance.playerGun.fireType == Gun.FireType.Scatter)
            {
                if (Input.GetMouseButtonDown(0) && PlayerStats.instance.canFire)
                {
                    await PlayerStats.instance.FireGun();

                    Vector3 point = new(cam.pixelWidth / 2, cam.pixelHeight / 2, 0f);
                    Ray ray = cam.ScreenPointToRay(point);

                    if (Physics.Raycast(ray, out RaycastHit hit, 50f, bulletLayer))
                    {
                        //Debug.Log($"Hit at point {hit.point}");
                        try
                        {
                            List<Task> tasks = new();

                            for (int i = 0; i < 5; i++)
                            {
                                Vector3 newPoint = new((cam.pixelWidth / 2) + Random.Range(-(cam.pixelWidth / screenDiv), (cam.pixelWidth / screenDiv)), (cam.pixelHeight / 2) + +Random.Range(-(cam.pixelWidth / screenDiv), (cam.pixelWidth / screenDiv)), 0f);
                                Ray newRay = cam.ScreenPointToRay(newPoint);

                                if (Physics.Raycast(newRay, out RaycastHit newHit, 50f, bulletLayer))
                                {
                                    GameObject hitObject = newHit.transform.gameObject;
                                    ReactiveTarget target = hitObject.GetComponent<ReactiveTarget>();

                                    Vector3 direction = (hit.point - bulletSpawnPoint.transform.position);
                                    direction.Normalize();

                                    if (target)
                                    {
                                        target.ReactToHit(PlayerStats.instance.playerGun.damage, direction);
                                        WanderingAI behavior = hitObject.GetComponent<WanderingAI>();
                                        if (behavior)
                                            behavior.MakeAwareOfPlayer(transform.position);
                                    }

                                    tasks.Add(SphereIndicator(bulletSpawnPoint.transform.position, newHit.point, PlayerStats.instance.playerGun.bulletVelocity, newHit.transform.gameObject, cancelSource.Token));
                                }
                            }

                            await Task.WhenAll(tasks);
                        }
                        catch (System.OperationCanceledException)
                        {
#if PRINT_DEBUG_INFO
                    Debug.Log("Cancelation requested. Async ops exiting");
#endif
                            return;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0) && !PlayerStats.instance.canFire)
                    await PlayerStats.instance.ReloadGun();
            }
        }
    }

    protected void OnDestroy()
    {
        cancelSource.Cancel();
        cancelSource.Dispose();
    }

    private async Task SphereIndicator(Vector3 startPos, Vector3 endPos, float velocity, GameObject newParent, CancellationToken token)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(.2f, .2f, .2f);
        sphere.transform.position = startPos;
        sphere.layer = bulletLayer;

        while (sphere.transform.position != endPos)
        {
            token.ThrowIfCancellationRequested();

            sphere.transform.position = Vector3.MoveTowards(sphere.transform.position, endPos, velocity * Time.deltaTime);
            await Task.Yield();
        }

        sphere.transform.parent = newParent.transform;

        if (sphereDespawnTime != 0)
        {
            await Task.Delay(1000 * sphereDespawnTime);

            token.ThrowIfCancellationRequested();

            Destroy(sphere);
        }
        else
            return;
    }
}
