using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float jumpSpeed = 11f;
    public Animator animator;
    public Transform sprite;
    public LayerMask groundLayers;
    Rigidbody2D body;
    Collider2D shape;
    Vector2 movement;
    Vector3 spawn;
    int jumps;
    bool grounded;
    bool jumpQueued;
    readonly RaycastHit2D[] hits = new RaycastHit2D[8];

    void Awake() { body = GetComponent<Rigidbody2D>(); shape = GetComponent<Collider2D>(); spawn = transform.position; }
    public void OnMove(InputValue value) { movement = value.Get<Vector2>(); }
    public void OnJump(InputValue value) { if (value.isPressed) jumpQueued = true; }

    void FixedUpdate()
    {
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = groundLayers, useTriggers = false };
        int count = shape.Cast(Vector2.down, filter, hits, 0.08f);
        bool onFloor = false;
        for (int i = 0; i < count; i++) if (hits[i].normal.y > 0.6f) onFloor = true;
        grounded = onFloor && body.linearVelocity.y <= 0.1f;
        if (grounded) jumps = 0;
        var velocity = body.linearVelocity;
        velocity.x = movement.x * speed;
        if (jumpQueued && (grounded || jumps < 2))
        {
            // Walking off a ledge consumes the ground jump, leaving one air jump.
            if (!grounded && jumps == 0) jumps = 1;
            velocity.y = jumpSpeed;
            jumps++;
            grounded = false;
        }
        jumpQueued = false;
        body.linearVelocity = velocity;
        bool running = Mathf.Abs(velocity.x) > Mathf.Epsilon;
        animator.SetBool("IsRunning", running);
        if (running) sprite.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
        if (transform.position.y < -10) { body.position = spawn; body.linearVelocity = Vector2.zero; jumps = 0; }
    }
}
