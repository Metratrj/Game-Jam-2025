using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] HealthBar healthBar;

    public float Health;
    public float MaxHealth;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    private bool hasReceivedInput = false; // NEU: Flag, ob schon Eingabe da war

    [Header("Shooting")] // NEU
    [SerializeField] private GameObject projectilePrefab; // Das Projektil-Prefab
    [SerializeField] private float fireRate = 0.5f; // Wie oft geschossen werden kann (Schuss pro Sekunde)
    private float nextFireTime = 0f; // Zeitpunkt, ab dem wieder geschossen werden kann

    public void Shoot(InputAction.CallbackContext context)
    {
        // Nur schießen, wenn die Aktion "started" ist (Taste gedrückt) und die Abklingzeit abgelaufen ist
        if (context.started && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate; // Setze die nächste mögliche Schusszeit

            // Instanziiere das Projektil am Spieler (oder leicht davor)
            // Die Position des Projektils sollte etwas vor dem Spieler sein, damit es nicht mit dem Spieler kollidiert.
            Vector3 shootPosition = transform.position + (Vector3)moveInput.normalized * 0.6f; // 0.6f ist ein kleiner Offset

            // Wenn der Spieler sich nicht bewegt, schieße in die letzte Blickrichtung
            if (moveInput == Vector2.zero)
            {
                // Nutze die "LastInputX/Y" aus dem Animator, um die Schussrichtung zu bestimmen
                float lastInputX = animator.GetFloat("LastInputX");
                float lastInputY = animator.GetFloat("LastInputY");
                shootPosition = transform.position + new Vector3(lastInputX, lastInputY, 0).normalized * 0.6f;
                // Falls LastInput noch nicht gesetzt ist (z.B. Spieler stand von Anfang an still),
                // schieße einfach nach rechts oder in eine Standardrichtung.
                if (lastInputX == 0 && lastInputY == 0)
                {
                    shootPosition = transform.position + Vector3.right * 0.6f;
                }
            }


            GameObject projectileInstance = Instantiate(projectilePrefab, shootPosition, Quaternion.identity);

            // Übergebe die Schussrichtung an das Projektil
            // Wenn Spieler sich bewegt, ist moveInput die Richtung.
            Vector2 shootDirection = (moveInput == Vector2.zero) ?
                                      new Vector2(animator.GetFloat("LastInputX"), animator.GetFloat("LastInputY")).normalized :
                                      moveInput.normalized;

            // Fallback für den Fall, dass keine Richtung gesetzt ist (z.B. wenn LastInputX/Y auch 0 sind)
            if (shootDirection == Vector2.zero)
            {
                shootDirection = Vector2.right; // Standardrichtung, wenn keine andere bekannt ist
            }


            projectileInstance.GetComponent<ProjectileScript>().SetDirection(shootDirection);

            Debug.Log($"Projektil gespawnt in Richtung: {shootDirection}");

            // Optional: Füge hier einen Schuss-Sound oder visuelle Effekte hinzu
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        healthBar.SetMaxHealth(MaxHealth);

        // NEU: Deaktiviere den Rigidbody beim Start temporär
        // Das verhindert, dass die Physik den Spieler direkt verschiebt,
        // bevor er seine tatsächliche Startposition erhält und eine Eingabe hat.
        if (rb != null)
        {
            rb.isKinematic = true; // Macht den Rigidbody unbeweglich durch Physik
            rb.linearVelocity = Vector2.zero; // Stellt sicher, dass keine Restgeschwindigkeit da ist
        }
    }

    private void Update()
    {
        // Nur Bewegung anwenden, wenn wir Input haben UND der Rigidbody nicht null ist
        if (rb != null && hasReceivedInput)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
        else if (rb != null)
        {
            // Wenn noch kein Input, Rigidbody in Ruhe halten
            rb.linearVelocity = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Minus))
            SetHealth(-20f);

        if (Input.GetKeyDown(KeyCode.Plus))
            SetHealth(20f);
    }

    public void Move(InputAction.CallbackContext context)
    {
        // NEU: Aktiviere den Rigidbody bei der ersten Bewegungseingabe
        if (!hasReceivedInput && rb != null)
        {
            rb.isKinematic = false; // Rigidbody wird wieder von der Physik beeinflusst
            hasReceivedInput = true;
        }

        animator.SetBool("IsWalking", true);

        if (context.canceled)
        {
            animator.SetBool("IsWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    public void SetHealth(float healthChange)
    {
        Health += healthChange;
        Health = Mathf.Clamp(Health, 0, MaxHealth);

        healthBar.SetHealth(Health);
    }
}