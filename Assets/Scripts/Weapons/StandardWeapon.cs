using UnityEngine;

public class StandardWeapon : MonoBehaviour
{
    private Animator anim;
    
    [Header("Current Target")]
    public Enemy target;
    
    [Header("Weapon Stats")]
    [SerializeField] private int damage;
    [SerializeField]private float fireRate;
    public float range;
    private float delay;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Tick()
    {
        if (delay > 0f)
        {
            delay -= Time.deltaTime;
            return;
        }
        
        if (!target) return;
        
        anim.Play("Shoot",0,0f);
        EnemyManager.EnqueueDamageData(new EnemyDamageData(target,damage));
        
        delay=1f/fireRate;
    }
}