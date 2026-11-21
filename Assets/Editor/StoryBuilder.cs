using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class StoryBuilder
{
    static Material Mat(Color c, float smooth, Color e)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (e.a > 0f && m.HasProperty("_EmissionColor"))
        {
            m.SetColor("_EmissionColor", e);
            m.EnableKeyword("_EMISSION");
        }
        return m;
    }

    static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Material m)
    {
        GameObject g = GameObject.CreatePrimitive(t);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    static GameObject Get(string n)
    {
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
            if (t.name == n && t.gameObject.scene.IsValid()) return t.gameObject;
        return null;
    }

    static GameObject Fresh(string name)
    {
        Scene s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(s, "Assets/Scenes/" + name + ".unity");
        DinoBuilder.Build();
        GameObject player = GameObject.Find("Player");
        Mission2Manager m2 = player.GetComponent<Mission2Manager>();
        if (m2 != null) Object.DestroyImmediate(m2);
        return player;
    }

    static void Place(GameObject p, Vector3 pos, Vector3 look)
    {
        p.transform.position = pos;
        Vector3 d = look - pos;
        d.y = 0f;
        p.transform.rotation = Quaternion.LookRotation(d);
    }

    static void Save(string name, GameObject player)
    {
        player.AddComponent<StoryScene>();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/" + name + ".unity");
        Debug.Log(name + " scene built");
    }

    public static void BuildAll()
    {
        Color none = new Color(0, 0, 0, 0);
        GameObject p = Fresh("Hatch");
        Get("parinosauras").SetActive(false);
        Material eggM = Mat(new Color(0.92f, 0.88f, 0.74f), 0.55f, new Color(0.25f, 0.2f, 0.1f, 1f));
        Material rockM = Mat(new Color(0.4f, 0.37f, 0.33f), 0.2f, none);
        for (int i = 0; i < 8; i++)
        {
            float a = i * 0.785f;
            Prim(PrimitiveType.Sphere, "NestRock", new Vector3(16f + Mathf.Cos(a) * 2.3f, 0.3f, 30f + Mathf.Sin(a) * 2.3f), Vector3.one * 0.8f, rockM);
        }
        Prim(PrimitiveType.Sphere, "StoryEgg", new Vector3(16f, 1.05f, 30f), new Vector3(1.5f, 2.0f, 1.5f), eggM);
        Place(p, new Vector3(16f, 1.2f, 20f), new Vector3(16f, 0f, 30f));
        Save("Hatch", p);

        p = Fresh("Care");
        Get("parinosauras").SetActive(false);
        GameObject b = Get("adityosauras");
        b.SetActive(true);
        b.transform.position = new Vector3(16f, 0f, 30f);
        Material berryM = Mat(new Color(0.8f, 0.1f, 0.2f), 0.6f, new Color(0.5f, 0.05f, 0.1f, 1f));
        Vector3[] bp = { new Vector3(11f, 0.4f, 24f), new Vector3(23f, 0.4f, 27f), new Vector3(13f, 0.4f, 37f) };
        for (int i = 0; i < 3; i++) Prim(PrimitiveType.Sphere, "Item" + i, bp[i], Vector3.one * 0.55f, berryM);
        Place(p, new Vector3(16f, 1.2f, 22f), new Vector3(16f, 0f, 30f));
        Save("Care", p);

        p = Fresh("Cave");
        Vector3 c = new Vector3(300f, 0f, 300f);
        Material wallM = Mat(new Color(0.3f, 0.26f, 0.23f), 0.1f, none);
        Material floorM = Mat(new Color(0.34f, 0.3f, 0.26f), 0.1f, none);
        Material plugM = Mat(new Color(0.42f, 0.36f, 0.3f), 0.05f, none);
        Prim(PrimitiveType.Cube, "CaveFloor", c + new Vector3(0, -0.25f, 0), new Vector3(26, 0.5f, 26), floorM);
        Prim(PrimitiveType.Cube, "CaveCeil", c + new Vector3(0, 8f, 0), new Vector3(26, 1f, 26), wallM);
        Prim(PrimitiveType.Cube, "CaveW1", c + new Vector3(0, 4f, 13.5f), new Vector3(27, 9, 1), wallM);
        Prim(PrimitiveType.Cube, "CaveW2", c + new Vector3(0, 4f, -13.5f), new Vector3(27, 9, 1), wallM);
        Prim(PrimitiveType.Cube, "CaveW3", c + new Vector3(13.5f, 4f, 0), new Vector3(1, 9, 27), wallM);
        Prim(PrimitiveType.Cube, "CaveW4", c + new Vector3(-13.5f, 4f, 0), new Vector3(1, 9, 27), wallM);
        System.Random r = new System.Random(5);
        for (int i = 0; i < 10; i++)
        {
            float x = (float)(r.NextDouble() * 22 - 11), z = (float)(r.NextDouble() * 10 - 11);
            float s = (float)(r.NextDouble() * 1.4 + 0.5);
            Prim(PrimitiveType.Sphere, "CaveRock", c + new Vector3(x, s * 0.3f, z), new Vector3(s * 1.4f, s * 0.8f, s), wallM);
        }
        for (int i = 0; i < 5; i++)
            Prim(PrimitiveType.Cube, "Item" + i, c + new Vector3(-10f + i * 5f, 4f, 2f), new Vector3(5f, 8f, 3f), plugM);
        Vector3[] lp = { new Vector3(-7, 5, -6), new Vector3(7, 5, -7) };
        for (int i = 0; i < lp.Length; i++)
        {
            GameObject l = new GameObject("CaveLight");
            l.transform.position = c + lp[i];
            Light li = l.AddComponent<Light>();
            li.type = LightType.Point;
            li.color = new Color(1f, 0.7f, 0.4f);
            li.intensity = 6f;
            li.range = 22f;
        }
        Vector3[] bl = { new Vector3(-7, 4, 8), new Vector3(0, 3, 9), new Vector3(7, 4, 8) };
        for (int i = 0; i < bl.Length; i++)
        {
            GameObject l = new GameObject("BackLight" + i);
            l.transform.position = c + bl[i];
            Light li = l.AddComponent<Light>();
            li.type = LightType.Point;
            li.color = new Color(1f, 0.6f, 0.3f);
            li.intensity = 7f;
            li.range = 18f;
            l.SetActive(false);
        }
        Material oreM = Mat(new Color(0.95f, 0.75f, 0.2f), 0.8f, new Color(1.6f, 1.1f, 0.2f, 1f));
        Material hayM = Mat(new Color(0.75f, 0.62f, 0.3f), 0.1f, none);
        Material fireM = Mat(new Color(1f, 0.5f, 0.1f), 0.2f, new Color(2.5f, 1.1f, 0.2f, 1f));
        Prim(PrimitiveType.Sphere, "Fire", c + new Vector3(0f, 0.5f, 9f), new Vector3(0.8f, 1f, 0.8f), fireM);
        for (int i = 0; i < 7; i++)
        {
            float a = i * 0.9f;
            Prim(PrimitiveType.Sphere, "FireRock", c + new Vector3(Mathf.Cos(a) * 0.9f, 0.2f, 9f + Mathf.Sin(a) * 0.9f), Vector3.one * 0.4f, wallM);
        }
        Prim(PrimitiveType.Sphere, "Hay", c + new Vector3(-5f, 0.15f, 9f), new Vector3(4f, 0.3f, 3f), hayM);
        for (int i = 0; i < 6; i++)
            Prim(PrimitiveType.Sphere, "Ore" + i, c + new Vector3(-9f + i * 3.4f, 0.7f, 12.2f), new Vector3(0.7f, 1.0f, 0.7f), oreM);
        GameObject mum = Get("parinosauras");
        mum.SetActive(true);
        mum.transform.position = c + new Vector3(7f, 0f, -5f);
        GameObject baby = Get("adityosauras");
        baby.SetActive(true);
        baby.transform.position = c + new Vector3(3f, 0f, -6f);
        new GameObject("SettleMum").transform.position = c + new Vector3(5f, 0f, 8f);
        new GameObject("SettleBaby").transform.position = c + new Vector3(-2.5f, 0f, 9.5f);
        Place(p, c + new Vector3(0f, 1.2f, -9f), c + new Vector3(0f, 0f, 9f));
        Save("Cave", p);
    }
}
