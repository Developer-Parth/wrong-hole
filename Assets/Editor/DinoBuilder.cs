using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class DinoBuilder
{
    static Material Mat(Color c, float smooth = 0.25f)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        return m;
    }
    static Material GlowMat(Color c, Color e)
    {
        Material m = Mat(c);
        if (m.HasProperty("_EmissionColor"))
        {
            m.SetColor("_EmissionColor", e);
            m.EnableKeyword("_EMISSION");
        }
        return m;
    }
    static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Material m, Transform parent = null)
    {
        GameObject g = GameObject.CreatePrimitive(t);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (parent != null) g.transform.SetParent(parent, true);
        return g;
    }
    static void MakeTransparent(Material m)
    {
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        if (m.HasProperty("_Surface")) { m.SetFloat("_Surface", 1f); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); }
        else m.EnableKeyword("_ALPHABLEND_ON");
        m.renderQueue = 3000;
    }
    static bool NearPath(float x, float z)
    {
        float bx = 18f, bz = 40f;
        float t = (x * bx + z * bz) / (bx * bx + bz * bz);
        t = Mathf.Clamp01(t);
        float dx = x - t * bx, dz = z - t * bz;
        return dx * dx + dz * dz < 49f;
    }
    [MenuItem("WrongHole/Build Mission 2")]
    public static void Build()
    {
        foreach (GameObject o in SceneManager.GetActiveScene().GetRootGameObjects())
            if (o.transform.parent == null) Object.DestroyImmediate(o);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.5f, 0.42f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.72f, 0.66f, 0.5f);
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance = 130f;
        Camera.main?.gameObject.SetActive(false);
        GameObject sun = new GameObject("Sun");
        Light sl = sun.AddComponent<Light>();
        sl.type = LightType.Directional;
        sl.color = new Color(1f, 0.9f, 0.72f);
        sl.intensity = 1.25f;
        sl.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(38f, -125f, 0f);
        Material groundM = Mat(new Color(0.62f, 0.52f, 0.34f));
        Prim(PrimitiveType.Cube, "Ground", new Vector3(0, -0.5f, 40), new Vector3(140, 1, 140), groundM);
        Material rockM = Mat(new Color(0.45f, 0.42f, 0.38f));
        System.Random rng = new System.Random(7);
        for (int i = 0; i < 14; i++)
        {
            float rx = (float)(rng.NextDouble() * 120 - 55);
            float rz = (float)(rng.NextDouble() * 110 - 15);
            float rs = (float)(rng.NextDouble() * 2.2 + 0.8);
            if (NearPath(rx, rz)) continue;
            Prim(PrimitiveType.Sphere, "Rock", new Vector3(rx, rs * 0.28f, rz), new Vector3(rs, rs * 0.62f, rs * 0.85f), rockM);
        }
        Material fernM = Mat(new Color(0.2f, 0.42f, 0.16f));
        Material trunkM = Mat(new Color(0.42f, 0.3f, 0.18f));
        for (int i = 0; i < 44; i++)
        {
            float fx = (float)(rng.NextDouble() * 80 - 35);
            float fz = (float)(rng.NextDouble() * 75 - 8);
            GameObject fs = Prim(PrimitiveType.Cylinder, "FernStem", new Vector3(fx, 0.5f, fz), new Vector3(0.12f, 0.5f, 0.12f), trunkM);
            Object.DestroyImmediate(fs.GetComponent<Collider>());
            GameObject ft = Prim(PrimitiveType.Sphere, "FernTop", new Vector3(fx, 1.15f, fz), new Vector3(1.3f, 0.4f, 1.3f), fernM);
            Object.DestroyImmediate(ft.GetComponent<Collider>());
        }
        for (int i = 0; i < 18; i++)
        {
            float px = (float)(rng.NextDouble() * 85 - 38);
            float pz = (float)(rng.NextDouble() * 78 - 6);
            float ph = (float)(rng.NextDouble() * 2 + 3.2f);
            if (NearPath(px, pz)) continue;
            Prim(PrimitiveType.Cylinder, "PalmTrunk", new Vector3(px, ph / 2, pz), new Vector3(0.35f, ph / 2, 0.35f), trunkM);
            Prim(PrimitiveType.Sphere, "PalmTop", new Vector3(px, ph + 0.4f, pz), new Vector3(2.6f, 0.7f, 2.6f), Mat(new Color(0.24f, 0.5f, 0.2f)));
        }
        Material bumpM = Mat(new Color(0.55f, 0.45f, 0.3f));
        for (int i = 0; i < 9; i++)
        {
            float mx = (float)(rng.NextDouble() * 130 - 60);
            float mz = (float)(rng.NextDouble() * 120 - 20);
            float mr = (float)(rng.NextDouble() * 7 + 5);
            if (NearPath(mx, mz)) continue;
            Prim(PrimitiveType.Sphere, "Mound", new Vector3(mx, -mr * 0.18f, mz), new Vector3(mr, mr * 0.55f, mr), bumpM);
        }
        Material grassA = Mat(new Color(0.3f, 0.5f, 0.18f));
        Material grassB = Mat(new Color(0.38f, 0.55f, 0.2f));
        for (int i = 0; i < 150; i++)
        {
            float gx = (float)(rng.NextDouble() * 95 - 42);
            float gz = (float)(rng.NextDouble() * 85 - 12);
            if (NearPath(gx, gz)) continue;
            for (int b = 0; b < 4; b++)
            {
                float gh = (float)(rng.NextDouble() * 0.55 + 0.45);
                float bx = gx + (float)(rng.NextDouble() * 0.9 - 0.45);
                float bz = gz + (float)(rng.NextDouble() * 0.9 - 0.45);
                GameObject gb = Prim(PrimitiveType.Cylinder, "Grass", new Vector3(bx, gh / 2f, bz), new Vector3(0.11f, gh / 2f, 0.11f), (i + b) % 2 == 0 ? grassA : grassB);
                Object.DestroyImmediate(gb.GetComponent<Collider>());
            }
        }
        Material patchM = Mat(new Color(0.5f, 0.4f, 0.26f));
        for (int i = 0; i < 26; i++)
        {
            float qx = (float)(rng.NextDouble() * 95 - 42);
            float qz = (float)(rng.NextDouble() * 85 - 12);
            if (NearPath(qx, qz)) continue;
            float qs = (float)(rng.NextDouble() * 4 + 2.5f);
            Prim(PrimitiveType.Cube, "Patch", new Vector3(qx, 0.03f, qz), new Vector3(qs, 0.05f, qs * 0.7f), patchM);
        }
        Material shrubM = Mat(new Color(0.16f, 0.36f, 0.14f));
        Material shrubM2 = Mat(new Color(0.22f, 0.44f, 0.16f));
        for (int i = 0; i < 30; i++)
        {
            float hx = (float)(rng.NextDouble() * 90 - 40);
            float hz = (float)(rng.NextDouble() * 80 - 10);
            if (NearPath(hx, hz)) continue;
            float hs = (float)(rng.NextDouble() * 1.1 + 0.7);
            GameObject hb = Prim(PrimitiveType.Sphere, "Shrub", new Vector3(hx, hs * 0.35f, hz), new Vector3(hs, hs * 0.62f, hs), i % 2 == 0 ? shrubM : shrubM2);
            Object.DestroyImmediate(hb.GetComponent<Collider>());
        }
        Material silM = Mat(new Color(0.3f, 0.26f, 0.18f));
        for (int i = 0; i < 14; i++)
        {
            float sx = -65f + i * 10f;
            float sz = 96f + (float)(rng.NextDouble() * 10);
            float sh = (float)(rng.NextDouble() * 3 + 5);
            Prim(PrimitiveType.Cylinder, "FarTrunk", new Vector3(sx, sh / 2f, sz), new Vector3(0.5f, sh / 2f, 0.5f), silM);
            Prim(PrimitiveType.Sphere, "FarTop", new Vector3(sx, sh + 0.8f, sz), new Vector3(4f, 1.2f, 4f), silM);
        }
        Material boneM = Mat(new Color(0.85f, 0.82f, 0.72f));
        for (int i = 0; i < 6; i++)
        {
            float ox = -30f + (float)(rng.NextDouble() * 24 - 12);
            float oz = 78f + (float)(rng.NextDouble() * 12);
            GameObject bo = Prim(PrimitiveType.Cylinder, "Bone", new Vector3(ox, 0.12f, oz), new Vector3(0.14f, 0.7f, 0.14f), boneM);
            bo.transform.rotation = Quaternion.Euler(88f, (float)(rng.NextDouble() * 360), 0f);
        }
        Material volcM = Mat(new Color(0.3f, 0.24f, 0.2f));
        Prim(PrimitiveType.Cylinder, "Volcano", new Vector3(-30, 12, 95), new Vector3(16, 12, 16), volcM);
        Prim(PrimitiveType.Sphere, "VolcanoGlow", new Vector3(-30, 24.4f, 95), new Vector3(5, 1.6f, 5), GlowMat(new Color(0.9f, 0.35f, 0.1f), new Color(1.6f, 0.55f, 0.12f)));
        Prim(PrimitiveType.Cube, "Pond", new Vector3(25, 0.02f, 18), new Vector3(14, 0.05f, 10), Mat(new Color(0.25f, 0.5f, 0.62f), 0.8f));
        for (int i = 0; i < 14; i++)
        {
            float a2 = i * 0.449f;
            float rx2 = 25 + Mathf.Cos(a2) * (7.4f + (float)(rng.NextDouble() * 1.6));
            float rz2 = 18 + Mathf.Sin(a2) * (5.4f + (float)(rng.NextDouble() * 1.6));
            float rh = (float)(rng.NextDouble() * 0.5 + 0.9);
            GameObject rd = Prim(PrimitiveType.Cylinder, "Reed", new Vector3(rx2, rh / 2f, rz2), new Vector3(0.06f, rh / 2f, 0.06f), grassA);
            Object.DestroyImmediate(rd.GetComponent<Collider>());
        }
        Material dinoM = Mat(new Color(0.38f, 0.48f, 0.28f));
        Material bellyM = Mat(new Color(0.62f, 0.6f, 0.42f));
        PlaceRealDino("parinosauras", "Assets/Models/dinos/Apatosaurus/Apatosaurus.fbx", new Vector3(15, 0, 35), 5f, dinoM, bellyM);
        BuildDino("adityosauras", new Vector3(10, 0, 29), 0.42f, dinoM, bellyM); GameObject.Find("adityosauras").SetActive(false);
        Material eggM = Mat(new Color(0.9f, 0.86f, 0.72f, 0.55f), 0.5f); MakeTransparent(eggM);
        for (int i = 0; i < 6; i++)
        {
            float a = i * 1.047f;
            Prim(PrimitiveType.Sphere, "NestRock", new Vector3(20 + Mathf.Cos(a) * 1.6f, 0.25f, 42 + Mathf.Sin(a) * 1.6f), Vector3.one * 0.55f, rockM);
        }
        Prim(PrimitiveType.Sphere, "Egg1", new Vector3(19.5f, 0.5f, 42), new Vector3(0.7f, 0.95f, 0.7f), eggM); EggInside(new Vector3(19.5f, 0.5f, 42));
        Prim(PrimitiveType.Sphere, "Egg2", new Vector3(20.7f, 0.45f, 42.3f), new Vector3(0.6f, 0.8f, 0.6f), eggM); EggInside(new Vector3(20.7f, 0.45f, 42.3f));
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, 1.2f, 0);
        player.transform.rotation = Quaternion.Euler(0, 20f, 0);
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.9f, 0);
        GameObject camGO = new GameObject("PlayerCamera");
        camGO.transform.SetParent(player.transform, false);
        camGO.transform.localPosition = new Vector3(0, 1.65f, 0);
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.78f, 0.68f, 0.5f);
        camGO.AddComponent<AudioListener>();
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.cameraTransform = camGO.transform;
        player.AddComponent<Mission2Manager>();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Mission 2 scene built");
    }
    static void PlaceRealDino(string name, string path, Vector3 pos, float targetH, Material dinoM, Material bellyM)
    {
        GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (src == null) { BuildDino(name, pos, 1f, dinoM, bellyM); return; }
        GameObject go = (GameObject)Object.Instantiate(src, pos, Quaternion.identity);
        go.name = name;
        Bounds b = new Bounds(pos, Vector3.one * 0.1f);
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
        float h = Mathf.Max(0.01f, b.size.y);
        float k2 = targetH / h;
        go.transform.localScale = Vector3.one * k2;
        go.transform.position = new Vector3(pos.x, pos.y - b.min.y * k2, pos.z);
    }

    static void BuildDino(string name, Vector3 pos, float k, Material bodyM, Material bellyM)
    {
        GameObject d = new GameObject(name);
        d.transform.position = pos;
        d.transform.rotation = Quaternion.Euler(0, 160f, 0);
        Prim(PrimitiveType.Capsule, "Body", pos + new Vector3(0, 2.1f * k, 0), new Vector3(2.2f * k, 1.9f * k, 3.4f * k), bodyM, d.transform).transform.localRotation = Quaternion.Euler(90, 0, 0);
        Prim(PrimitiveType.Sphere, "Belly", pos + new Vector3(0, 1.7f * k, 0.2f * k), new Vector3(1.9f * k, 1.4f * k, 2.6f * k), bellyM, d.transform);
        for (int i = 0; i < 3; i++)
        {
            float ny = (3.1f + i * 0.85f) * k;
            float nz = (1.5f + i * 0.5f) * k;
            Prim(PrimitiveType.Cylinder, "Neck" + i, pos + new Vector3(0, ny, nz), new Vector3(0.85f * k, 0.55f * k, 0.85f * k), bodyM, d.transform).transform.localRotation = Quaternion.Euler(-38, 0, 0);
        }
        Prim(PrimitiveType.Sphere, "Snout", pos + new Vector3(0, 5.7f * k, 4f * k), new Vector3(0.7f * k, 0.5f * k, 0.7f * k), bellyM, d.transform);
        Material eyeM = GlowMat(Color.black, new Color(0.9f, 0.8f, 0.3f));
        Prim(PrimitiveType.Sphere, "EyeL", pos + new Vector3(0.42f * k, 6.05f * k, 3.7f * k), Vector3.one * 0.16f * k, eyeM, d.transform);
        Prim(PrimitiveType.Sphere, "EyeR", pos + new Vector3(-0.42f * k, 6.05f * k, 3.7f * k), Vector3.one * 0.16f * k, eyeM, d.transform);
        float[,] legs = new float[,] { { 0.9f, 1.2f }, { -0.9f, 1.2f }, { 0.9f, -1.2f }, { -0.9f, -1.2f } };
        for (int i = 0; i < 4; i++)
            Prim(PrimitiveType.Cylinder, "Leg" + i, pos + new Vector3(legs[i, 0] * k, 0.85f * k, legs[i, 1] * k), new Vector3(0.5f * k, 0.85f * k, 0.5f * k), bodyM, d.transform);
        Prim(PrimitiveType.Cylinder, "Tail1", pos + new Vector3(0, 1.9f * k, -2.4f * k), new Vector3(0.7f * k, 1.3f * k, 0.7f * k), bodyM, d.transform).transform.localRotation = Quaternion.Euler(72, 0, 0);
        Prim(PrimitiveType.Cylinder, "Tail2", pos + new Vector3(0, 1.5f * k, -4f * k), new Vector3(0.4f * k, 1f * k, 0.4f * k), bodyM, d.transform).transform.localRotation = Quaternion.Euler(80, 0, 0);
        for (int sp = 0; sp < 5; sp++)
            Prim(PrimitiveType.Cube, "Plate" + sp, pos + new Vector3(0, (2.9f + sp * 0.62f) * k, (0.4f + sp * 0.55f) * k), new Vector3(0.1f * k, 0.45f * k, 0.5f * k), bellyM, d.transform).transform.localRotation = Quaternion.Euler(-20, 0, 45);
        int fst = name == "parinosauras" ? 1 : 0;
        Texture2D ft = FaceBuilder.Face(fst, bodyM.color, new Color(0.1f, 0.08f, 0.06f), 0, new Color(0.15f, 0.4f, 0.15f), bodyM.color, false);
        FaceBuilder.Head("Head", ft, pos + new Vector3(0, 5.9f * k, 3.2f * k), new Vector3(1.1f * k, 0.85f * k, 1.4f * k), d.transform, 0f);
    }
    static void EggInside(Vector3 p)
    {
        Material sil = Mat(new Color(0.22f, 0.18f, 0.12f));
        Prim(PrimitiveType.Sphere, "EggBabyBody", p + new Vector3(0, -0.05f, 0), Vector3.one * 0.3f, sil);
        Prim(PrimitiveType.Sphere, "EggBabyHead", p + new Vector3(0.12f, 0.12f, 0.1f), Vector3.one * 0.18f, sil);
    }
    [MenuItem("WrongHole/Dino Diag")]
    static void DinoDiag()
    {
        var p = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/dinos/Apatosaurus/Apatosaurus.fbx");
        if (p == null) { Debug.Log("DINODIAG prefab missing"); return; }
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath("Assets/Models/dinos/Apatosaurus/Apatosaurus.fbx")) Debug.Log("DINODIAG sub " + a.GetType().Name + " " + a.name);
        foreach (var rd in p.GetComponentsInChildren<Renderer>(true))
            foreach (var m in rd.sharedMaterials)
                Debug.Log("DINODIAG mat " + m.name + " shader " + m.shader.name + " tex " + (m.mainTexture ? m.mainTexture.name : "NULL") + " col " + m.color);
    }

    [MenuItem("WrongHole/Dino Preview")]
    static void DinoPreview()
    {
        UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/dinos/Apatosaurus/Apatosaurus.fbx");
        if (go == null) { Debug.Log("PREV missing"); return; }
        var inst = (GameObject)Object.Instantiate(go);
        inst.name = "Prev";
        var lb = new GameObject("Sun");
        var li = lb.AddComponent<Light>(); li.type = LightType.Directional; li.intensity = 1.4f;
        lb.transform.rotation = Quaternion.Euler(50, -30, 0);
        var pl = GameObject.CreatePrimitive(PrimitiveType.Plane);
        pl.transform.localScale = Vector3.one * 3;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(6, 1, 0);
        cube.transform.localScale = Vector3.one * 2;
        var rs = inst.GetComponentsInChildren<Renderer>();
        if (rs.Length > 0)
        {
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            inst.transform.position = -new Vector3(b.center.x, b.min.y, b.center.z);
            Debug.Log("PREV bounds " + b.size + " skinned " + (inst.GetComponentInChildren<SkinnedMeshRenderer>() != null));
        }
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv != null) { sv.LookAt(new Vector3(0, 2, 0), Quaternion.Euler(15, -30, 0), 9f); sv.Repaint(); }
    }

}
