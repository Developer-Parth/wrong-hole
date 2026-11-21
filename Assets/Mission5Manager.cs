using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission5Manager : MonoBehaviour
{
    public static int State;
    Transform mum;
    Transform rock;
    int state;
    float timer = 20f;
    float loadAt = -1f;
    bool noM6;
    bool hidden;
    bool inZone;
    Vector3[] wps;
    int wp;
    string endMsg = "";

    void Start()
    {
        State = 0;
        state = 0;
        timer = 20f;
        GameObject m = GameObject.Find("parinosauras");
        if (m != null) mum = m.transform;
        GameObject r = GameObject.Find("HideRock");
        if (r != null) rock = r.transform;
        wps = new Vector3[] { new Vector3(30f, 0f, 52f), new Vector3(6f, 0f, 50f), new Vector3(8f, 0f, 58f), new Vector3(28f, 0f, 60f) };
        wp = 0;
        if (mum != null) mum.position = new Vector3(28f, 0f, 48f);
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

    bool RockBlocks()
    {
        if (mum == null || rock == null) return false;
        Vector3 a = mum.position;
        Vector3 b = transform.position;
        Vector3 c = rock.position;
        a.y = 0f; b.y = 0f; c.y = 0f;
        Vector3 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 0.001f) return false;
        if ((c - a).sqrMagnitude >= len2) return false;
        float t = Mathf.Clamp01(Vector3.Dot(c - a, ab) / len2);
        Vector3 p = a + ab * t;
        float dx = p.x - c.x, dz = p.z - c.z;
        return dx * dx + dz * dz < 8.41f;
    }

    void Fail(string msg)
    {
        state = 9;
        endMsg = msg;
        Debug.Log("PLAYCHAIN M5 FAILED: " + msg);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        State = state;
        if (state == 0 || state == 1)
        {
            inZone = rock != null && FlatDist(transform.position, rock.position) < 5f;
            hidden = inZone && RockBlocks();
            if (hidden)
            {
                state = 1;
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = 0f;
                    state = 2;
                    Debug.Log("PLAYCHAIN M5 complete");
                    loadAt = Time.time + 3f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else state = 0;
            if (mum != null)
            {
                Vector3 target = wps[wp];
                Vector3 to = target - mum.position;
                to.y = 0f;
                if (to.magnitude < 1.5f) wp = (wp + 1) % wps.Length;
                else
                {
                    Vector3 dir = to.normalized;
                    mum.rotation = Quaternion.Slerp(mum.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
                    mum.position += dir * 4f * Time.deltaTime;
                }
                if (!hidden && FlatDist(transform.position, mum.position) < 9f)
                {
                    Fail("SHE FOUND YOU");
                    return;
                }
            }
        }
        if (loadAt > 0f && Time.time >= loadAt)
        {
            loadAt = -1f;
            if (SceneExists("Hatch")) SceneManager.LoadScene("Hatch");
            else noM6 = true;
        }
        if ((state == 2 || state == 9) && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
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
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, 1024, 36), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 4, 1024, 28), "MISSION 5: HIDE BEHIND THE ROCK - STAY OUT OF SIGHT", s);
        if (state == 0)
        {
            GUIStyle w = new GUIStyle(s);
            w.fontSize = 24;
            w.normal.textColor = new Color(1f, 0.5f, 0.3f);
            GUI.Label(new Rect(0, 40, 1024, 30), inZone ? "SHE CAN SEE YOU - STAY BEHIND THE ROCK!" : "GET BEHIND THE ROCK!", w);
        }
        if (state == 1)
        {
            GUIStyle t = new GUIStyle(GUI.skin.label);
            t.fontSize = 44;
            t.alignment = TextAnchor.MiddleCenter;
            t.normal.textColor = timer > 5f ? Color.white : new Color(1f, 0.4f, 0.3f);
            float cy = (Screen.height / k) * 0.20f;
            GUI.Label(new Rect(62, cy, 900, 56), timer.ToString("0.0"), t);
            GUIStyle h = new GUIStyle(s);
            h.fontSize = 18;
            GUI.Label(new Rect(62, cy + 58, 900, 26), "Stay hidden...", h);
        }
        if (state == 2 || state == 9)
        {
            float cy = (Screen.height / k) * 0.36f;
            GUIStyle b = new GUIStyle(GUI.skin.label);
            b.fontSize = 34;
            b.alignment = TextAnchor.MiddleCenter;
            b.wordWrap = true;
            b.normal.textColor = state == 2 ? new Color(1f, 0.95f, 0.6f) : new Color(1f, 0.35f, 0.3f);
            GUI.Label(new Rect(62, cy, 900, 50), state == 2 ? "MISSION 5 COMPLETE" : endMsg, b);
            GUIStyle r = new GUIStyle(b);
            r.fontSize = 20;
            r.normal.textColor = Color.white;
            string sub = state == 9 ? "Press R to restart" : (noM6 ? "Press R to restart" : "Somewhere cold and old awaits...");
            GUI.Label(new Rect(62, cy + 58, 900, 30), sub, r);
        }
        GUI.matrix = old;
    }
}
