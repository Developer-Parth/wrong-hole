using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission3Manager : MonoBehaviour
{
    public static int State;
    Transform mum;
    int state;
    float t0;
    float loadAt = -1f;
    bool noM4;

    void Start()
    {
        State = 0;
        state = 0;
        GameObject m = GameObject.Find("parinosauras");
        if (m != null) mum = m.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        State = state;
        if (state == 0 && mum != null)
        {
            if (Vector3.Distance(transform.position, mum.position) < 14f)
            {
                state = 1;
                t0 = Time.time;
                Vector3 look = transform.position - mum.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f) mum.rotation = Quaternion.LookRotation(look);
            }
        }
        else if (state == 1 && Time.time - t0 > 8.5f)
        {
            state = 2;
            loadAt = Time.time + 3f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (loadAt > 0f && Time.time >= loadAt)
        {
            loadAt = -1f;
            bool found = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (SceneUtility.GetScenePathByBuildIndex(i).EndsWith("/Mission4.unity")) { found = true; break; }
            }
            if (found) SceneManager.LoadScene("Mission4");
            else noM4 = true;
        }
        if (state == 2 && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
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
        GUI.Label(new Rect(0, 4, 1024, 28), "MISSION 3: APPROACH THE CREATURE", s);
        if (state == 0 && mum != null)
        {
            float d = Vector3.Distance(transform.position, mum.position);
            GUIStyle ds = new GUIStyle(s);
            ds.fontSize = 17;
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(new Rect(0, 36, 1024, 28), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(0, 40, 1024, 24), d > 25f ? "Distance to the creature: " + d.ToString("0") + "m" : "IT IS WATCHING YOU...", ds);
        }
        if (state >= 1)
        {
            float el = state == 1 ? Time.time - t0 : 99f;
            int idx = el < 2.5f ? 0 : el < 5f ? 1 : el < 7.5f ? 2 : 3;
            string msg = idx == 0 ? "THE CREATURE TURNS TOWARD YOU..." : idx == 1 ? "WAIT - PARIN?!" : idx == 2 ? "IT IS PARINOSAURAS!" : "MISSION 3 COMPLETE";
            float cy = (Screen.height / k) * 0.36f;
            GUIStyle b = new GUIStyle(GUI.skin.label);
            b.fontSize = 34;
            b.alignment = TextAnchor.MiddleCenter;
            b.wordWrap = true;
            b.normal.textColor = idx == 3 ? new Color(1f, 0.95f, 0.6f) : Color.white;
            GUI.Label(new Rect(62, cy, 900, 50), msg, b);
            if (state == 2)
            {
                GUIStyle r = new GUIStyle(b);
                r.fontSize = 20;
                r.normal.textColor = Color.white;
                GUI.Label(new Rect(62, cy + 58, 900, 30), noM4 ? "Press R to restart" : "Your stomach growls...", r);
            }
        }
        GUI.matrix = old;
    }
}
