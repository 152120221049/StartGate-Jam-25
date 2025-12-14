using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    public PowerUpType weaponType; // Bu hangi silah?
    public float cooldown = 0.5f;
    protected float lastFireTime;

    // Her silahın kendine has vuruş şekli olacak (Abstract)
    public abstract void Attack(Vector2 direction);

    // Flamethrower gibi basılı tutmalı silahlar için durdurma emri
    public virtual void StopAttack() { }
}