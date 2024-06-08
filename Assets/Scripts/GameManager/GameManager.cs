using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private Transform GameMap;
    [SerializeField] private List<GameObject> _Bullet;
    [SerializeField] private List<GameObject> _ItemPrefabs;

    [SerializeField]
    [SyncVar] private bool isGameStart;

    [SyncVar] private bool isSpawning;

    public List<GameObject> Bullet { get { return _Bullet; } }

    public Vector2 RandomPoint()
    {
        float sizeX = GameMap.transform.localScale.x;
        float sizeY = GameMap.transform.localScale.y;

        float RandomX = Random.Range(-24f, 24f) * sizeX;
        float RandomY = Random.Range(-24f, 24f) * sizeY;

        Vector2 RandomPos = new Vector2(RandomX,RandomY);

        return RandomPos;
    }

    [ServerCallback]
    private IEnumerator SpawnItems()
    {
        isSpawning = true;

        while (isGameStart)
        {
            Debug.Log("아이템 생산중...");

            yield return new WaitForSeconds(1f/*Random.Range(30f, 60f)*/);

            Vector2 spawnPos = RandomPoint();

            float spawnRot = Random.Range(0f, 360f);

            int randomIndex = Random.Range(0, _ItemPrefabs.Count);

            GameObject item = Instantiate(_ItemPrefabs[randomIndex], spawnPos, Quaternion.Euler(0, 0, spawnRot));
            NetworkServer.Spawn(item);
        }

        isSpawning = false;
        yield break;
    }

    public void CheckGameStart()
    {
        int playerCount = NetworkServer.connections.Count;
        isGameStart = (playerCount >= 2);

        if (isGameStart)
        {
            if (isSpawning) return;
            StartCoroutine(SpawnItems());
        }
    }
}
