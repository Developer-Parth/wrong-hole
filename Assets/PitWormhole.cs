using UnityEngine;

public class PitWormhole : MonoBehaviour
{
    public float catchRadius = 2.5f;
    public float spinSpeed = 60f;
    private Transform player;
    private Vector3 fullScale;
    private Vector3 hidScale;
    private Light[] glow;
    private float[] glowTarget;
    private Transform beam;
    private float beamFullY;
    private float revealAt = -1f;
    private bool revealed = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        fullScale = transform.localScale;
        hidScale = new Vector3(fullScale.x * 0.12f, fullScale.y * 0.3f, fullScale.z * 0.12f);
        transform.localScale = hidScale;
        glow = GetComponentsInChildren<Light>();
        glowTarget = new float[glow.Length];
        for (int i = 0; i < glow.Length; i++)
        {
            glowTarget[i] = glow[i].intensity;
            glow[i].intensity = 0f;
        }
        Transform bs = transform.Find("WormholeBeam");
        if (bs != null)
        {
            beam = bs;
            beamFullY = bs.localScale.y;
            bs.localScale = new Vector3(bs.localScale.x, 0.02f, bs.localScale.z);
        }
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
        if (player == null || GameManager.MissionOver) return;
        Vector3 d = player.position - transform.position;
        float dxz = new Vector2(d.x, d.z).magnitude;
        if (revealed && player.position.y > 0.8f && dxz > 8f)
        {
            revealed = false;
            transform.localScale = hidScale;
            for (int i = 0; i < glow.Length; i++) glow[i].intensity = 0f;
            if (beam != null) beam.localScale = new Vector3(beam.localScale.x, 0.02f, beam.localScale.z);
            if (Time.timeScale < 0.9f)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
            }
        }
        if (revealed && player.position.y < 0.6f && Time.timeScale > 0.9f)
        {
            Time.timeScale = 0.45f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        if (!revealed)
        {
            if (dxz < 3.5f || player.position.y < 0.6f)
            {
                revealed = true;
                revealAt = Time.time;
                Debug.Log("M1 wormhole reveal");
            }
        }
        else
        {
            float k = Mathf.Clamp01((Time.time - revealAt) / 0.6f);
            k = k * k * (3f - 2f * k);
            transform.localScale = Vector3.Lerp(hidScale, fullScale, k);
            for (int i = 0; i < glow.Length; i++) glow[i].intensity = glowTarget[i] * k;
            if (beam != null) beam.localScale = new Vector3(beam.localScale.x, Mathf.Lerp(0.02f, beamFullY, k), beam.localScale.z);
        }
        if ((player.position.y < -3.2f && dxz < 4.5f) || Vector3.Distance(player.position, transform.position) <= catchRadius)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            GameManager.ReachedWormhole();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            GameManager.ReachedWormhole();
        }
    }
}
