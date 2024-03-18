//#define PRINT_DEBUG_INFO

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using UnityEngine;

public class Gun: MonoBehaviour
{
    public int magazineSize;
    public int currentMagazineCount;
    public int reserveAmmo;
    public int damage;
    //seconds
    public int reloadTime;
    public FireType fireType;

    //m/s
    public int bulletVelocity;
    public int fireRate;

    public Sprite image;
    public Sprite crosshair;

    public enum FireType
    {
        Semi,
        Auto,
        Scatter
    }

    protected readonly CancellationTokenSource cancelSource = new();

    protected void OnDestroy()
    {
        cancelSource.Cancel();
        cancelSource.Dispose();
    }

    private async void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
#if PRINT_DEBUG_INFO
            Debug.Log("Player picking up new gun");
#endif

            PlayerStats.instance.ChangeGun(this);

            this.gameObject.SetActive(false);

            try
            {
                await WaitRespawn(cancelSource.Token);
            }
            catch(OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task WaitRespawn(CancellationToken token, int timeToWaitSeconds = 30)
    {
        await Task.Delay(1000 * timeToWaitSeconds);

        token.ThrowIfCancellationRequested();

        this.gameObject.SetActive(true);
    }
}
