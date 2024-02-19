using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FireSword : Skill
{

    [SerializeField] FireEffect fire;

    public override void Excute(Transform weaponTrm, Transform target, int power, Guid guid)
    {

        var f = Instantiate(fire, weaponTrm.position, quaternion.identity);

        f.Shoot(power * 5, power);

    }

}
