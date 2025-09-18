using UnityEngine;

public interface IDamageMethod
{
    public void DamageTick(Enemy target);
    public void Init(int damage, float fireRate, Animator anim);
}

public class StandardDamage : MonoBehaviour, IDamageMethod
{
    private Animator Anim;
    private int Damage;
    private float FireRate;
    private float Delay;

    public void Init(int damage, float fireRate, Animator anim)
    {
        this.Damage= damage;
        this.FireRate = fireRate;
        this.Anim = anim;
        Delay = 1f / this.FireRate;
    }
    
    public void DamageTick(Enemy target)
    {
        if (Delay > 0f)
        {
            Delay -= Time.deltaTime;
            return;
        }
        
        if (!target) return;
        
        Anim.Play("Shoot",0,0f);
        GameManager.EnqueueDamageData(new EnemyDamageData(target,Damage));
        
        Delay=1f/FireRate;
    }
}