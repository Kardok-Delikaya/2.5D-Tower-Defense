using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GameManager : MonoBehaviour
{
    public static Vector3[] NodePositions;
    
    private static Queue<EnemyDamageData> DamageData;
    private static Queue<Enemy> EnemiesToRemove;
    private static Queue<int> EnemyIDsToSummon;

    [Header("Play Loop")]
    public bool LoopShouldEnd;
    
    [Header("Enemy Check Point Parent")]
    public Transform NodeParent;
    
    [Header("Player")]
    public PlayerManager player;
    
    private void Start()
    {
        DamageData = new Queue<EnemyDamageData>();
        EnemyIDsToSummon=new Queue<int>();
        EnemiesToRemove = new Queue<Enemy>();
        EnemySummoner.Init();

        NodePositions=new Vector3[NodeParent.childCount];
        
        for (int i = 0; i < NodeParent.childCount; i++)
        {
            NodePositions[i]=NodeParent.GetChild(i).position;
        }
        
        StartCoroutine(GameLoop());
        InvokeRepeating("SummonEnemy",0f,1f);
    }

    void SummonEnemy()
    {
        EnqueueEnemyIDToSummon(0);
    }
    
    IEnumerator GameLoop()
    {
        while (LoopShouldEnd == false)
        {
            if (EnemyIDsToSummon.Count > 0)
            {
                for (int i = 0; i < EnemyIDsToSummon.Count; i++)
                {
                    EnemySummoner.SummonEnemy(EnemyIDsToSummon.Dequeue());
                }
            }
            
            NativeArray<Vector3> NodesToUse = new NativeArray<Vector3>(NodePositions,Allocator.TempJob);
            NativeArray<float> EnemySpeeds = new NativeArray<float>(EnemySummoner.EnemiesInGame.Count,Allocator.TempJob);
            NativeArray<int> NodeIndecs = new NativeArray<int>(EnemySummoner.EnemiesInGame.Count,Allocator.TempJob);
            TransformAccessArray EnemyAcces = new TransformAccessArray(EnemySummoner.EnemiesInGameTransform.ToArray(),2);

            for (int i = 0; i < EnemySummoner.EnemiesInGame.Count; i++)
            {
                EnemySpeeds[i] = EnemySummoner.EnemiesInGame[i].Speed;
                NodeIndecs[i] = EnemySummoner.EnemiesInGame[i].NodeIndex;
            }

            MoveEnemiesJob MoveJob = new MoveEnemiesJob
            {
                NodePosition = NodesToUse,
                EnemySpeed=EnemySpeeds,
                NodeIndex = NodeIndecs,
                deltaTime = Time.deltaTime
            };
            
            JobHandle MoveJobHandle= MoveJob.Schedule(EnemyAcces);
            MoveJobHandle.Complete();

            for (int i = 0; i < EnemySummoner.EnemiesInGame.Count; i++)
            {
                EnemySummoner.EnemiesInGame[i].NodeIndex=NodeIndecs[i];

                if (EnemySummoner.EnemiesInGame[i].NodeIndex == NodePositions.Length)
                {
                    EnqueueEnemyToRemove(EnemySummoner.EnemiesInGame[i]);
                }
            }
            
            NodeIndecs.Dispose();
            EnemySpeeds.Dispose();
            EnemyAcces.Dispose();
            NodesToUse.Dispose();

            
            //Target Enemies
            player.PlayerCombatManager.currentWeapon.target =
                PlayerTargeting.GetNearestEnemy(player.PlayerCombatManager);
            player.PlayerCombatManager.currentWeapon.Tick();
            
            //Deal Damage
            if (DamageData.Count > 0)
            {
                for (int i = 0; i < DamageData.Count; i++)
                {
                    EnemyDamageData CurrentDamageData = DamageData.Dequeue();
                    CurrentDamageData.TargetedEnemy.Health -= CurrentDamageData.TotalDamage;

                    if (CurrentDamageData.TargetedEnemy.Health <= 0)
                    {
                        EnqueueEnemyToRemove(CurrentDamageData.TargetedEnemy);
                    }
                }
            }
            
            //Remove Enemies
            if (EnemiesToRemove.Count > 0)
            {
                for (int i = 0; i < EnemiesToRemove.Count; i++)
                {
                    EnemySummoner.RemoveEnemy(EnemiesToRemove.Dequeue());
                }
            }
            
            yield return null;
        }
    }

    public static void EnqueueDamageData(EnemyDamageData damageData)
    {
        DamageData.Enqueue(damageData);
    }
    
    private static void EnqueueEnemyIDToSummon(int ID)
    {
        EnemyIDsToSummon.Enqueue(ID);
    }

    public static void EnqueueEnemyToRemove(Enemy EnemyToRemove)
    {
        EnemiesToRemove.Enqueue(EnemyToRemove);
    }
}

public struct EnemyDamageData
{
    public EnemyDamageData(Enemy target, int damage)
    {
        TargetedEnemy = target;
        TotalDamage = damage;
    }
    
    public Enemy TargetedEnemy;
    public int TotalDamage;
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction] public NativeArray<int> NodeIndex;

    [NativeDisableParallelForRestriction] public NativeArray<float> EnemySpeed;

    [NativeDisableParallelForRestriction] public NativeArray<Vector3> NodePosition;

    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (NodeIndex[index] < NodePosition.Length)
        {
            Vector3 PositionToMoveTo = NodePosition[NodeIndex[index]];
            transform.position = Vector3.MoveTowards(transform.position, PositionToMoveTo, EnemySpeed[index] * deltaTime);

            if (transform.position == PositionToMoveTo)
            {
                NodeIndex[index]++;
            }
        }
    }
}


[System.Serializable]
public class EnemiesToSpawn
{
    public int EnemyID;
    public float EnemyCount;
    public float TimeBeforeSpawn;
}

[System.Serializable]
public class WavesToStart
{
    public List<EnemiesToSpawn> _EnemiesToSpawn=new List<EnemiesToSpawn>();
    public float TimeBeforeWave;
}