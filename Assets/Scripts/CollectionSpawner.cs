using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CollectionSpawner : NetworkBehaviour
{
    public BaseCollectable pointMedalPrefab;
    public float pointMedalDelay = 5f;
    public float pointMedalInitialDelay = 3f;

    private BaseCollectable spawnedPointMedal;
    private float timeSincePointMedalCollected = 0f;

    public BaseCollectable[] powerUpPrefabs = new BaseCollectable[]
    {

    };
    public float powerUpDelay = 15f;
    public int powerUpSpawnCount = 2;
    public float powerUpInitialDelay = 3f;

    private List<BaseCollectable> spawnedPowerUps = new();
    private List<float> timeSincePowerUpsCollected = new();

    public Vector2[] collectableSpawnPositions = new Vector2[]
    {

    };

    // Start is called before the first frame update
    void Start()
    {
        if(IsServer)
        {
            timeSincePointMedalCollected = pointMedalDelay - pointMedalInitialDelay;

            for(int i = 0; i < powerUpSpawnCount; i++)
            {
                timeSincePowerUpsCollected.Add(powerUpDelay - powerUpInitialDelay);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(IsServer)
        {
            ServerHandlePointMedalRespawn();
            ServerHandlePowerUpsRespawn();
        }
    }

    private void ServerHandlePointMedalRespawn()
    {
        if(spawnedPointMedal == null)
        {
            timeSincePointMedalCollected += Time.deltaTime;
            if(timeSincePointMedalCollected >= pointMedalDelay)
            {
                SpawnPointMedal();
                timeSincePointMedalCollected = 0f;
            }
        }
    }

    public void SpawnPointMedal()
    {
        if (!pointMedalPrefab)
        {
            return;
        }
        Vector2 pos = ObtainRandomValidPosition();
        spawnedPointMedal = Instantiate(pointMedalPrefab, pos, Quaternion.identity);
        spawnedPointMedal.gameObject.GetComponent<NetworkObject>().Spawn();
    }

    private void ServerHandlePowerUpsRespawn()
    {
        int powerUpsCollected = spawnedPowerUps.RemoveAll(powerUp => powerUp == null);
        for(int i = 0; i < powerUpsCollected; i++)
        {
            timeSincePowerUpsCollected.Add(0);
        }
        for(int i = 0; i < timeSincePowerUpsCollected.Count; i++)
        {
            timeSincePowerUpsCollected[i] += Time.deltaTime;
            if(timeSincePowerUpsCollected[i] >= powerUpDelay)
            {
                SpawnPowerUp();
                timeSincePowerUpsCollected.RemoveAt(i);
            }
        }
        int powerUpsToSpawn = timeSincePowerUpsCollected.RemoveAll(time => time >= powerUpDelay);
        for(int i = 0; i < powerUpsToSpawn; i++)
        {
            SpawnPowerUp();
        }
    }

    public void SpawnPowerUp()
    {
        if (powerUpPrefabs.Length == 0)
        {
            return;
        }
        Vector2 pos = ObtainRandomValidPosition();
        spawnedPowerUps.Add(Instantiate(powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)], pos, Quaternion.identity));
        spawnedPowerUps[^1].gameObject.GetComponent<NetworkObject>().Spawn();
    }

    private Vector2 ObtainRandomValidPosition()
    {
        List<Vector2> availableSpawnPositions = new List<Vector2>(collectableSpawnPositions);
        availableSpawnPositions.RemoveAll(positionCandidate => {
            if (spawnedPointMedal != null)
            {
                if (positionCandidate == new Vector2(spawnedPointMedal.transform.position.x, spawnedPointMedal.transform.position.y))
                {
                    return true;
                }
            }
            if (spawnedPowerUps.Count > 0)
            {
                foreach(BaseCollectable powerUp in spawnedPowerUps)
                if (positionCandidate == new Vector2(powerUp.transform.position.x, powerUp.transform.position.y))
                {
                    return true;
                }
            }
            return false;
        });
        if(availableSpawnPositions.Count == 0)
        {
            return collectableSpawnPositions[Random.Range(0, collectableSpawnPositions.Length)];
        }
        List<Vector2> validSpawnPositions = new List<Vector2>(availableSpawnPositions);
        validSpawnPositions.RemoveAll(positionCandidate =>
        {
            foreach (Player player in GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None))
            {
                Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);
                if((playerPosition + new Vector2(0, .3f) - positionCandidate).magnitude < 1)
                {
                    return true;
                }
            }
            return false;
        });
        if(validSpawnPositions.Count == 0)
        {
            return availableSpawnPositions[Random.Range(0, availableSpawnPositions.Count)];
        }
        return validSpawnPositions[Random.Range(0, validSpawnPositions.Count)];
    }
}
