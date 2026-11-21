using UnityEngine;
using UnityEditor;

public static class TestM2
{
    static double t0;
    static int stage;

    [MenuItem("WrongHole/Verify Mission 2")]
    static void Go()
    {
        stage = 0;
        t0 = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
        if (!EditorApplication.isPlaying) EditorApplication.EnterPlaymode();
    }

    static void Tick()
    {
        double dt = EditorApplication.timeSinceStartup - t0;
        if (stage == 0 && EditorApplication.isPlaying && dt > 6.0)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null)
            {
                p.transform.position = new Vector3(8.5f, 1.2f, 21f);
                p.transform.rotation = Quaternion.Euler(0, 18f, 0);
                Transform cam = p.transform.Find("PlayerCamera");
                if (cam != null) cam.localRotation = Quaternion.identity;
                Debug.Log("M2VERIFY positioned");
                stage = 1;
                t0 = EditorApplication.timeSinceStartup;
            }
            else if (dt > 25.0)
            {
                Debug.Log("M2VERIFY no player found");
                stage = 9;
                EditorApplication.update -= Tick;
            }
        }
        else if (stage == 1 && dt > 2.5)
        {
            Debug.Log("M2VERIFY done");
            EditorApplication.update -= Tick;
        }
    }
}
