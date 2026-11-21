using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool MissionOver = false;
    public static bool Won = false;
    public static bool Epilogue = false;
    private static string endMessage = "";
    private static bool ended = false;
    private static bool won = false;
    private static float loadAt = -1f;

    void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        MissionOver = false;
        Won = false;
        ended = false;
        won = false;
        loadAt = -1f;
        endMessage = "";
    }

    public static void Caught()
    {
        End("THE CHEETAHS CAUGHT YOU", false);
    }

    public static void ReachedWormhole()
    {
        End("MISSION 1 COMPLETE - THE WORMHOLE PULLED YOU IN", true);
    }

    private static void End(string msg, bool isWin)
    {
        if (ended) return;
        ended = true;
        won = isWin;
        Won = isWin;
        MissionOver = true;
        endMessage = msg;
        if (isWin) loadAt = Time.time + 4f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (loadAt > 0f && Time.time >= loadAt)
        {
            loadAt = -1f;
            try
            {
                SceneManager.LoadScene("Mission2");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Mission2 load failed: " + e.Message);
            }
        }
        if (Epilogue && !ended && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Epilogue = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (ended && Keyboard.current != null && (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.homeKey.wasPressedThisFrame))
        {
            loadAt = -1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        float k = Screen.width / 1024f;
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(k, k, 1));
        GUIStyle h = new GUIStyle(GUI.skin.label);
        h.fontSize = 20;
        h.normal.textColor = Color.white;
        GUI.color = new Color(0, 0, 0, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, 1024, 36), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(14, 6, 1000, 28), "MISSION 1: ESCAPE THE FOREST - REACH THE WORMHOLE", h);
        if (Epilogue && !ended)
        {
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, 1024, Screen.height / k), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle e = new GUIStyle(GUI.skin.label);
            e.fontSize = 44;
            e.alignment = TextAnchor.MiddleCenter;
            e.normal.textColor = new Color(1f, 0.9f, 0.55f);
            float cy = (Screen.height / k) * 0.38f;
            GUI.Label(new Rect(62, cy, 900, 60), "WRONG HOLE - THE END(?)", e);
            GUIStyle sub = new GUIStyle(e);
            sub.fontSize = 20;
            sub.normal.textColor = Color.white;
            GUI.Label(new Rect(62, cy + 66, 900, 30), "Same forest. Tonight.", sub);
            GUIStyle r = new GUIStyle(e);
            r.fontSize = 17;
            GUI.Label(new Rect(62, cy + 104, 900, 26), "Press R to play again", r);
        }
        if (ended)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 30;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            style.normal.textColor = Color.white;
            float cy = (Screen.height / k) * 0.5f;
            GUI.Label(new Rect(62, cy - 50, 900, 46), endMessage, style);
            GUIStyle sub = new GUIStyle(style);
            sub.fontSize = 20;
            GUI.Label(new Rect(62, cy + 4, 900, 30), won ? "Entering the dinosaur era..." : "Press R to restart", sub);
        }
        GUI.matrix = old;
    }
}
