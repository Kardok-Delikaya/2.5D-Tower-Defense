using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Animator anim;
    
    [Header("Current Target")]
    public Enemy target;
    
    [Header("Weapon Stats")]
    [SerializeField] private int damage;
    [SerializeField]private float fireRate;
    public float range;
    private float delay;
    
    
    private IDamageMethod currentDamageMethodClass;
    
    private void Start()
    {
        anim = GetComponent<Animator>();
        currentDamageMethodClass=GetComponent<IDamageMethod>();
        currentDamageMethodClass.Init(damage,fireRate,anim);
    }

    public void Tick()
    {
        currentDamageMethodClass.DamageTick(target);
    }
}
