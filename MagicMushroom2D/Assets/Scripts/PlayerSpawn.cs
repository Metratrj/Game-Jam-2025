using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private GameObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (dungeonGenerator != null && dungeonGenerator.StartRoom != null)
        {
            Vector2 spawnPosition = dungeonGenerator.StartRoom.bounds.center;
            Debug.Log($"Versuche Spieler zu spawnen bei: {spawnPosition}");

            playerPrefab.transform.position = spawnPosition; // Setze die Startposition des Prefabs

            //GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            // NEU: Stelle sicher, dass die Position direkt nach dem Spawnen gesetzt wird
            // Das sollte Instantiate bereits tun, aber zur Sicherheit kann man es explizit machen.
            //playerInstance.transform.position = spawnPosition;

            Debug.Log($"Spieler gespawnt bei tatsächlicher Position: {playerPrefab.transform.position}");

            playerPrefab.name = "Player";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
