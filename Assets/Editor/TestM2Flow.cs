using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

public static class TestM2Flow
{
    [DllImport("user32.dll")] static extern void keybd_event(byte b, byte s, uint f, UIntPtr i);

    static double t0;
    static int stage;

    [MenuItem("WrongHole/Verify Mission 2 Flow")]
    static void Go()
    {
        t0 = EditorApplication.timeSinceStartup;
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
        if (stage == 1 && dt > 2.0)
        {
            GameObject p = GameObject.Find("Player");
            if (p == null) { Debug.Log("M2FLOW no player"); EditorApplication.update -= Tick; stage = 9; return; }
            p.transform.position = new Vector3(0, 1.2f, 0);
            p.transform.rotation = Quaternion.Euler(0, 20f, 0);
            Transform cam = p.transform.Find("PlayerCamera");
            if (cam != null) cam.localRotation = Quaternion.identity;
            Type gv = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            if (gv != null) { EditorWindow w = EditorWindow.GetWindow(gv); if (w != null) w.Focus(); }
            Debug.Log("M2FLOW ready");
            stage = 2;
            t0 = EditorApplication.timeSinceStartup;
            return;
        }
        if (stage == 2 && dt > 1.0)
        {
            keybd_event(0x57, 0x11, 0, UIntPtr.Zero);
            keybd_event(0xA0, 0x2A, 0, UIntPtr.Zero);
            stage = 3;
            t0 = EditorApplication.timeSinceStartup;
            return;
        }
        if (stage == 3 && dt > 5.0)
        {
            keybd_event(0x57, 0x11, 2, UIntPtr.Zero);
            keybd_event(0xA0, 0x2A, 2, UIntPtr.Zero);
            GameObject p = GameObject.Find("Player");
            GameObject b = GameObject.Find("adityosauras");
            float dist = (p != null && b != null) ? Vector3.Distance(p.transform.position, b.transform.position) : -1f;
            bool done = false;
            if (p != null)
            {
                Mission2Manager m = p.GetComponent<Mission2Manager>();
                if (m != null)
                {
                    FieldInfo f = typeof(Mission2Manager).GetField("done", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (f != null) done = (bool)f.GetValue(m);
                }
            }
            string pos = p != null ? p.transform.position.x.ToString("F1") + "," + p.transform.position.z.ToString("F1") : "?";
            Debug.Log("M2FLOW pos=(" + pos + ") dist=" + dist.ToString("F1") + " done=" + done);
            EditorApplication.update -= Tick;
            stage = 9;
        }
    }
}
