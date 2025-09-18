using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementManager))]
public class PlayerManager : MonoBehaviour
{
    public PlayerMovementManager playerMovementManager { get; private set; }
    public PlayerCombatManager PlayerCombatManager { get; private set; }

    public PlayerData playerData;
    
    private void Start()
    {
        playerMovementManager = GetComponent<PlayerMovementManager>();
        PlayerCombatManager = GetComponent<PlayerCombatManager>();
    }
}