using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Player Combat")]
    [SerializeField] private int maxHealth = 50;
    public LayerMask enemyLayer;
    public StandardWeapon currentWeapon;
    [SerializeField] private StandardWeapon[] weaponsOnPlayer;
    private readonly List<SpriteRenderer> spritesOnPlayer=new List<SpriteRenderer>();
    private bool isInvincible;
    private bool isDead;
    private int health;
    
    [Header("Player Movement")]
    [SerializeField] private float speed=5;
    [SerializeField] private AudioClip walkSound;
    private AudioSource audioSource;
    private float walkSoundTimer;
    private Vector3 movement;
    
    private void Start()
    {
        spritesOnPlayer.AddRange(GetComponentsInChildren<SpriteRenderer>());
        audioSource=GetComponent<AudioSource>();
        audioSource.clip = walkSound;
        health = maxHealth;
        UIManager.instance.revivePlayerButton.onClick.AddListener(RevivePlayer);
        TakeDamage(0);
    }

    private void Update()
    {
        if (isDead) return;
        
        HandleMovement();
        HandleWalkSound();
        HandleCurrentWeapon();
    }

    private void HandleCurrentWeapon()
    {
        currentWeapon.target =
            PlayerTargeting.GetNearestEnemy(this);
        currentWeapon.Tick();
    }
    
    public void TakeDamage(float damageToTake)
    {
        if (isInvincible) return;
        
        health -= (int)damageToTake;

        if (health > 0)
        {
            StartCoroutine(IFrame());
        }
        else
        {
            health = 0;
            UIManager.instance.GameEnded(true);
        }
        
        UIManager.instance.playerHealthText.text=$"Player Health: {health}";
    }
    
    private void HandleMovement()
    {
        transform.Translate(movement * (speed * Time.deltaTime));
    }

    private void HandleWalkSound()
    {
        walkSoundTimer -= Time.deltaTime;
        
        if (movement.magnitude > 0&&walkSoundTimer<0)
        {
            walkSoundTimer = .5f;
            audioSource.pitch=Random.Range(0.8f,1.2f);
            audioSource.Play();
        }
    }

    private IEnumerator IFrame()
    {
        isInvincible = true;

        for (int i = 0; i < 10; i++)
        {
            foreach (var sprites in spritesOnPlayer)
            {
                sprites.enabled = !sprites.enabled;
            }
            
            yield return new WaitForSeconds(.1f);
        }
        
        isInvincible = false;
    }

    public void RevivePlayer()
    {
        health=maxHealth;
        isDead = false;
        TakeDamage(0);
    }
    
    public void HandleMovementInput(InputAction.CallbackContext context)
    { 
        movement = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y).normalized;
    }
    
    public void HandleWeaponChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!weaponsOnPlayer[0].gameObject.activeSelf)
            {
                weaponsOnPlayer[0].gameObject.SetActive(true);
                currentWeapon=weaponsOnPlayer[0];
                weaponsOnPlayer[1].gameObject.SetActive(false);
            }
            else
            {
                weaponsOnPlayer[1].gameObject.SetActive(true);
                currentWeapon=weaponsOnPlayer[1];
                weaponsOnPlayer[0].gameObject.SetActive(false);
            }
        }
    }
    
    public void HandleCallNextWave(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (EnemyManager.waveInProgress) return;

            EnemyManager.currentWaveCoolDown = 0;
        }
    }
}