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