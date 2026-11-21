using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class TestPlayChains
{
    [DllImport("user32.dll")] static extern void keybd_event(byte b, byte s, uint f, UIntPtr i);
    [DllImport("user32.dll")] static extern void mouse_event(uint f, uint dx, uint dy, uint d, UIntPtr i);

    static double t0;
    static int stage;
    static bool curW;
    static bool curShift;
    static string startScene;
    static string lastScene;
    static int mode;
    static bool midLogged;
    static double lastF;
    static bool sawFail;
    static bool rSent;
    static double rAt;
    static double exitAt;
    static bool replay;
    static double replayAt;

    [MenuItem("WrongHole/Play M1 to M3")]
    static void GoM13() { Begin("Assets/Scenes/Mission1.unity", 3); }

    [MenuItem("WrongHole/Play M3")]
    static void GoM3() { Begin("Assets/Scenes/Mission3.unity", 3); }

    [MenuItem("WrongHole/Play M4")]
    static void GoM4() { Begin("Assets/Scenes/Mission4.unity", 4); }

    [MenuItem("WrongHole/Play M5")]
    static void GoM5() { Begin("Assets/Scenes/Mission5.unity", 5); }

    [MenuItem("WrongHole/Play M6")]
    static void GoM6() { Begin("Assets/Scenes/Mission6.unity", 6); }

    [MenuItem("WrongHole/Play M5 Exposed")]
    static void GoM5X() { Begin("Assets/Scenes/Mission5.unity", 7); }

    [MenuItem("WrongHole/Play Full")]
    static void GoFull() { Begin("Assets/Scenes/Mission1.unity", 9); }

    [MenuItem("WrongHole/Abort Chain")] static void GoAbort(){ EditorApplication.update -= Tick; SetKeys(false,false); stage=9; Debug.Log("PLAYCHAIN ABORTED"); }

    static void Begin(string scenePath, int m)
    {
        t0 = EditorApplication.timeSinceStartup;
        curW = false;
        curShift = false;
        startScene = scenePath;
        lastScene = "";
        mode = m;
        GameManager.Epilogue = false;
        midLogged = false;
        lastF = 0;
        sawFail = false;
        rSent = false;
        replay = false;
        exitAt = -1;
        if (EditorApplication.isPlaying)
        {
            stage = -1;
            EditorApplication.update += Tick;
            EditorApplication.ExitPlaymode();
        }
        else
        {
            stage = 0;
            EditorApplication.update += Tick;
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(startScene);
            EditorApplication.EnterPlaymode();
        }
    }

    static void SetKeys(bool w, bool s)
    {
        if (w && !curW) { keybd_event(0x57, 0x11, 0, UIntPtr.Zero); curW = true; }
        if (!w && curW) { keybd_event(0x57, 0x11, 2, UIntPtr.Zero); curW = false; }
        if (s && !curShift) { keybd_event(0xA0, 0x2A, 0, UIntPtr.Zero); curShift = true; }
        if (!s && curShift) { keybd_event(0xA0, 0x2A, 2, UIntPtr.Zero); curShift = false; }
    }

    static void Finish(string why)
    {
        SetKeys(false, false);
        Debug.Log(why);
        exitAt = EditorApplication.timeSinceStartup + 8.0;
        stage = 5;
    }

    static void KeyTap(byte vk, byte sc)
    {
        keybd_event(vk, sc, 0, UIntPtr.Zero);
        keybd_event(vk, sc, 2, UIntPtr.Zero);
    }

    static void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        double dt = now - t0;
        if (stage == -1)
        {
            if (!EditorApplication.isPlaying)
            {
                stage = 0;
                t0 = now;
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(startScene);
                EditorApplication.EnterPlaymode();
            }
            return;
        }
        if (stage == 0)
        {
            if (EditorApplication.isPlaying && dt > 3.0) { stage = 1; t0 = now; }
            return;
        }
        if (stage == 1)
        {
            Type gv = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            if (gv != null) { EditorWindow w = EditorWindow.GetWindow(gv); if (w != null) w.Focus(); }
            Debug.Log("PLAYCHAIN running from=" + SceneManager.GetActiveScene().name + " mode=" + mode);
            stage = 2;
            t0 = now;
            return;
        }
        if (stage == 2)
        {
            if (dt > 1.0)
            {
                if (mode == 6) SetKeys(true, false);
                else if (mode == 7) SetKeys(false, false);
                else SetKeys(true, true);
                stage = 3;
                t0 = now;
            }
            return;
        }
        if (stage == 3)
        {
            string sn = SceneManager.GetActiveScene().name;
            if (sn != lastScene)
            {
                lastScene = sn;
                midLogged = false;
                Debug.Log("PLAYCHAIN scene=" + sn);
            }
            if (mode == 6 && (Mission6Manager.State == 3 || (sn == "Mission1" && Mission6Manager.State >= 2)))
            {
                Finish("PLAYCHAIN complete scene=Mission6");
                return;
            }
            if (mode == 9 && sn == "Mission1" && GameManager.Epilogue && !replay)
            {
                if (!rSent) { rSent = true; rAt = now + 2.5; SetKeys(false, false); Debug.Log("PLAYCHAIN FULL COMPLETE epilogue shown"); }
                else if (now > rAt) { replay = true; replayAt = now; KeyTap(0x52, 0x13); Debug.Log("PLAYCHAIN R tapped at epilogue"); }
                return;
            }
            if (mode == 9 && replay)
            {
                if (sn == "Mission1" && !GameManager.Epilogue && !GameManager.MissionOver) { SetKeys(true, true); if (!midLogged) { midLogged = true; Debug.Log("PLAYCHAIN replay started epilogue cleared"); } }
                if (sn == "Mission2") { Finish("PLAYCHAIN REPLAY OK reached Mission2 again"); return; }
                if (now - replayAt > 40.0) { Finish("PLAYCHAIN REPLAY TIMEOUT scene=" + sn + " epi=" + GameManager.Epilogue); return; }
                return;
            }
            if ((mode == 9 || mode == 1) && sn == "Mission1" && GameManager.MissionOver && !GameManager.Won)
            {
                Finish("PLAYCHAIN FAIL scene=Mission1 caught");
                return;
            }
            if ((mode == 4 || mode == 5 || mode == 9) && sn == "Mission4" && Mission4Manager.State == 9)
            {
                Finish("PLAYCHAIN FAIL scene=Mission4");
                return;
            }
            if ((mode == 5 || mode == 9) && sn == "Mission5" && Mission5Manager.State == 9)
            {
                Finish("PLAYCHAIN FAIL scene=Mission5");
                return;
            }
            if (mode == 4 && sn == "Mission4")
            {
                if (Mission4Manager.State < 2) SetKeys(true, true);
                else { Finish("PLAYCHAIN complete scene=Mission4"); return; }
            }
            if (mode == 9 && sn == "Mission4" && Mission4Manager.State < 2) SetKeys(true, true);
            if ((mode == 5 || mode == 9) && sn == "Mission5")
            {
                if (Mission5Manager.State >= 1) SetKeys(false, false);
                else SetKeys(true, true);
                if (mode == 5 && Mission5Manager.State == 2) { Finish("PLAYCHAIN complete scene=Mission5"); return; }
            }
            if (mode == 9 && sn == "Mission2") SetKeys(true, true);
            if ((mode == 3 || mode == 9) && sn == "Mission3")
            {
                if (Mission3Manager.State >= 1)
                {
                    SetKeys(false, false);
                    if (!midLogged) { midLogged = true; Debug.Log("PLAYCHAIN mid scene=Mission3"); }
                }
                if (mode == 3 && Mission3Manager.State >= 1) { stage = 4; t0 = now; return; }
            }
            if (mode == 9 && (sn == "Hatch" || sn == "Care" || sn == "Cave")) { TestStory.Drive(now); return; } if ((mode == 6 || mode == 9) && sn == "Mission6")
            {
                if (Mission6Manager.State < 2)
                {
                    SetKeys(true, false);
                    if (Mission6Manager.PromptVisible && now - lastF > 0.6)
                    {
                        lastF = now;
                        KeyTap(0x46, 0x21);
                    }
                }
                else SetKeys(false, false);
            }
            if (mode == 7 && sn == "Mission5")
            {
                if (!sawFail)
                {
                    if (Mission5Manager.State == 9)
                    {
                        sawFail = true;
                        SetKeys(false, false);
                        Debug.Log("PLAYCHAIN M5 caught OK");
                        rAt = now + 2.0;
                    }
                    else
                    {
                        GameObject p = GameObject.Find("Player");
                        GameObject m = GameObject.Find("parinosauras");
                        if (p != null && m != null)
                        {
                            Vector3 d = m.transform.position - p.transform.position;
                            float desired = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                            float err = Mathf.DeltaAngle(p.transform.eulerAngles.y, desired);
                            int dx = (int)Mathf.Clamp(err * 8f, -30f, 30f);
                            if (dx != 0) mouse_event(1, (uint)dx, 0, 0, UIntPtr.Zero);
                        }
                        SetKeys(true, true);
                    }
                }
                else if (!rSent)
                {
                    if (now > rAt)
                    {
                        rSent = true;
                        KeyTap(0x52, 0x13);
                        Debug.Log("PLAYCHAIN M5 R sent");
                    }
                }
                else if (Mission5Manager.State == 0)
                {
                    Finish("PLAYCHAIN M5 restart OK");
                    return;
                }
            }
            bool mid = false, done = false;
            if (mode == 4 && sn == "Mission4") { mid = Mission4Manager.State >= 1; done = Mission4Manager.State == 2; }
            if (mode == 5 && sn == "Mission5") { mid = Mission5Manager.State >= 1; done = Mission5Manager.State == 2; }
            if (mid && !midLogged)
            {
                midLogged = true;
                Debug.Log("PLAYCHAIN mid scene=" + sn);
            }
            if (done && mode == 4) { return; }
            double limit = mode == 9 ? 300.0 : (mode == 7 ? 90.0 : 60.0);
            if (dt > limit)
            {
                GameObject p = GameObject.Find("Player");
                string pp = p != null ? p.transform.position.ToString("F1") : "gone";
                Finish("PLAYCHAIN TIMEOUT scene=" + sn + " pos=" + pp);
            }
            return;
        }
        if (stage == 4)
        {
            if (Mission3Manager.State == 2)
            {
                Finish("PLAYCHAIN complete scene=Mission3");
                return;
            }
            if (dt > 15.0)
            {
                Finish("PLAYCHAIN reveal TIMEOUT m3state=" + Mission3Manager.State);
            }
            return;
        }
        if (stage == 5)
        {
            if (exitAt > 0 && now > exitAt)
            {
                EditorApplication.update -= Tick;
                stage = 9;
                EditorApplication.ExitPlaymode();
            }
        }
    }
}
