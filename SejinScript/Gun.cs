using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Threading;
using UnityEngine.UIElements;

public class Gun : MonoBehaviourPunCallbacks
{
    public int gunType = 0;
    public bool isTurretReady = false;
    public Weapon weapon;
    public FlameWeapon flameWeapon;
    public IceWeapon iceWeapon;
    public Barrier barrier;
    private Transform capsule;

    [PunRPC]
    public void runTurret()
    {
        isTurretReady=true;
        if (gunType == 0)
        {
            weapon = gameObject.GetComponent<Weapon>();
            if (weapon != null)
            {
                weapon.enabled = true;
            }
            else
            {
                Debug.Log("weapon을 찾지 못했습니다.");
            }
        }
        else if (gunType == 1)
        {
            flameWeapon = gameObject.GetComponent<FlameWeapon>();
            if (flameWeapon != null)
            {
                flameWeapon.enabled = true;
            }
            else
            {
                Debug.Log("flameWeapon을 찾지 못했습니다.");
            }
        }
        else if (gunType == 2)
        {
            iceWeapon = gameObject.GetComponent<IceWeapon>();
            if (iceWeapon != null)
            {
                iceWeapon.enabled = true;
            }
            else
            {
                Debug.Log("iceWeapon을 찾지 못했습니다.");
            }
        }
        else if (gunType == 3)
        {
            barrier = gameObject.GetComponent<Barrier>();
            if(barrier != null && transform.parent != null)
            {
                Wall wall = transform.parent.GetComponentInChildren<Wall>();
                transform.rotation = Quaternion.identity;
                if(wall.rotation != -1)
                {
                    if (transform.parent.rotation.y != 0)
                    {
                        transform.localEulerAngles = new Vector3(0, transform.parent.rotation.y + 90, 0);
                    }
                    else
                    {
                        switch (wall.rotation)
                        {
                            case 0:
                                transform.localEulerAngles = new Vector3(0, 0, 0);
                                break;
                            case 1:
                                transform.localEulerAngles = new Vector3(0, 90, 0); 
                                break;
                            case 2:
                                transform.localEulerAngles = new Vector3(0, 180, 0);
                                break;
                            case 3:
                                transform.localEulerAngles = new Vector3(0, -90, 0);
                                break;
                            default:
                                break;
                        }
                    }
                    barrier.enabled = true;
                }
            }
            else
            {
                Debug.Log("barrier을 찾지 못했습니다.");
            }
        }
        Transform shockwave = transform.parent.Find("Capsule(Clone)").Find("Shockwave");
        ParticleSystem particleSystem = shockwave.GetComponent<ParticleSystem>();
        particleSystem.Play();
        MusicManager.TurretReady();
    }

    [PunRPC]
    public void offTurret()
    {
        isTurretReady=false;
        if (gunType == 0)
        {
            weapon = gameObject.GetComponent<Weapon>();
            if (weapon != null)
            {
                weapon.enabled = false;
            }
            else
            {
                Debug.Log("weapon을 찾지 못했습니다.");
            }
        }
        else if (gunType == 1)
        {
            flameWeapon = gameObject.GetComponent<FlameWeapon>();
            if (flameWeapon != null)
            {
                flameWeapon.enabled = false;
            }
            else
            {
                Debug.Log("flameWeapon을 찾지 못했습니다.");
            }
        }
        else if (gunType == 2)
        {
            iceWeapon = gameObject.GetComponent<IceWeapon>();
            if (iceWeapon != null)
            {
                iceWeapon.enabled = false;
            }
            else
            {
                Debug.Log("IceWeapon을 찾지 못했습니다.");
            }
        }
        else if (gunType == 3)
        {
            barrier = gameObject.GetComponent<Barrier>();
            if (barrier != null)
            {
                barrier.enabled = false;
            }
            else
            {
                Debug.Log("barrier 찾지 못했습니다.");
            }
        }
    }
}
