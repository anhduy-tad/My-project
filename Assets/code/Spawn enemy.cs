using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnenemy : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs; // Đổi tên mảng thành số nhiều
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenSpawn = 2f;

    void Start()
    {
        StartCoroutine(SpawnEnemyCoroutine());
    }

    private IEnumerator SpawnEnemyCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenSpawn);

            // Kiểm tra mảng không bị rỗng trước khi spawn để tránh lỗi
            if (enemyPrefabs.Length > 0 && spawnPoints.Length > 0)
            {
                // Chọn ngẫu nhiên 1 enemy từ mảng enemyPrefabs
                GameObject selectedEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

                // Chọn ngẫu nhiên 1 vị trí từ mảng spawnPoints
                Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

                // Instantiate tại vị trí selectedSpawnPoint.position
                Instantiate(selectedEnemy, selectedSpawnPoint.position, Quaternion.identity);
            }
        }
    }
}