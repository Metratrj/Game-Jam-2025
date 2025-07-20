using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f; // Wie lange das Projektil existiert
    [SerializeField] private int damage = 1; // Schaden, den das Projektil verursacht

    private Vector2 direction; // Richtung, in die das Projektil fliegen soll

    // Methode, um die Richtung des Projektils zu setzen (vom Spieler-Skript aufgerufen)
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized; // Normalisieren, um eine konstante Geschwindigkeit zu gewährleisten
    }

    void Start()
    {
        // Zerstöre das Projektil nach einer bestimmten Zeit, um nicht benötigte Objekte zu entfernen
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Bewege das Projektil in seine Richtung
        transform.Translate(direction * speed * Time.deltaTime);
    }

    // Wird aufgerufen, wenn dieser Trigger-Collider einen anderen Trigger-Collider berührt
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Beispiel: Wenn das Projektil einen Gegner trifft
        if (other.CompareTag("Enemy")) // Stelle sicher, dass deine Gegner den Tag "Enemy" haben!
        {
            // Hier könntest du Schaden an den Gegner anwenden
            // Beispiel: other.GetComponent<EnemyHealth>().TakeDamage(damage);
            Debug.Log($"Projektil hat Gegner '{other.name}' getroffen und {damage} Schaden verursacht.");

            // Zerstöre das Projektil, nachdem es etwas getroffen hat
            Destroy(gameObject);
        }
        // Optional: Projektil zerstören, wenn es eine Wand trifft
        else if (other.CompareTag("WallCollision")) // Verwende den Tag, den du deiner Collision Tilemap gibst
        {
             Destroy(gameObject);
        }
    }
}
