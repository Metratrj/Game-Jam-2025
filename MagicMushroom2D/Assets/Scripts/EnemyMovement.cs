using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f; // Reichweite, in der der Gegner den Spieler bemerkt

    private Transform playerTransform; // Referenz auf den Spieler
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Finde den Spieler in der Szene. Annahme: Der Spieler hat den Tag "Player".
        // Stelle sicher, dass dein Spieler-Prefab den Tag "Player" zugewiesen hat!
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("EnemyMovement: Spielerobjekt mit Tag 'Player' nicht gefunden.");
        }
    }

    void FixedUpdate() // FixedUpdate für Physik-Bewegung
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRange)
            {
                // Richtung zum Spieler berechnen
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Bewegung anwenden
                rb.linearVelocity = directionToPlayer * moveSpeed;
            }
            else
            {
                // Wenn außerhalb der Reichweite, stehenbleiben
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            // Wenn kein Spieler gefunden wurde, auch stehenbleiben
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Optional: Visualisiere die detectionRange im Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    // Update is called once per frame
    /* void Update()
    {
        
    } */
}
