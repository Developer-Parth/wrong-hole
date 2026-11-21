using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class TestPlayM1toM2
{
    [DllImport("user32.dll")] static extern void keybd_event(byte b, byte s, uint f, UIntPtr i);

    static double t0;
    static int stage;
    static bool keysDown;

    [MenuItem("WrongHole/Play M1 to M2")]
    static void Go()
    {
        t0 = EditorApplication.timeSinceStartup;
        keysDown = false;
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
            EditorApplication.EnterPlaymode();
        }
    }

    static void KeysUp()
    {
        if (!keysDown) return;
        keybd_event(0x57, 0x11, 2, UIntPtr.Zero);
        keybd_event(0xA0, 0x2A, 2, UIntPtr.Zero);
        keysDown = false;
    }

    static void Tick()
    {
        double dt = EditorApplication.timeSinceStartup - t0;
        if (stage == -1)
        {
            if (!EditorApplication.isPlaying)
            {
                stage = 0;
                t0 = EditorApplication.timeSinceStartup;
                EditorApplication.EnterPlaymode();
            }
            return;
        }
        if (stage == 0)
        {
            if (EditorApplication.isPlaying && dt > 3.0) { stage = 1; t0 = EditorApplication.timeSinceStartup; }
            return;
        }
        if (stage == 1)
        {
            Type gv = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            if (gv != null) { EditorWindow w = EditorWindow.GetWindow(gv); if (w != null) w.Focus(); }
            Debug.Log("PLAYM1M2 running");
            stage = 2;
            t0 = EditorApplication.timeSinceStartup;
            return;
        }
        if (stage == 2 && dt > 1.0)
        {
            keybd_event(0x57, 0x11, 0, UIntPtr.Zero);
            keybd_event(0xA0, 0x2A, 0, UIntPtr.Zero);
            keysDown = true;
            stage = 3;
            t0 = EditorApplication.timeSinceStartup;
            return;
        }
        if (stage == 3)
        {
            string sn = SceneManager.GetActiveScene().name;
            if (sn == "Mission2")
            {
                KeysUp();
                GameObject b = GameObject.Find("Daaminosauras");
                Debug.Log("PLAYM1M2 scene=Mission2 baby=" + (b != null));
                EditorApplication.update -= Tick;
                stage = 9;
                return;
            }
            if (dt > 25.0)
            {
                KeysUp();
                GameObject p = GameObject.Find("Player");
                GameObject wh = GameObject.Find("Wormhole");
                string pp = p != null ? p.transform.position.ToString("F1") : "gone";
                string dd = (p != null && wh != null) ? Vector3.Distance(p.transform.position, wh.transform.position).ToString("F1") : "?";
                Debug.Log("PLAYM1M2 TIMEOUT scene=" + sn + " pos=" + pp + " dist=" + dd + " over=" + GameManager.MissionOver);
                EditorApplication.update -= Tick;
                stage = 9;
            }
        }
    }
}
