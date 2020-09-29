using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;

    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    private PlayerController player;


    public void AttachedToPlayer(PlayerController incPlayer)
    {
        player = incPlayer;
        //Disable Weapon movement
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        //Reset weapon position to pivot
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;

        //Disable the weapon colliders to prevent continuous coll w/player
        foreach (Collider coll in gameObject.GetComponents<Collider>())
        {
            coll.enabled = false;
        }
    }

    public void TryShoot()
    {
        //Can we shoot?
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
        {//Cannot shoot
            return;
        }

        curAmmo--;
        lastShootTime = Time.time;

        //Update ammo ui
        GameUI.instance.UpdateAmmoText();


        // spawn the bullet
        //player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.transform.position, Camera.main.transform.forward);
        SpawnBullet(bulletSpawnPos.transform.position, Camera.main.transform.forward);
    }

    [PunRPC]
    void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        // spawn and orient it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        Debug.Log("Spawned with dir " + dir * bulletSpeed);

        // get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        // initialize it and set the velocity
        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript.rb.AddForce(dir * bulletSpeed, ForceMode.VelocityChange);
    }



    #region Pickups

    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        //Update UI
        GameUI.instance.UpdateAmmoText();
    }

    #endregion Pickups



}
