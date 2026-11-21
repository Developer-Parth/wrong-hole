using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission2Manager : MonoBehaviour
{
    Transform baby;
    bool done;
    bool noM3;
    float loadAt = -1f;
    void Start()
    {
        GameObject b = GameObject.Find("Sukhdevosauras");
        if (b != null) baby = b.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (!done && baby != null && Vector3.Distance(transform.position, baby.position) < 9f)
        {
            done = true;
            loadAt = Time.time + 4f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (loadAt > 0f && Time.time >= loadAt)
        {
            loadAt = -1f;
            bool found = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (SceneUtility.GetScenePathByBuildIndex(i).EndsWith("/Mission3.unity")) { found = true; break; }
            }
            if (found) SceneManager.LoadScene("Mission3");
            else noM3 = true;
        }
        if (done && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
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
        GUI.color = new Color(0, 0, 0, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, 1024, 36), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 4, 1024, 28), "MISSION 2: EXPLORE THE DINOSAUR ERA - FIND THE DINOSAURS", s);
        if (done)
        {
            float cy = (Screen.height / k) * 0.34f;
            GUIStyle b = new GUIStyle(GUI.skin.label);
            b.fontSize = 32;
            b.alignment = TextAnchor.MiddleCenter;
            b.wordWrap = true;
            b.normal.textColor = new Color(1f, 0.95f, 0.6f);
            GUI.Label(new Rect(62, cy, 900, 46), "A HUGE CREATURE - SUKHDEVOSAURAS", b);
            GUIStyle r = new GUIStyle(b);
            r.fontSize = 20;
            r.normal.textColor = Color.white;
            GUI.Label(new Rect(62, cy + 52, 900, 30), noM3 ? "Press R to restart" : "The creature wants a closer look at you...", r);
        }
        GUI.matrix = old;
    }
}
