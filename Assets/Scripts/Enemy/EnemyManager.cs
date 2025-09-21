using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

public class EnemyManager : MonoBehaviour
{
    public static Vector3[] checkPointsPositions;
    
    private static Queue<EnemyDamageData> _damageData;
    private static Queue<Enemy> _enemiesToRemove;
    private static Queue<int> _enemyIDsToSummon;

    private bool loopShouldEnd=false;

    [Header("Player")] 
    [SerializeField] private Player player;

    [Header("Check Point Parent")]
    [SerializeField] private Transform checkPointsParent;

    [Header("Enemy Waves for This Level")] 
    public List<WavesToStart> waves;
    public static float currentWaveCoolDown = 0;
    public static bool waveInProgress = false;
    private bool allWavesEnded = false;
    private int currentWave = 0;
    
    private void Start()
    {
        _damageData = new Queue<EnemyDamageData>();
        _enemyIDsToSummon=new Queue<int>();
        _enemiesToRemove = new Queue<Enemy>();
        EnemySummoner.Init();

        checkPointsPositions=new Vector3[checkPointsParent.childCount];
        
        for (var i = 0; i < checkPointsParent.childCount; i++)
        {
            checkPointsPositions[i]=checkPointsParent.GetChild(i).position;
        }
        
        StartCoroutine(GameLoop());
    }

    private void Update()
    {
        if (allWavesEnded) return;
        
        if (!waveInProgress)
        {
            currentWaveCoolDown -= Time.deltaTime;
            UIManager.instance.waveText.text = $"{(int)currentWaveCoolDown} seconds left until next wave. Press Shift to summon Wave Early";
            
            if (currentWaveCoolDown <= 0)
            {
                waveInProgress = true;
                UIManager.instance.waveText.text = "Wave in progress.";
                StartCoroutine(SpawnWaveEnemies());
            }
        }
    }

    private IEnumerator SpawnWaveEnemies()
    {
        foreach (var enemyList in waves[currentWave].enemiesToSpawn)
        {
            var waitForEnemies = new WaitForSeconds(enemyList.timeBeforeSpawn);
            
            for (var i = 0; i < enemyList.enemyCount; i++)
            {
                EnqueueEnemyIDToSummon(enemyList.enemyID);
                
                yield return waitForEnemies;
            }
        }

        currentWave++;
        waveInProgress = false;
        
        if (currentWave >= waves.Count)
        {
            allWavesEnded = true;
            yield break;
        }

        currentWaveCoolDown = waves[currentWave].timeBeforeWave;
    }

    private IEnumerator GameLoop()
    {
        while (loopShouldEnd == false)
        {
            if (_enemyIDsToSummon.Count > 0)
            {
                for (var i = 0; i < _enemyIDsToSummon.Count; i++)
                {
                    EnemySummoner.SummonEnemy(_enemyIDsToSummon.Dequeue());
                }
            }
            
            var checkPointsToUse = new NativeArray<Vector3>(checkPointsPositions,Allocator.TempJob);
            var enemySpeeds = new NativeArray<float>(EnemySummoner.EnemiesInGame.Count,Allocator.TempJob);
            var checkPointsIndecs = new NativeArray<int>(EnemySummoner.EnemiesInGame.Count,Allocator.TempJob);
            var enemyAcces = new TransformAccessArray(EnemySummoner.EnemiesInGameTransform.ToArray(),2);

            for (var i = 0; i < EnemySummoner.EnemiesInGame.Count; i++)
            {
                enemySpeeds[i] = EnemySummoner.EnemiesInGame[i].Speed;
                checkPointsIndecs[i] = EnemySummoner.EnemiesInGame[i].CheckPointIndex;
            }

            var moveJob = new MoveEnemiesJob
            {
                CheckPointsPosition = checkPointsToUse,
                EnemySpeed=enemySpeeds,
                CheckPointIndex = checkPointsIndecs,
                DeltaTime = Time.deltaTime
            };
            
            //Move Enemies
            var moveJobHandle= moveJob.Schedule(enemyAcces);
            moveJobHandle.Complete();

            for (var i = 0; i < EnemySummoner.EnemiesInGame.Count; i++)
            {
                EnemySummoner.EnemiesInGame[i].CheckPointIndex=checkPointsIndecs[i];

                if (EnemySummoner.EnemiesInGame[i].CheckPointIndex == checkPointsPositions.Length)
                {
                    player.TakeDamage(EnemySummoner.EnemiesInGame[i].Damage);
                    EnqueueEnemyToRemove(EnemySummoner.EnemiesInGame[i]);
                }
            }
            
            checkPointsIndecs.Dispose();
            enemySpeeds.Dispose();
            enemyAcces.Dispose();
            checkPointsToUse.Dispose();
            
            //Deal Damage
            if (_damageData.Count > 0)
            {
                for (var i = 0; i < _damageData.Count; i++)
                {
                    var currentDamageData = _damageData.Dequeue();
                    currentDamageData.TargetedEnemy.Health -= currentDamageData.TotalDamage;

                    if (currentDamageData.TargetedEnemy.Health <= 0)
                    {
                        EnqueueEnemyToRemove(currentDamageData.TargetedEnemy);
                    }
                }
            }
            
            //Remove Enemies
            if (_enemiesToRemove.Count > 0)
            {
                for (var i = 0; i < _enemiesToRemove.Count; i++)
                {
                    EnemySummoner.RemoveEnemy(_enemiesToRemove.Dequeue());
                }
            }

            if (allWavesEnded && EnemySummoner.EnemiesInGame.Count == 0)
            {
                Debug.Log("Game Ended");
                UIManager.instance.GameEnded();
                loopShouldEnd = true;
                //Game Ender
            }
            
            yield return null;
        }
    }
    
    public static void EnqueueDamageData(EnemyDamageData damageData)
    {
        _damageData.Enqueue(damageData);
    }
    
    private static void EnqueueEnemyIDToSummon(int id)
    {
        _enemyIDsToSummon.Enqueue(id);
    }

    public static void EnqueueEnemyToRemove(Enemy enemyToRemove)
    {
        _enemiesToRemove.Enqueue(enemyToRemove);
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
    [NativeDisableParallelForRestriction] public NativeArray<int> CheckPointIndex;

    [NativeDisableParallelForRestriction] public NativeArray<float> EnemySpeed;

    [NativeDisableParallelForRestriction] public NativeArray<Vector3> CheckPointsPosition;

    public float DeltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (CheckPointIndex[index] < CheckPointsPosition.Length)
        {
            Vector3 positionToMoveTo = CheckPointsPosition[CheckPointIndex[index]];
            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, EnemySpeed[index] * DeltaTime);

            if (transform.position == positionToMoveTo)
            {
                CheckPointIndex[index]++;
            }
        }
    }
}


[System.Serializable]
public class EnemiesToSpawn
{
    public int enemyID;
    public float enemyCount;
    public float timeBeforeSpawn;
}

[System.Serializable]
public class WavesToStart
{
    public List<EnemiesToSpawn> enemiesToSpawn=new List<EnemiesToSpawn>();
    public float timeBeforeWave;
}