using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class TestM1toM2
{
    static double t0;
    static double lastDiag;
    static int stage;
    static GameObject player;
    static GameObject wormhole;

    [MenuItem("WrongHole/Verify M1 to M2")]
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
            if (EditorApplication.isPlaying && dt > 3.0)
            {
                Debug.Log("M1M2FLOW scene=" + SceneManager.GetActiveScene().name);
                stage = 1;
                t0 = EditorApplication.timeSinceStartup;
            }
            return;
        }
        if (stage == 1 && dt > 2.0)
        {
            foreach (CheetahAI c in UnityEngine.Object.FindObjectsByType<CheetahAI>(FindObjectsSortMode.None))
            {
                c.enabled = false;
            }
            Type gm = typeof(GameManager);
            FieldInfo fEnded = gm.GetField("ended", BindingFlags.NonPublic | BindingFlags.Static);
            if (fEnded != null) fEnded.SetValue(null, false);
            FieldInfo fWon = gm.GetField("won", BindingFlags.NonPublic | BindingFlags.Static);
            if (fWon != null) fWon.SetValue(null, false);
            FieldInfo fLoad = gm.GetField("loadAt", BindingFlags.NonPublic | BindingFlags.Static);
            if (fLoad != null) fLoad.SetValue(null, -1f);
            GameManager.MissionOver = false;
            player = GameObject.Find("Player");
            wormhole = GameObject.Find("Wormhole");
            if (player == null) { Debug.Log("M1M2FLOW no player"); EditorApplication.update -= Tick; stage = 9; return; }
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = new Vector3(0f, 3.5f, 25f);
            if (cc != null) cc.enabled = true;
            Debug.Log("M1M2FLOW dropped over pit");
            stage = 2;
            t0 = EditorApplication.timeSinceStartup;
            lastDiag = 0;
            return;
        }
        if (stage == 2)
        {
            string sn = SceneManager.GetActiveScene().name;
            if (sn == "Mission2")
            {
                GameObject b = GameObject.Find("adityosauras");
                Debug.Log("M1M2FLOW scene=" + sn + " baby=" + (b != null));
                EditorApplication.update -= Tick;
                stage = 9;
                return;
            }
            if (EditorApplication.timeSinceStartup - lastDiag > 2.0)
            {
                lastDiag = EditorApplication.timeSinceStartup;
                string pp = player != null ? player.transform.position.ToString("F1") : "gone";
                string dd = (player != null && wormhole != null) ? Vector3.Distance(player.transform.position, wormhole.transform.position).ToString("F1") : "?";
                Debug.Log("M1M2FLOW diag pos=" + pp + " dist=" + dd + " over=" + GameManager.MissionOver);
            }
            if (dt > 18.0)
            {
                Debug.Log("M1M2FLOW TIMEOUT scene=" + sn);
                EditorApplication.update -= Tick;
                stage = 9;
            }
        }
    }
}
