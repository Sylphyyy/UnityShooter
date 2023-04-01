using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System;

[CreateAssetMenu(menuName = "3PS/New Gun")] 
public class GunInfo : ItemInfo
{
    public float damage;
    public float firerate = 0.1f;
    public float spread = 0.001f; //a spread of 0.5, for example, means the bullets can hit anywhere on the screen.
    public int pelletsPerAttack = 1;
    public bool automatic;
    public float shootCooldown = 0.5f; // Set the cooldown to 0.5 seconds

    public float scopeZoomMult = 0.9f;
    public float scopeInSpeed = 0.2f;

}
