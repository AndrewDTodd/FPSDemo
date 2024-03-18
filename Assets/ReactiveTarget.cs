using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using UnityEngine;

public class ReactiveTarget : MonoBehaviour
{
    private int health = 10;

    protected CancellationTokenSource cancelSource = new();
    private static SemaphoreSlim semaphoreSlim = new(1, 1);

    private Object lockObj = new();

    private WanderingAI behavior;

    private void Start()
    {
        behavior = GetComponent<WanderingAI>();
    }

    protected void OnDestroy()
    {
        cancelSource.Cancel();
        cancelSource.Dispose();
    }

    public async void ReactToHit(int damage, Vector3 hitDirection)
    {
        health -= damage;

        if (health <= 0)
        {
            if (semaphoreSlim.CurrentCount > 0)
                await semaphoreSlim.WaitAsync(cancelSource.Token);
            else
                return;
            try
            {
                await Die(hitDirection, cancelSource.Token);
            }
            catch (System.OperationCanceledException)
            {
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        else
        {
            if (semaphoreSlim.CurrentCount > 0)
                await semaphoreSlim.WaitAsync(cancelSource.Token);
            else
                return;
            try
            {
                await TakeDamage(cancelSource.Token);
            }
            catch (System.OperationCanceledException)
            {
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }

    private async Task Die(Vector3 hitDirection, CancellationToken token)
    {
        SceneController.instance.removeEnemy();

        if (behavior)
        {
            behavior.SetAlive(false);
        }

        Material mat = GetComponent<MeshRenderer>().material;
        mat.color = Color.red;

        Rigidbody ridgBody = GetComponent<Rigidbody>();
        ridgBody.isKinematic = false;

        ridgBody.AddForce((hitDirection * 5f), ForceMode.Impulse);

        await Task.Delay(3000);

        token.ThrowIfCancellationRequested();

        Destroy(this.gameObject);
    }

    private async Task TakeDamage(CancellationToken token)
    {
        Material mat = GetComponent<MeshRenderer>().material;

        Color orgColor = mat.color;

        bool changed = false;

        for (int i = 0; i < 3; i++)
        {
            if (token.IsCancellationRequested)
            {
                mat.color = orgColor;

                token.ThrowIfCancellationRequested();
            }

            if (!changed)
            {
                mat.color = Color.red;
                changed = true;
            }
            else
            {
                mat.color = orgColor;
                changed = false;
            }

            await Task.Delay(200);
        }

        mat.color = orgColor;
    }
}
