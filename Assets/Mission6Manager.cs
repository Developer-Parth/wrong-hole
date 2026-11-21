using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission6Manager : MonoBehaviour
{
    public static int State;
    public static bool PromptVisible;
    Transform[] coffins;
    bool[] snapped;
    int photos;
    int state;
    float flash;
    float cineAt = -1f;
    bool noM1;
    Camera cam;
    PlayerController pc;
    Transform lastCoffin;
    float[] lidAt = new float[3] { -1f, -1f, -1f };
    float[] lidBase = new float[3];

    void Start()
    {
        State = 0;
        PromptVisible = false;
        state = 0;
        photos = 0;
        flash = 0f;
        coffins = new Transform[3];
        snapped = new bool[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject g = GameObject.Find("Coffin" + (i + 1));
            if (g != null)
            {
                coffins[i] = g.transform;
                Transform lid0 = g.transform.Find("Lid");
                if (lid0 != null) lidBase[i] = lid0.position.y;
            }
        }
        cam = Camera.main;
        pc = GetComponent<PlayerController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static float FlatDist(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static bool SceneExists(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            if (SceneUtility.GetScenePathByBuildIndex(i).EndsWith("/" + name + ".unity")) return true;
        return false;
    }

    void Update()
    {
        State = state;
        if (flash > 0f) flash -= Time.deltaTime * 1.6f;
        for (int i = 0; i < 3; i++)
        {
            if (lidAt[i] < 0f || coffins[i] == null) continue;
            Transform lid = coffins[i].Find("Lid");
            if (lid == null) continue;
            float k2 = Mathf.Clamp01((Time.time - lidAt[i]) / 1.4f);
            Vector3 lp = lid.position;
            lp.y = lidBase[i] + k2 * 2.8f;
            lid.position = lp;
        }
        if (state == 0)
        {
            int near = -1;
            float best = 4f;
            for (int i = 0; i < 3; i++)
            {
                if (snapped[i] || coffins[i] == null) continue;
                float d = FlatDist(transform.position, coffins[i].position);
                if (d < best) { best = d; near = i; }
            }
            PromptVisible = near >= 0;
            UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
            if (near >= 0 && kb != null && kb.fKey.wasPressedThisFrame)
            {
                snapped[near] = true;
                photos++;
                flash = 1f;
                lastCoffin = coffins[near];
                Debug.Log("PLAYCHAIN M6 photo " + photos);
                lidAt[near] = Time.time;
                if (photos >= 3)
                {
                    state = 2;
                    cineAt = Time.time;
                    PromptVisible = false;
                    if (pc != null) pc.enabled = false;
                    Debug.Log("PLAYCHAIN M6 cinematic");
                }
            }
        }
        else if (state == 2)
        {
            float t = Time.time - cineAt;
            if (cam != null && lastCoffin != null)
            {
                Vector3 look = lastCoffin.position + new Vector3(0f, 2.1f, -0.4f);
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(look - cam.transform.position), 3f * Time.deltaTime);
                cam.fieldOfView = Mathf.Lerp(60f, 17f, Mathf.Clamp01(t / 3.5f));
            }
            if (t > 8.5f)
            {
                state = 3;
                Debug.Log("PLAYCHAIN M6 complete");
                GameManager.Epilogue = true;
                if (SceneExists("Mission1")) SceneManager.LoadScene("Mission1");
                else
                {
                    noM1 = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        if (state == 3 && noM1 && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        float k = Screen.width / 1024f;
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(k, k, 1));
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = 20;
        s.alignment = TextAnchor.MiddleCenter;
        s.normal.textColor = Color.white;
        GUI.color = new Color(0f, 0f, 0f, state == 2 ? 0f : 0.55f);
        GUI.DrawTexture(new Rect(0, 0, 1024, 36), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 4, 1024, 28), (state == 2 ? "" : "MISSION 6: THE PYRAMID - PHOTOGRAPH THE COFFINS"), s);
        if (state == 0)
        {
            GUIStyle c = new GUIStyle(s);
            c.fontSize = 18;
            c.normal.textColor = new Color(1f, 0.9f, 0.6f);
            GUI.Label(new Rect(0, 42, 1024, 24), "COFFINS: " + photos + " / 3", c);
            if (PromptVisible)
            {
                GUIStyle p = new GUIStyle(s);
                p.fontSize = 26;
                p.normal.textColor = Color.white;
                float cy = (Screen.height / k) * 0.62f;
                GUI.Label(new Rect(62, cy, 900, 34), "PRESS F TO PHOTOGRAPH", p);
            }
        }
        if (state == 2)
        {
            float t = Time.time - cineAt;
            string msg = null;
            if (t > 0.8f && t <= 3f) msg = "THE THIRD COFFIN IS DIFFERENT...";
            else if (t > 3f && t <= 5.6f) msg = "IT'S YOURS";
            else if (t > 5.6f) msg = "SAME FOREST. TONIGHT.";
            if (msg != null)
            {
                GUIStyle b = new GUIStyle(GUI.skin.label);
                b.fontSize = msg == "IT'S YOURS" ? 30 : 24;
                b.alignment = TextAnchor.MiddleCenter;
                b.normal.textColor = msg == "IT'S YOURS" ? new Color(1f, 0.3f, 0.25f) : new Color(1f, 0.95f, 0.75f);
                float cy = (Screen.height / k) * 0.82f;
                GUI.Label(new Rect(62, cy, 900, 60), msg, b);
            }
        }
        if (state == 3 && noM1)
        {
            GUIStyle b = new GUIStyle(s);
            b.fontSize = 28;
            GUI.Label(new Rect(62, 300, 900, 40), "Press R to restart", b);
        }
        if (flash > 0f)
        {
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(flash));
            GUI.DrawTexture(new Rect(0, 0, 1024, Screen.height / k), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        GUI.matrix = old;
    }
}
