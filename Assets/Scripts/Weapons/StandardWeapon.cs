using UnityEngine;
using System.Collections.Generic;

public class StandardWeapon : MonoBehaviour
{
    private Animator anim;
    private AudioSource audioSource;
    
    [Header("Current Target")]
    public Enemy target;
    
    [Header("Weapon Stats")]
    [SerializeField] private int damage;
    [SerializeField]private float fireRate;
    public float range;
    private float delay;
    
    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> fireSounds;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.clip = fireSounds[Random.Range(0, fireSounds.Count)];
        audioSource.Play();
        EnemyManager.EnqueueDamageData(new EnemyDamageData(target,damage));
        
        delay=1f/fireRate;
    }
}