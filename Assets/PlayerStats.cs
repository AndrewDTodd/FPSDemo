using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public Gun playerGun = null;

    public static PlayerStats instance = null;

    public Image UIGunIcon;
    public Image UIGunCrosshair;
    public TMPro.TextMeshProUGUI UIGunText;
    public Slider relodeSlider;
    public Slider healthSlider;

    public bool canFire = true;

    public int health = 5;

    private void Start()
    {
        if(!instance)
        {
            instance = this;
        }

        ChangeGun(playerGun);

        healthSlider.maxValue = health;
        healthSlider.value = health;
    }

    public void ChangeGun(Gun newGun)
    {
        Color uiColor = UIGunIcon.color;

        if (newGun == null)
        {
            uiColor.a = 0f;
            UIGunIcon.color = uiColor;

            UIGunText.text = "";

            return;
        }

        playerGun = newGun;

        playerGun.currentMagazineCount = playerGun.magazineSize;

        UIGunIcon.sprite = playerGun.image;
        UIGunCrosshair.sprite = playerGun.crosshair;
        uiColor.a = 1f;
        UIGunIcon.color = uiColor;

        UIGunText.text = $"{playerGun.magazineSize}/{playerGun.reserveAmmo}";

        canFire = true;
    }

    public void Hurt(int damage)
    {
        health -= damage;
        healthSlider.value -= damage;

        if(health <= 0)
        {
            SceneController.instance.PlayerLose();
        }
    }

    public async Task FireGun()
    {
        canFire = false;

        if (playerGun.currentMagazineCount > 0)
        {
            playerGun.currentMagazineCount--;

            UIGunText.text = $"{playerGun.currentMagazineCount}/{playerGun.reserveAmmo}";

            await Task.Delay(1000 / (playerGun.fireRate / 60));

            if(playerGun.currentMagazineCount > 0)
                canFire = true;
        }
    }

    public async Task ReloadGun()
    {
        if(playerGun.reserveAmmo > 0)
        {
            canFire = false;

            //await Task.Delay(1000 * (playerGun.reloadTime));

            float timer = 0;

            while(timer < playerGun.reloadTime)
            {
                timer += Time.deltaTime;

                relodeSlider.value = timer/playerGun.reloadTime;
                await Task.Yield();
            }

            playerGun.reserveAmmo += playerGun.currentMagazineCount;
            playerGun.currentMagazineCount = (playerGun.reserveAmmo > playerGun.magazineSize ? playerGun.magazineSize : playerGun.reserveAmmo);

            playerGun.reserveAmmo -= playerGun.currentMagazineCount;

            UIGunText.text = $"{playerGun.currentMagazineCount}/{playerGun.reserveAmmo}";

            canFire = true;
        }
    }
}
