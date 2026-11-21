using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class TestStory
{
    [DllImport("user32.dll")] static extern void keybd_event(byte b, byte s, uint f, UIntPtr i);
    static bool wDown;
    static double lastF, t0, exitAt;
    static int stage;
    static string seen = "";

    [MenuItem("WrongHole/Play Story")]
    static void Go()
    {
        t0 = EditorApplication.timeSinceStartup;
        stage = 0;
        exitAt = -1;
        seen = "";
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Hatch.unity");
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    static void Hold(bool d)
    {
        if (d && !wDown) { keybd_event(0x57, 0x11, 0, UIntPtr.Zero); wDown = true; }
        if (!d && wDown) { keybd_event(0x57, 0x11, 2, UIntPtr.Zero); wDown = false; }
    }

    public static void Drive(double now)
    {
        GameObject p = GameObject.Find("Player");
        if (p == null) return;
        bool move = false;
        Vector3 t = StoryScene.Target;
        if (StoryScene.State == 0 && t != Vector3.zero)
        {
            Vector3 d = t - p.transform.position;
            d.y = 0f;
            if (d.magnitude > 1.2f)
            {
                p.transform.rotation = Quaternion.LookRotation(d);
                move = true;
            }
        }
        if (StoryScene.Prompt && now - lastF > 0.6)
        {
            lastF = now;
            keybd_event(0x46, 0x21, 0, UIntPtr.Zero);
            keybd_event(0x46, 0x21, 2, UIntPtr.Zero);
        }
        Hold(move);
    }

    static void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        double dt = now - t0;
        if (stage == 0)
        {
            if (EditorApplication.isPlaying && dt > 3.0)
            {
                Type gv = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
                if (gv != null) { EditorWindow w = EditorWindow.GetWindow(gv); if (w != null) w.Focus(); }
                Debug.Log("STORYTEST start");
                stage = 1;
                t0 = now;
            }
            return;
        }
        if (stage == 1)
        {
            string sn = SceneManager.GetActiveScene().name;
            if (sn != seen) { seen = sn; Debug.Log("STORYTEST scene=" + sn); }
            if (sn == "Hatch" || sn == "Care" || sn == "Cave") Drive(now);
            else Hold(false);
            if (sn == "Mission6")
            {
                Debug.Log("STORYTEST COMPLETE reached Mission6");
                Hold(false);
                stage = 2;
                exitAt = now + 6.0;
            }
            else if (dt > 150.0)
            {
                Hold(false);
                Debug.Log("STORYTEST TIMEOUT scene=" + sn + " state=" + StoryScene.State);
                stage = 2;
                exitAt = now + 3.0;
            }
            return;
        }
        if (stage == 2 && now > exitAt)
        {
            EditorApplication.update -= Tick;
            stage = 9;
            EditorApplication.ExitPlaymode();
        }
    }
}
