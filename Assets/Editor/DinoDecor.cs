using UnityEngine;
using UnityEditor;

public static class DinoDecor
{
    [MenuItem("WrongHole/Add Scene1 Trees To Dino Desert")]
    public static void Add()
    {
        if (GameObject.Find("S1Pine0") != null)
        {
            Debug.Log("DinoDecor already added");
            return;
        }
        GameObject pine = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/wh_pine_v2.blend");
        GameObject rocks = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/whrocks.blend");
        if (pine == null)
        {
            Debug.LogError("wh_pine_v2 not found");
            return;
        }
        System.Random rng = new System.Random(7);
        Vector3[] clear = { new Vector3(0f, 0f, 0f), new Vector3(20f, 0f, 42f), new Vector3(25f, 0f, 18f), new Vector3(-30f, 0f, 95f) };
        float[] clearR = { 9f, 9f, 10f, 13f };
        int placed = 0;
        int guard = 0;
        while (placed < 12 && guard++ < 200)
        {
            float x = (float)(rng.NextDouble() * 120.0 - 60.0);
            float z = (float)(rng.NextDouble() * 120.0 - 20.0);
            bool bad = false;
            for (int c = 0; c < clear.Length; c++)
            {
                float dx = x - clear[c].x;
                float dz = z - clear[c].z;
                if (dx * dx + dz * dz < clearR[c] * clearR[c]) { bad = true; break; }
            }
            if (bad) continue;
            GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(pine);
            t.name = "S1Pine" + placed;
            t.transform.position = new Vector3(x, 0f, z);
            t.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            float s = 0.85f + (float)rng.NextDouble() * 0.5f;
            t.transform.localScale = Vector3.one * s;
            placed++;
        }
        int rplaced = 0;
        guard = 0;
        while (rocks != null && rplaced < 6 && guard++ < 200)
        {
            float x = (float)(rng.NextDouble() * 120.0 - 60.0);
            float z = (float)(rng.NextDouble() * 120.0 - 20.0);
            bool bad = false;
            for (int c = 0; c < clear.Length; c++)
            {
                float dx = x - clear[c].x;
                float dz = z - clear[c].z;
                if (dx * dx + dz * dz < clearR[c] * clearR[c]) { bad = true; break; }
            }
            if (bad) continue;
            GameObject r = (GameObject)PrefabUtility.InstantiatePrefab(rocks);
            r.name = "S1Rock" + rplaced;
            r.transform.position = new Vector3(x, 0f, z);
            r.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            rplaced++;
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("DinoDecor placed " + placed + " pines and " + rplaced + " rock groups");
    }
}
