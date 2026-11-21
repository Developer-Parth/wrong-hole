using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission4Manager : MonoBehaviour
{
    public static int State;
    Transform mum;
    Transform egg;
    Vector3 nest = new Vector3(20f, 0f, 42f);
    int state;
    float hunger = 100f;
    float loadAt = -1f;
    bool noM5;
    string endMsg = "";

    void Start()
    {
        State = 0;
        state = 0;
        hunger = 100f;
        GameObject m = GameObject.Find("parinosauras");
        if (m != null) mum = m.transform;
        GameObject e = GameObject.Find("Egg1");
        if (e != null) egg = e.transform;
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

    void Fail(string msg)
    {
        state = 9;
        endMsg = msg;
        Debug.Log("PLAYCHAIN M4 FAILED: " + msg);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        State = state;
        if (state == 0)
        {
            hunger -= Time.deltaTime * (100f / 75f);
            if (hunger <= 0f)
            {
                hunger = 0f;
                Fail("YOU COLLAPSED FROM HUNGER");
                return;
            }
            if (FlatDist(transform.position, nest) < 3.2f)
            {
                state = 1;
                Debug.Log("PLAYCHAIN M4 grabbed egg");
                if (egg != null)
                {
                    egg.SetParent(transform, true);
                    egg.localPosition = new Vector3(0f, 0.55f, 0.85f);
                    egg.localScale = new Vector3(0.45f, 0.58f, 0.45f);
                }
            }
        }
        else if (state == 1)
        {
            if (mum != null)
            {
                Vector3 to = transform.position - mum.position;
                to.y = 0f;
                float d = to.magnitude;
                if (d < 3.5f)
                {
                    Fail("PARINOSAURAS CAUGHT YOU");
                    return;
                }
                Vector3 dir = to.normalized;
                mum.rotation = Quaternion.Slerp(mum.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
                mum.position = Vector3.MoveTowards(mum.position, new Vector3(transform.position.x, mum.position.y, transform.position.z), 7.5f * Time.deltaTime);
            }
            if (FlatDist(transform.position, nest) > 35f)
            {
                state = 2;
                Debug.Log("PLAYCHAIN M4 escape complete");
                loadAt = Time.time + 3f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        if (loadAt > 0f && Time.time >= loadAt)
        {
            loadAt = -1f;
            if (SceneExists("Mission5")) SceneManager.LoadScene("Mission5");
            else noM5 = true;
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
        GUI.Label(new Rect(0, 4, 1024, 28), "MISSION 4: STEAL THE DINOSAUR EGG - YOU ARE STARVING", s);
        if (state == 0 || state == 1)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(764, 44, 246, 22), Texture2D.whiteTexture);
            GUI.color = hunger > 30f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.25f, 0.15f);
            GUI.DrawTexture(new Rect(767, 47, 240f * (hunger / 100f), 16), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle hb = new GUIStyle(s);
            hb.fontSize = 15;
            GUI.Label(new Rect(764, 66, 246, 20), "HUNGER", hb);
        }
        if (state == 1)
        {
            float cy = (Screen.height / k) * 0.30f;
            GUIStyle b = new GUIStyle(GUI.skin.label);
            b.fontSize = 32;
            b.alignment = TextAnchor.MiddleCenter;
            b.normal.textColor = new Color(1f, 0.6f, 0.3f);
            GUI.Label(new Rect(62, cy, 900, 44), "GOT THE EGG - RUN!", b);
            GUIStyle ds = new GUIStyle(s);
            ds.fontSize = 18;
            float d = FlatDist(transform.position, nest);
            GUI.Label(new Rect(62, cy + 48, 900, 26), "Get away from the nest: " + d.ToString("0") + "m / 35m", ds);
        }
        if (state == 2 || state == 9)
        {
            float cy = (Screen.height / k) * 0.36f;
            GUIStyle b = new GUIStyle(GUI.skin.label);
            b.fontSize = 34;
            b.alignment = TextAnchor.MiddleCenter;
            b.wordWrap = true;
            b.normal.textColor = state == 2 ? new Color(1f, 0.95f, 0.6f) : new Color(1f, 0.35f, 0.3f);
            GUI.Label(new Rect(62, cy, 900, 50), state == 2 ? "MISSION 4 COMPLETE" : endMsg, b);
            GUIStyle r = new GUIStyle(b);
            r.fontSize = 20;
            r.normal.textColor = Color.white;
            string sub = state == 9 ? "Press R to restart" : (noM5 ? "Press R to restart" : "Find somewhere to hide...");
            GUI.Label(new Rect(62, cy + 58, 900, 30), sub, r);
        }
        GUI.matrix = old;
    }
}
