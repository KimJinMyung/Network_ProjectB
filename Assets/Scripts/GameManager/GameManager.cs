using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private GameObject GameMap;

    private Collider2D mapSize;

    public Vector2 RandomPoint()
    {
        mapSize = GameMap.GetComponent<Collider2D>();

        Bounds bounds = mapSize.bounds;

        float RandomX = Random.Range(bounds.min.x, bounds.max.x);
        float RandomY = Random.Range(bounds.min.y, bounds.max.y);

        Vector2 RandomPos = new Vector2(RandomX,RandomY);

        return RandomPos;
    }
}
