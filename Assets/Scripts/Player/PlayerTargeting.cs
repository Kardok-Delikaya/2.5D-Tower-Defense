using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public static class PlayerTargeting
{
    public static Enemy GetNearestEnemy(Player player)
    {
        Collider[] EnemiesInRange = Physics.OverlapSphere(player.transform.position, player.currentWeapon.range, player.enemyLayer);
        NativeArray<EnemyData> EnemiesToCalculate = new NativeArray<EnemyData>(EnemiesInRange.Length, Allocator.TempJob);
        NativeArray<int> EnemyIndex=new NativeArray<int>(new int[] {-1}, Allocator.TempJob);
        int EnemyIndexToReturn = -1;
        
        for(int i=0; i<EnemiesToCalculate.Length; i++)
        {
            Enemy CurrentEnemy = EnemiesInRange[i].GetComponent<Enemy>();
            int EnemyIndexInList = EnemySummoner.EnemiesInGame.FindIndex(x => x == CurrentEnemy);
            
            EnemiesToCalculate[i] = new EnemyData(CurrentEnemy.transform.position,  EnemyIndexInList);
        }

        SearchForEnemy EnemySearchJob = new SearchForEnemy
        {
            _EnemiesToCalculate = EnemiesToCalculate,
            _EnemyToIndex = EnemyIndex,
            PlayerLocation = player.transform.position,
            CompareValue = Mathf.Infinity
        };

        JobHandle dependency = new JobHandle();
        JobHandle SearchForHandle=EnemySearchJob.Schedule(EnemiesToCalculate.Length, dependency);
        
        SearchForHandle.Complete();

        if (EnemyIndex[0] != -1)
        {
            EnemyIndexToReturn = EnemiesToCalculate[EnemyIndex[0]].EnemyIndex;
            
            EnemiesToCalculate.Dispose();
            EnemyIndex.Dispose();
            
            return EnemySummoner.EnemiesInGame[EnemyIndexToReturn];
        }
        
        EnemiesToCalculate.Dispose();
        EnemyIndex.Dispose();
        return null;
    }

    private struct EnemyData
    {
        public EnemyData(Vector3 position, int enemyIndex)
        {
            EnemyPosition=position;
            EnemyIndex = enemyIndex;
        }

        public Vector3 EnemyPosition;
        public int EnemyIndex;
    }

    private struct SearchForEnemy : IJobFor
    {
        public NativeArray<EnemyData> _EnemiesToCalculate;
        public NativeArray<int> _EnemyToIndex;
        public Vector3 PlayerLocation;
        public float CompareValue;
        
        public void Execute(int index)
        {
            float CurrentEnemyDistanceToPlayer =
                Vector3.Distance(PlayerLocation, _EnemiesToCalculate[index].EnemyPosition);
            if (CurrentEnemyDistanceToPlayer < CompareValue)
            {
                _EnemyToIndex[0] = index;
                CompareValue = CurrentEnemyDistanceToPlayer;
            }
        }
    }
}