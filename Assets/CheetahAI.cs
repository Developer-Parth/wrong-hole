using UnityEngine;

public class CheetahAI : MonoBehaviour
{
    public float chaseSpeed = 6.5f;
    public float reactionDelay = 2.2f;
    public float catchDistance = 2.2f;
    private Transform player;
    private CharacterController controller;
    private float startTime;
    private float yVelocity;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        controller = GetComponent<CharacterController>();
        startTime = Time.time;
    }

    void Update()
    {
        if (player == null || GameManager.MissionOver || GameManager.Epilogue) return;
        if (GameManager.Frozen)
        {
            yVelocity -= 20f * Time.deltaTime;
            controller.Move(Vector3.up * yVelocity * Time.deltaTime);
            return;
        }
        if (Time.time - startTime < reactionDelay)
        {
            yVelocity -= 20f * Time.deltaTime;
            controller.Move(Vector3.up * yVelocity * Time.deltaTime);
            return;
        }
        Vector3 to = player.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        float dy = transform.position.y - player.position.y;
        if (dist <= catchDistance && dy < 2.5f)
        {
            GameManager.Caught();
            return;
        }
        Vector3 dir = to.normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        yVelocity -= 20f * Time.deltaTime;
        Vector3 move = dir * chaseSpeed + Vector3.up * yVelocity;
        controller.Move(move * Time.deltaTime);
    }
}
