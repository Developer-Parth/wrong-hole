using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class MissionBuilder
{
    [MenuItem("WrongHole/Build Mission 1")]
    public static void BuildMission()
    {
        string[] names = { "Ground", "Pit", "Wormhole", "Player", "Cheetah1", "Cheetah2", "Cheetah3", "Cheetah4", "Tree", "Trees", "GameManager", "Moon", "Stars", "Bush", "SaplingTrunk", "SaplingLeaves", "GrassTuft", "Firefly", "Bus", "EpilogueStone" };
        foreach (string n in names)
        {
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject g in all)
            {
                if (g != null && g.name.StartsWith(n)) Object.DestroyImmediate(g);
            }
        }
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.19f, 0.23f, 0.33f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 25f;
        RenderSettings.fogEndDistance = 95f;
        RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.12f);
        Light moonLight = null;
        GameObject dl = GameObject.Find("Directional Light");
        if (dl != null)
        {
            moonLight = dl.GetComponent<Light>();
            moonLight.color = new Color(0.60f, 0.70f, 1.00f);
            moonLight.intensity = 2.0f;
            dl.transform.rotation = Quaternion.Euler(55f, -140f, 0f);
        }
        Material groundMat = MakeMat(new Color(0.14f, 0.22f, 0.14f), 0f);
        Material pathMat = MakeMat(new Color(0.30f, 0.24f, 0.15f), 0f);
        Material trunkMat = MakeMat(new Color(0.25f, 0.16f, 0.09f), 0f);
        Material leafMat = MakeMat(new Color(0.10f, 0.30f, 0.12f), 0f);
        Material leafMat2 = MakeMat(new Color(0.14f, 0.38f, 0.16f), 0f);
        Material leafMat3 = MakeMat(new Color(0.07f, 0.24f, 0.18f), 0f);
        Material bushMat = MakeMat(new Color(0.08f, 0.20f, 0.10f), 0f);
        Material grassMat = MakeMat(new Color(0.16f, 0.30f, 0.14f), 0f);
        Material fireflyMat = MakeMat(new Color(1f, 1f, 1f), 0f);
        fireflyMat.EnableKeyword("_EMISSION");
        fireflyMat.SetColor("_EmissionColor", new Color(0.7f, 1f, 0.35f) * 2.4f);
        Material cheetahMat = MakeMat(new Color(0.95f, 0.75f, 0.40f), 0f);
        Material darkMat = MakeMat(new Color(0.18f, 0.12f, 0.06f), 0f);
        Material eyeMat = MakeMat(new Color(1f, 1f, 1f), 0f);
        eyeMat.EnableKeyword("_EMISSION");
        eyeMat.SetColor("_EmissionColor", new Color(0.8f, 1f, 0.25f) * 3f);
        Material holeMat = MakeMat(new Color(1f, 1f, 1f), 0f);
        holeMat.EnableKeyword("_EMISSION");
        holeMat.SetColor("_EmissionColor", new Color(0.3f, 0.7f, 1f) * 3.5f);
        Material holeMat2 = MakeMat(new Color(1f, 1f, 1f), 0f);
        holeMat2.EnableKeyword("_EMISSION");
        holeMat2.SetColor("_EmissionColor", new Color(0.65f, 0.3f, 1f) * 3.5f);
        Material moonMat = MakeMat(new Color(1f, 1f, 1f), 0f);
        moonMat.EnableKeyword("_EMISSION");
        moonMat.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * 2.2f);
        BuildGround(groundMat);
        BuildPath(pathMat);
        BuildPit();
        BuildWormhole(holeMat, holeMat2);
        BuildMoonAndStars(moonMat);
        BuildPlayer();
        MakeCheetah("Cheetah1", new Vector3(18f, 1f, -18f), cheetahMat, darkMat, eyeMat);
        MakeCheetah("Cheetah2", new Vector3(-18f, 1f, -18f), cheetahMat, darkMat, eyeMat);
        MakeCheetah("Cheetah3", new Vector3(25f, 1f, -5f), cheetahMat, darkMat, eyeMat);
        MakeCheetah("Cheetah4", new Vector3(-25f, 1f, -5f), cheetahMat, darkMat, eyeMat);
        BuildTrees(trunkMat, leafMat, leafMat2);
        BuildBus();
        BuildEpilogueStone();
        BuildForestDetail(trunkMat, leafMat3, bushMat, grassMat, fireflyMat);
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Mission 1 scene built and saved");
    }


    static GameObject Part(PrimitiveType t, string name, Vector3 localPos, Vector3 scale, Material m, Transform parent, Vector3 localEuler)
    {
        GameObject g = GameObject.CreatePrimitive(t);
        g.name = name;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localScale = scale;
        g.transform.localRotation = Quaternion.Euler(localEuler);
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    static void BuildBus()
    {
        Material bodyM = MakeMat(new Color(0.85f, 0.6f, 0.1f), 0.25f);
        Material glassM = MakeMat(new Color(0.5f, 0.7f, 0.85f), 0.6f);
        Material darkM = MakeMat(new Color(0.1f, 0.1f, 0.1f), 0.1f);
        Material lightM = MakeMat(new Color(1f, 1f, 1f), 0f);
        lightM.EnableKeyword("_EMISSION");
        lightM.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.6f) * 2.5f);
        GameObject bus = new GameObject("Bus");
        bus.transform.position = new Vector3(-12.5f, 0f, -20f);
        bus.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
        Part(PrimitiveType.Cube, "BusBody", new Vector3(0f, 1.4f, 0f), new Vector3(7f, 1.8f, 2.3f), bodyM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "BusTop", new Vector3(0f, 2.55f, 0f), new Vector3(7f, 0.6f, 2.3f), bodyM, bus.transform, Vector3.zero);
        for (int i = 0; i < 5; i++)
        {
            float wx = -2.4f + i * 1.2f;
            Part(PrimitiveType.Cube, "BusWindowL", new Vector3(wx, 1.95f, -1.16f), new Vector3(0.95f, 0.7f, 0.05f), glassM, bus.transform, Vector3.zero);
            Part(PrimitiveType.Cube, "BusWindowR", new Vector3(wx, 1.95f, 1.16f), new Vector3(0.95f, 0.7f, 0.05f), glassM, bus.transform, Vector3.zero);
        }
        Part(PrimitiveType.Cube, "BusWindshield", new Vector3(3.52f, 1.9f, 0f), new Vector3(0.05f, 0.75f, 1.9f), glassM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "BusRearWindow", new Vector3(-3.52f, 1.9f, 0f), new Vector3(0.05f, 0.7f, 1.8f), glassM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "BusDoor", new Vector3(2.9f, 1.3f, 1.17f), new Vector3(0.9f, 1.5f, 0.05f), darkM, bus.transform, Vector3.zero);
        for (int wx = -1; wx <= 1; wx += 2)
            for (int wz = -1; wz <= 1; wz += 2)
                Part(PrimitiveType.Cylinder, "BusWheel", new Vector3(2.3f * wx, 0.5f, 1.15f * wz), new Vector3(0.55f, 0.12f, 0.55f), darkM, bus.transform, new Vector3(90f, 0f, 0f));
        Part(PrimitiveType.Sphere, "BusLightL", new Vector3(3.55f, 0.9f, -0.75f), Vector3.one * 0.18f, lightM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Sphere, "BusLightR", new Vector3(3.55f, 0.9f, 0.75f), Vector3.one * 0.18f, lightM, bus.transform, Vector3.zero);
    }

    static void BuildEpilogueStone()
    {
        Material stoneM = MakeMat(new Color(0.55f, 0.48f, 0.3f), 0.2f);
        Material darkM = MakeMat(new Color(0.2f, 0.14f, 0.08f), 0.1f);
        Material faceM = MakeMat(new Color(0.75f, 0.62f, 0.32f), 0.5f);
        GameObject st = new GameObject("EpilogueStone");
        st.transform.position = new Vector3(-9f, 0f, -18.4f);
        st.transform.rotation = Quaternion.Euler(0f, 15f, 0f);
        Part(PrimitiveType.Cube, "StoneBase", new Vector3(0f, 0.08f, 0f), new Vector3(1.4f, 0.16f, 1.1f), darkM, st.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "StoneBody", new Vector3(0f, 1.15f, 0f), new Vector3(1f, 2.3f, 0.7f), stoneM, st.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "StoneLid", new Vector3(0f, 1.15f, -0.42f), new Vector3(0.9f, 2.2f, 0.16f), stoneM, st.transform, Vector3.zero);
        Part(PrimitiveType.Sphere, "StoneFace", new Vector3(0f, 1.78f, -0.52f), new Vector3(0.52f, 0.62f, 0.2f), faceM, st.transform, Vector3.zero);
        st.SetActive(false);
    }

    static Material MakeMat(Color c, float smooth)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        return m;
    }

    static void BuildGround(Material mat)
    {
        Vector3[] pos = {
            new Vector3(0f, -0.5f, -18.5f),
            new Vector3(0f, -0.5f, 43.5f),
            new Vector3(-31f, -0.5f, 25f),
            new Vector3(31f, -0.5f, 25f)
        };
        Vector3[] size = {
            new Vector3(120f, 1f, 83f),
            new Vector3(120f, 1f, 33f),
            new Vector3(58f, 1f, 4f),
            new Vector3(58f, 1f, 4f)
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Ground";
            g.transform.position = pos[i];
            g.transform.localScale = size[i];
            g.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }

    static void BuildPath(Material mat)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = "GroundPath";
        p.transform.position = new Vector3(0f, 0.02f, 11.5f);
        p.transform.localScale = new Vector3(3f, 0.05f, 23f);
        p.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void BuildPit()
    {
        Material rock = MakeMat(new Color(0.09f, 0.08f, 0.07f), 0f);
        Vector3[] wallPos = {
            new Vector3(2.4f, -6.6f, 25f), new Vector3(-2.4f, -6.6f, 25f),
            new Vector3(0f, -6.6f, 27.4f), new Vector3(0f, -6.6f, 22.6f)
        };
        foreach (Vector3 p in wallPos)
        {
            GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = "Pit";
            w.transform.position = p;
            w.transform.localScale = (Mathf.Abs(p.x) > 0f) ? new Vector3(0.8f, 13f, 5.6f) : new Vector3(5.6f, 13f, 0.8f);
            w.GetComponent<Renderer>().sharedMaterial = rock;
        }
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Pit";
        floor.transform.position = new Vector3(0f, -12.5f, 25f);
        floor.transform.localScale = new Vector3(5.6f, 0.6f, 5.6f);
        floor.GetComponent<Renderer>().sharedMaterial = rock;
    }

    static void BuildWormhole(Material matA, Material matB)
    {
        GameObject wh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wh.name = "Wormhole";
        wh.transform.position = new Vector3(0f, -5.5f, 25f);
        wh.transform.localScale = new Vector3(4.4f, 0.5f, 4.4f);
        Object.DestroyImmediate(wh.GetComponent<Collider>());
        SphereCollider trig = wh.AddComponent<SphereCollider>();
        trig.isTrigger = true;
        trig.radius = 0.5f;
        Rigidbody rb = wh.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        Material diskMat = MakeMat(new Color(0.18f, 0.4f, 0.9f), 0.5f);
        diskMat.EnableKeyword("_EMISSION");
        diskMat.SetColor("_EmissionColor", new Color(0.3f, 0.65f, 1f) * 2.2f);
        wh.GetComponent<Renderer>().sharedMaterial = diskMat;
        wh.AddComponent<PitWormhole>();
        GameObject l1 = new GameObject("WormholeLightBlue");
        l1.transform.SetParent(wh.transform);
        l1.transform.localPosition = new Vector3(0f, 6f, 0f);
        Light p1 = l1.AddComponent<Light>();
        p1.type = LightType.Point;
        p1.color = new Color(0.3f, 0.7f, 1f);
        p1.intensity = 6f;
        p1.range = 30f;
        GameObject l2 = new GameObject("WormholeLightPurple");
        l2.transform.SetParent(wh.transform);
        l2.transform.localPosition = new Vector3(0f, 14f, 0f);
        Light p2 = l2.AddComponent<Light>();
        p2.type = LightType.Point;
        p2.color = new Color(0.65f, 0.3f, 1f);
        p2.intensity = 4.5f;
        p2.range = 34f;
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "WormholeBeam";
        Object.DestroyImmediate(beam.GetComponent<Collider>());
        beam.transform.SetParent(wh.transform);
        Material beamMat = MakeMat(new Color(1f, 1f, 1f), 0f);
        beamMat.EnableKeyword("_EMISSION");
        beamMat.SetColor("_EmissionColor", new Color(0.35f, 0.6f, 1f) * 2.4f);
        beam.transform.localPosition = new Vector3(0f, 10f, 0f);
        beam.transform.localScale = new Vector3(0.16f, 11f, 0.16f);
        beam.GetComponent<Renderer>().sharedMaterial = beamMat;
        for (int i = 0; i < 10; i++)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = "WormholeSpark";
            Object.DestroyImmediate(s.GetComponent<Collider>());
            s.transform.SetParent(wh.transform);
            float a = i * 36f * Mathf.Deg2Rad;
            float h = 2f + 4f * Mathf.Sin(i * 1.3f);
            s.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.5f, h, Mathf.Sin(a) * 0.5f);
            s.transform.localScale = Vector3.one * (0.5f / 4.4f) * 2.2f;
            s.GetComponent<Renderer>().sharedMaterial = beamMat;
        }
        Material rimA = MakeMat(new Color(0.12f, 0.10f, 0.08f), 0f);
        Material rimB = rimA;
        for (int i = 0; i < 8; i++)
        {
            GameObject r = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            r.name = "PitRim";
            Object.DestroyImmediate(r.GetComponent<Collider>());
            float a = i * 45f * Mathf.Deg2Rad;
            r.transform.position = new Vector3(Mathf.Cos(a) * 3.1f, 0.10f, 25f + Mathf.Sin(a) * 3.1f);
            r.transform.localScale = new Vector3(0.6f, 0.35f, 0.6f);
            r.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? rimA : rimB;
        }
    }

    static void BuildMoonAndStars(Material moonMat)
    {
        GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moon.name = "Moon";
        Object.DestroyImmediate(moon.GetComponent<Collider>());
        moon.transform.position = new Vector3(-55f, 48f, 85f);
        moon.transform.localScale = Vector3.one * 9f;
        moon.GetComponent<Renderer>().sharedMaterial = moonMat;
        System.Random rng = new System.Random(7);
        for (int i = 0; i < 40; i++)
        {
            GameObject st = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            st.name = "Stars";
            Object.DestroyImmediate(st.GetComponent<Collider>());
            float x = (float)(rng.NextDouble() * 240.0 - 120.0);
            float z = (float)(rng.NextDouble() * 240.0 - 80.0);
            float y = (float)(rng.NextDouble() * 30.0 + 42.0);
            st.transform.position = new Vector3(x, y, z);
            st.transform.localScale = Vector3.one * (float)(rng.NextDouble() * 0.7 + 0.5);
            st.GetComponent<Renderer>().sharedMaterial = moonMat;
        }
    }

    static void BuildPlayer()
    {
        Material hoodM = MakeMat(new Color(0.2f, 0.45f, 0.9f), 0.3f);
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.1f, 0f);
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.4f;
        player.GetComponent<Renderer>().sharedMaterial = hoodM;
        Color psk = new Color(0.8f, 0.64f, 0.47f);
        Texture2D pft = FaceBuilder.Face(0, psk, new Color(0.08f, 0.06f, 0.05f), 2, new Color(0.3f, 0.18f, 0.1f), psk);
        FaceBuilder.Head("PlayerHead", pft, new Vector3(0f, 1.62f, 0f), new Vector3(0.42f, 0.46f, 0.42f), player.transform, 0f);
        player.AddComponent<PlayerController>();
        GameObject camObj = new GameObject("PlayerCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.08f, 0.16f);
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        player.GetComponent<PlayerController>().cameraTransform = camObj.transform;
        GameObject lantern = new GameObject("PlayerLantern");
        lantern.transform.SetParent(player.transform);
        lantern.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        Light pl = lantern.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.color = new Color(1f, 0.82f, 0.55f);
        pl.intensity = 1.8f;
        pl.range = 12f;
    }

    static void MakeCheetah(string name, Vector3 pos, Material bodyMat, Material darkMat, Material eyeMat)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        CharacterController cc = root.AddComponent<CharacterController>();
        cc.height = 1.3f;
        cc.radius = 0.45f;
        cc.center = new Vector3(0f, 0.65f, 0f);
        root.AddComponent<CheetahAI>();
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = name + "Body";
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        body.transform.localScale = new Vector3(0.62f, 0.55f, 1.35f);
        body.GetComponent<Renderer>().sharedMaterial = bodyMat;
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = name + "Head";
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.transform.SetParent(root.transform);
        head.transform.localPosition = new Vector3(0f, 1.15f, 0.72f);
        head.transform.localScale = Vector3.one * 0.38f;
        head.GetComponent<Renderer>().sharedMaterial = bodyMat;
        GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Cube);
        snout.name = name + "Snout";
        Object.DestroyImmediate(snout.GetComponent<Collider>());
        snout.transform.SetParent(root.transform);
        snout.transform.localPosition = new Vector3(0f, 1.05f, 0.95f);
        snout.transform.localScale = new Vector3(0.2f, 0.14f, 0.22f);
        snout.GetComponent<Renderer>().sharedMaterial = darkMat;
        for (int e = -1; e <= 1; e += 2)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.name = name + "Ear";
            Object.DestroyImmediate(ear.GetComponent<Collider>());
            ear.transform.SetParent(root.transform);
            ear.transform.localPosition = new Vector3(0.13f * e, 1.36f, 0.66f);
            ear.transform.localScale = Vector3.one * 0.12f;
            ear.GetComponent<Renderer>().sharedMaterial = darkMat;
        }
        for (int e2 = -1; e2 <= 1; e2 += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = name + "Eye";
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            eye.transform.SetParent(root.transform);
            eye.transform.localPosition = new Vector3(0.09f * e2, 1.22f, 0.95f);
            eye.transform.localScale = Vector3.one * 0.08f;
            eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
        }
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tail.name = name + "Tail";
        Object.DestroyImmediate(tail.GetComponent<Collider>());
        tail.transform.SetParent(root.transform);
        tail.transform.localPosition = new Vector3(0f, 0.95f, -0.85f);
        tail.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
        tail.transform.localScale = new Vector3(0.09f, 0.5f, 0.09f);
        tail.GetComponent<Renderer>().sharedMaterial = bodyMat;
        for (int lx = -1; lx <= 1; lx += 2)
        {
            for (int lz = -1; lz <= 1; lz += 2)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = name + "Leg";
                Object.DestroyImmediate(leg.GetComponent<Collider>());
                leg.transform.SetParent(root.transform);
                leg.transform.localPosition = new Vector3(0.24f * lx, 0.35f, 0.38f * lz);
                leg.transform.localScale = new Vector3(0.11f, 0.35f, 0.11f);
                leg.GetComponent<Renderer>().sharedMaterial = bodyMat;
            }
        }
        for (int i = 0; i < 12; i++)
        {
            GameObject spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spot.name = name + "Spot";
            Object.DestroyImmediate(spot.GetComponent<Collider>());
            spot.transform.SetParent(root.transform);
            float sx = (i % 2 == 0) ? 0.28f : -0.28f;
            float sy = 0.72f + 0.12f * (i % 4);
            float sz = -0.5f + 0.11f * i;
            spot.transform.localPosition = new Vector3(sx, sy, sz);
            spot.transform.localScale = Vector3.one * 0.12f;
            spot.GetComponent<Renderer>().sharedMaterial = darkMat;
        }
    }

    static void BuildTrees(Material trunkMat, Material leafMat, Material leafMat2)
    {
        System.Random rng = new System.Random(42);
        int placed = 0;
        while (placed < 55)
        {
            float x = (float)(rng.NextDouble() * 110.0 - 55.0);
            float z = (float)(rng.NextDouble() * 95.0 - 40.0);
            if (Mathf.Abs(x) < 7f && z > 15f && z < 35f) continue;
            if (Mathf.Abs(x) < 5f && Mathf.Abs(z) < 8f) continue;
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "TreeTrunk";
            trunk.transform.position = new Vector3(x, 2.4f, z);
            trunk.transform.localScale = new Vector3(0.5f, 2.4f, 0.5f);
            trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;
            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "TreeLeaves";
            Object.DestroyImmediate(leaves.GetComponent<Collider>());
            leaves.transform.position = new Vector3(x, 6f, z);
            leaves.transform.localScale = new Vector3(3.4f, 2.8f, 3.4f);
            leaves.GetComponent<Renderer>().sharedMaterial = leafMat;
            GameObject leaves2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves2.name = "TreeLeaves";
            Object.DestroyImmediate(leaves2.GetComponent<Collider>());
            leaves2.transform.position = new Vector3(x, 8f, z);
            leaves2.transform.localScale = Vector3.one * 2.2f;
            leaves2.GetComponent<Renderer>().sharedMaterial = leafMat2;
            placed++;
        }
    }

    static void BuildForestDetail(Material trunkMat, Material leafMat3, Material bushMat, Material grassMat, Material fireflyMat)
    {
        System.Random rng = new System.Random(77);
        int bushes = 0;
        while (bushes < 34)
        {
            float x = (float)(rng.NextDouble() * 100.0 - 50.0);
            float z = (float)(rng.NextDouble() * 85.0 - 35.0);
            if (Mathf.Abs(x) < 6f && z > 15f && z < 35f) continue;
            if (Mathf.Abs(x) < 4f && Mathf.Abs(z) < 7f) continue;
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.name = "Bush";
            Object.DestroyImmediate(b.GetComponent<Collider>());
            float s = 0.7f + (float)rng.NextDouble() * 0.8f;
            b.transform.position = new Vector3(x, s * 0.4f, z);
            b.transform.localScale = new Vector3(s * 1.6f, s * 0.8f, s * 1.6f);
            b.GetComponent<Renderer>().sharedMaterial = bushMat;
            bushes++;
        }
        int saplings = 0;
        while (saplings < 18)
        {
            float x = (float)(rng.NextDouble() * 100.0 - 50.0);
            float z = (float)(rng.NextDouble() * 85.0 - 35.0);
            if (Mathf.Abs(x) < 6f && z > 15f && z < 35f) continue;
            if (Mathf.Abs(x) < 4f && Mathf.Abs(z) < 7f) continue;
            GameObject t = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            t.name = "SaplingTrunk";
            t.transform.position = new Vector3(x, 0.9f, z);
            t.transform.localScale = new Vector3(0.2f, 0.9f, 0.2f);
            t.GetComponent<Renderer>().sharedMaterial = trunkMat;
            GameObject l = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            l.name = "SaplingLeaves";
            Object.DestroyImmediate(l.GetComponent<Collider>());
            l.transform.position = new Vector3(x, 2.2f, z);
            l.transform.localScale = new Vector3(1.4f, 1.6f, 1.4f);
            l.GetComponent<Renderer>().sharedMaterial = leafMat3;
            saplings++;
        }
        int grass = 0;
        while (grass < 130)
        {
            float x = (float)(rng.NextDouble() * 100.0 - 50.0);
            float z = (float)(rng.NextDouble() * 85.0 - 35.0);
            if (Mathf.Abs(x) < 2.2f && z > 0f && z < 24f) continue;
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "GrassTuft";
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.position = new Vector3(x, 0.16f, z);
            g.transform.localScale = new Vector3(0.08f, 0.32f, 0.08f);
            g.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, (float)(rng.NextDouble() * 24.0 - 12.0));
            g.GetComponent<Renderer>().sharedMaterial = grassMat;
            grass++;
        }
        int flies = 0;
        while (flies < 44)
        {
            float x = (float)(rng.NextDouble() * 90.0 - 45.0);
            float z = (float)(rng.NextDouble() * 80.0 - 32.0);
            float y = 0.8f + (float)rng.NextDouble() * 1.6f;
            GameObject f = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            f.name = "Firefly";
            Object.DestroyImmediate(f.GetComponent<Collider>());
            f.transform.position = new Vector3(x, y, z);
            f.transform.localScale = Vector3.one * 0.13f;
            f.GetComponent<Renderer>().sharedMaterial = fireflyMat;
            flies++;
        }
    }
}
