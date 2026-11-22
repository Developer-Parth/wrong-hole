using UnityEngine;
using UnityEngine.SceneManagement;

public static class Mission6Builder
{
    static Material Mat(Color c, float smooth)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        return m;
    }

    static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Material m, Transform parent)
    {
        GameObject g = GameObject.CreatePrimitive(t);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (parent != null) g.transform.SetParent(parent, true);
        return g;
    }

    static Material TexMat(Texture2D t, float sx, float sy, float smooth)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.mainTexture = t;
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", t);
        m.mainTextureScale = new Vector2(sx, sy);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        return m;
    }

    static int H(int x, int y) { return (x * 73856093) ^ (y * 19349663) & 0x7fffffff; }

    static Texture2D SandTex()
    {
        int W = 128;
        Texture2D t = new Texture2D(W, W, TextureFormat.RGBA32, false);
        for (int y = 0; y < W; y++)
            for (int x = 0; x < W; x++)
            {
                float n = (H(x, y) % 100) / 100f;
                float n2 = (H(x / 8, y / 8) % 100) / 100f;
                Color c = new Color(0.58f + n * 0.1f + n2 * 0.06f, 0.46f + n * 0.08f + n2 * 0.05f, 0.28f + n * 0.05f);
                t.SetPixel(x, y, c);
            }
        t.Apply();
        t.wrapMode = TextureWrapMode.Repeat;
        return t;
    }

    static Texture2D GlyphsTex()
    {
        int W = 256;
        Texture2D t = new Texture2D(W, W, TextureFormat.RGBA32, false);
        for (int y = 0; y < W; y++)
            for (int x = 0; x < W; x++)
            {
                float n = (H(x, y) % 100) / 100f;
                t.SetPixel(x, y, new Color(0.58f + n * 0.08f, 0.47f + n * 0.07f, 0.3f + n * 0.05f));
            }
        Color ink = new Color(0.32f, 0.22f, 0.12f);
        Color gold = new Color(0.75f, 0.58f, 0.22f);
        System.Action<int, int, int, int, Color> fill = (x0, y0, w, h, c) =>
        {
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                    if (x >= 0 && x < W && y >= 0 && y < W) t.SetPixel(x % W, y % W, c);
        };
        System.Action<int, int, int, Color> dot = (cx, cy, r, c) =>
        {
            for (int y = cy - r; y <= cy + r; y++)
                for (int x = cx - r; x <= cx + r; x++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r && x >= 0 && x < W && y >= 0 && y < W) t.SetPixel(x, y, c);
        };
        for (int band = 0; band < 4; band++)
        {
            int yBase = 24 + band * 60;
            fill(0, yBase - 14, W, 2, gold);
            fill(0, yBase + 40, W, 2, gold);
            for (int cell = 0; cell < 12; cell++)
            {
                int xBase = 10 + cell * 21;
                int kind = H(cell, band) % 5;
                if (kind == 0) { fill(xBase, yBase, 4, 26, ink); fill(xBase + 8, yBase + 10, 8, 4, ink); }
                else if (kind == 1) { dot(xBase + 6, yBase + 13, 8, ink); dot(xBase + 6, yBase + 13, 4, new Color(0.58f, 0.47f, 0.3f)); }
                else if (kind == 2) { fill(xBase, yBase + 11, 14, 4, ink); fill(xBase + 4, yBase + 4, 4, 18, ink); }
                else if (kind == 3) { fill(xBase, yBase, 4, 26, ink); fill(xBase + 8, yBase, 4, 26, ink); fill(xBase, yBase + 11, 12, 4, ink); }
                else { dot(xBase + 3, yBase + 8, 4, ink); dot(xBase + 9, yBase + 18, 4, ink); fill(xBase + 2, yBase + 24, 10, 3, ink); }
            }
        }
        t.Apply();
        t.wrapMode = TextureWrapMode.Repeat;
        return t;
    }

    static void Torch(float x, float z)
    {
        GameObject t = new GameObject("Torch");
        t.transform.position = new Vector3(x, 3.4f, z);
        Light l = t.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.6f, 0.25f);
        l.intensity = 2.2f;
        l.range = 14f;
        l.shadows = LightShadows.Soft;
    }

    static void Mummy(int n, Vector3 pos, Transform parent)
    {
        Material wrap = Mat(n == 3 ? new Color(0.85f, 0.85f, 0.8f) : new Color(0.78f, 0.72f, 0.58f), 0.1f);
        Material band = Mat(new Color(0.55f, 0.48f, 0.35f), 0.1f);
        Material hood = Mat(new Color(0.2f, 0.45f, 0.9f), 0.2f);
        Material hair = Mat(new Color(0.08f, 0.06f, 0.05f), 0.2f);
        GameObject root = new GameObject("Mummy");
        root.transform.SetParent(parent, true);
        root.transform.position = pos;
        float z = -0.28f;
        Prim(PrimitiveType.Capsule, "Torso", pos + new Vector3(0, 1.0f, z), new Vector3(0.62f, 0.62f, 0.34f), n == 3 ? hood : wrap, root.transform);
        if (n == 3) Prim(PrimitiveType.Sphere, "Hair", pos + new Vector3(0, 2.0f, z + 0.04f), new Vector3(0.4f, 0.22f, 0.38f), hair, root.transform);
        Color sk = new Color(0.8f, 0.64f, 0.47f);
        Texture2D ftx = n == 3
            ? FaceBuilder.Face(0, sk, new Color(0.08f, 0.06f, 0.05f), 2, new Color(0.3f, 0.18f, 0.1f), sk)
            : FaceBuilder.Face(2, new Color(0.78f, 0.72f, 0.58f), new Color(0.08f, 0.06f, 0.05f), 0, new Color(0.2f, 0.2f, 0.22f), new Color(0.78f, 0.72f, 0.58f));
        FaceBuilder.Head("Head", ftx, pos + new Vector3(0, 1.82f, z), new Vector3(0.38f, 0.44f, 0.36f), root.transform, 180f);
        for (int i = 0; i < 6; i++)
            Prim(PrimitiveType.Cylinder, "Wrap" + i, pos + new Vector3(0, 0.35f + i * 0.28f, z), new Vector3(0.66f, 0.015f, 0.38f), band, root.transform).transform.rotation = Quaternion.identity;
        Prim(PrimitiveType.Capsule, "ArmL", pos + new Vector3(-0.38f, 1.0f, z - 0.06f), new Vector3(0.14f, 0.38f, 0.14f), wrap, root.transform);
        Prim(PrimitiveType.Capsule, "ArmR", pos + new Vector3(0.38f, 1.0f, z - 0.06f), new Vector3(0.14f, 0.38f, 0.14f), wrap, root.transform);
        Prim(PrimitiveType.Capsule, "Legs", pos + new Vector3(0, 0.45f, z), new Vector3(0.4f, 0.4f, 0.28f), wrap, root.transform);
    }

    static void Coffin(int n, Vector3 pos, Material gold, Material dark, Material face)
    {
        GameObject c = new GameObject("Coffin" + n);
        c.transform.position = pos;
        Prim(PrimitiveType.Cube, "Body", pos + new Vector3(0, 1.15f, 0), new Vector3(1.0f, 2.3f, 0.7f), gold, c.transform);
        GameObject lidObj = Prim(PrimitiveType.Cube, "Lid", pos + new Vector3(0, 1.15f, -0.42f), new Vector3(0.9f, 2.2f, 0.16f), gold, c.transform);
        Prim(PrimitiveType.Sphere, "Face", pos + new Vector3(0, 1.78f, -0.5f), new Vector3(0.52f, 0.62f, 0.2f), face, lidObj.transform);
        Prim(PrimitiveType.Cube, "Band", pos + new Vector3(0, 0.9f, -0.44f), new Vector3(0.94f, 0.22f, 0.1f), dark, lidObj.transform);
        Prim(PrimitiveType.Cube, "Base", pos + new Vector3(0, 0.08f, 0), new Vector3(1.4f, 0.16f, 1.1f), dark, c.transform);
        Mummy(n, pos, c.transform);
    }

    public static void BuildInto()
    {
        RenderSettings.ambientLight = new Color(0.05f, 0.045f, 0.06f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.02f, 0.015f, 0.03f);
        RenderSettings.fogStartDistance = 10f;
        RenderSettings.fogEndDistance = 55f;
        Camera.main?.gameObject.SetActive(false);
        GameObject moon = new GameObject("DimLight");
        Light dl = moon.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.color = new Color(0.3f, 0.28f, 0.24f);
        dl.intensity = 0.4f;
        moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Material sand = TexMat(SandTex(), 15f, 45f, 0.12f);
        Material walls = TexMat(GlyphsTex(), 22f, 2f, 0.15f);
        Material wallsEnd = TexMat(GlyphsTex(), 8f, 2f, 0.15f);
        Material dark = Mat(new Color(0.2f, 0.14f, 0.08f), 0.1f);
        Material gold = Mat(new Color(0.8f, 0.62f, 0.25f), 0.55f);
        Material face = Mat(new Color(0.9f, 0.78f, 0.45f), 0.6f);
        Prim(PrimitiveType.Cube, "Floor", new Vector3(0, -0.5f, 40), new Vector3(30, 1, 90), sand, null);
        Prim(PrimitiveType.Cube, "WallL", new Vector3(-12, 3.5f, 40), new Vector3(1, 8, 90), walls, null);
        Prim(PrimitiveType.Cube, "WallR", new Vector3(12, 3.5f, 40), new Vector3(1, 8, 90), walls, null);
        Prim(PrimitiveType.Cube, "WallB", new Vector3(0, 3.5f, -5), new Vector3(30, 8, 1), wallsEnd, null);
        Prim(PrimitiveType.Cube, "WallF", new Vector3(0, 3.5f, 85), new Vector3(30, 8, 1), wallsEnd, null);
        Prim(PrimitiveType.Cube, "Ceiling", new Vector3(0, 7.5f, 40), new Vector3(30, 1, 90), dark, null);
        Prim(PrimitiveType.Cube, "TrimL", new Vector3(-11.4f, 6.9f, 40), new Vector3(0.35f, 0.5f, 90), gold, null);
        Prim(PrimitiveType.Cube, "TrimR", new Vector3(11.4f, 6.9f, 40), new Vector3(0.35f, 0.5f, 90), gold, null);
        Prim(PrimitiveType.Cube, "TrimB", new Vector3(0, 6.9f, -4.4f), new Vector3(30, 0.5f, 0.35f), gold, null);
        Prim(PrimitiveType.Cube, "TrimF", new Vector3(0, 6.9f, 84.4f), new Vector3(30, 0.5f, 0.35f), gold, null);
        Prim(PrimitiveType.Cube, "SkirtL", new Vector3(-11.45f, 0.35f, 40), new Vector3(0.25f, 0.7f, 90), dark, null);
        Prim(PrimitiveType.Cube, "SkirtR", new Vector3(11.45f, 0.35f, 40), new Vector3(0.25f, 0.7f, 90), dark, null);
        Material pillar = Mat(new Color(0.52f, 0.4f, 0.24f), 0.2f);
        float[] px = { -8f, 8f, -8f, 8f };
        float[] pz = { 10f, 10f, 70f, 70f };
        for (int i = 0; i < 4; i++)
        {
            Prim(PrimitiveType.Cylinder, "Pillar", new Vector3(px[i], 3.5f, pz[i]), new Vector3(1.4f, 3.5f, 1.4f), pillar, null);
            Prim(PrimitiveType.Cylinder, "PillarCap", new Vector3(px[i], 7.1f, pz[i]), new Vector3(1.9f, 0.28f, 1.9f), gold, null);
            Prim(PrimitiveType.Cylinder, "PillarBase", new Vector3(px[i], 0.2f, pz[i]), new Vector3(1.9f, 0.35f, 1.9f), gold, null);
        }
        Torch(-10, 12); Torch(10, 12); Torch(-10, 36); Torch(10, 36); Torch(-10, 60); Torch(10, 60);
        Coffin(1, new Vector3(3f, 0f, 18f), gold, dark, face);
        Coffin(2, new Vector3(-3f, 0f, 36f), gold, dark, face);
        Coffin(3, new Vector3(3f, 0f, 54f), gold, dark, face);
        Material sandFlat = Mat(new Color(0.6f, 0.5f, 0.33f), 0.1f);
        Material deadTrunk = Mat(new Color(0.42f, 0.33f, 0.22f), 0f);
        Material deadLeaf = Mat(new Color(0.58f, 0.5f, 0.33f), 0f);
        Material desertRock = Mat(new Color(0.5f, 0.44f, 0.32f), 0f);
        Prim(PrimitiveType.Cube, "DesertFloor", new Vector3(0f, -0.5f, -50f), new Vector3(70f, 1f, 90f), sand, null);
        Prim(PrimitiveType.Cube, "PyramidStep1", new Vector3(0f, 3f, -9.5f), new Vector3(46f, 7f, 9f), sandFlat, null);
        Prim(PrimitiveType.Cube, "PyramidStep2", new Vector3(0f, 9f, -9.5f), new Vector3(36f, 6f, 8f), sandFlat, null);
        Prim(PrimitiveType.Cube, "PyramidStep3", new Vector3(0f, 14f, -9.5f), new Vector3(26f, 5f, 7f), sandFlat, null);
        Material gold2 = Mat(new Color(0.8f, 0.62f, 0.25f), 0.55f);
        Prim(PrimitiveType.Cube, "PyramidDoor", new Vector3(0f, 2.2f, -5.2f), new Vector3(5f, 4.4f, 0.6f), gold2, null);
        System.Random drng = new System.Random(42);
        for (int i = 0; i < 8; i++)
        {
            float tx = (float)(drng.NextDouble() * 44.0 - 22.0);
            float tz = (float)(drng.NextDouble() * 32.0 - 48.0);
            float tilt = (float)(drng.NextDouble() * 14.0 - 7.0);
            GameObject dt = Prim(PrimitiveType.Cylinder, "DeadTreeTrunk", new Vector3(tx, 2.4f, tz), new Vector3(0.5f, 2.4f, 0.5f), deadTrunk, null);
            dt.transform.rotation = Quaternion.Euler(tilt, 0f, tilt * 0.6f);
            Prim(PrimitiveType.Sphere, "DeadTreeTop", new Vector3(tx, 6f, tz), new Vector3(3.4f, 2.8f, 3.4f), deadLeaf, null);
            Prim(PrimitiveType.Sphere, "DeadTreeTop2", new Vector3(tx, 8f, tz), Vector3.one * 2.2f, deadLeaf, null);
        }
        for (int i = 0; i < 9; i++)
        {
            float bx = (float)(drng.NextDouble() * 44.0 - 22.0);
            float bz = (float)(drng.NextDouble() * 34.0 - 50.0);
            float bs = 0.7f + (float)drng.NextDouble() * 0.8f;
            Prim(PrimitiveType.Sphere, "DeadBush", new Vector3(bx, bs * 0.25f, bz), new Vector3(bs * 1.6f, bs * 0.55f, bs * 1.6f), deadLeaf, null);
        }
        for (int i = 0; i < 7; i++)
        {
            float rx = (float)(drng.NextDouble() * 40.0 - 20.0);
            float rz = (float)(drng.NextDouble() * 30.0 - 48.0);
            float rs = 0.6f + (float)drng.NextDouble() * 1.2f;
            Prim(PrimitiveType.Sphere, "DesertRock", new Vector3(rx, rs * 0.2f, rz), new Vector3(rs * 1.5f, rs * 0.7f, rs * 1.2f), desertRock, null);
        }
        Material moonM = Mat(new Color(1f, 1f, 1f), 0f);
        moonM.EnableKeyword("_EMISSION");
        moonM.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * 2.2f);
        GameObject dmoon = Prim(PrimitiveType.Sphere, "DesertMoon", new Vector3(20f, 42f, 70f), Vector3.one * 9f, moonM, null);
        Object.DestroyImmediate(dmoon.GetComponent<Collider>());
        Material shaftM = Mat(new Color(0.05f, 0.045f, 0.06f), 0f);
        Prim(PrimitiveType.Cube, "ShaftW1", new Vector3(3.5f, -51f, 40f), new Vector3(1f, 102f, 8f), shaftM, null);
        Prim(PrimitiveType.Cube, "ShaftW2", new Vector3(-3.5f, -51f, 40f), new Vector3(1f, 102f, 8f), shaftM, null);
        Prim(PrimitiveType.Cube, "ShaftW3", new Vector3(0f, -51f, 43.5f), new Vector3(8f, 102f, 1f), shaftM, null);
        Prim(PrimitiveType.Cube, "ShaftW4", new Vector3(0f, -51f, 36.5f), new Vector3(8f, 102f, 1f), shaftM, null);
        Prim(PrimitiveType.Cube, "ShaftBottom", new Vector3(0f, -102.5f, 40f), new Vector3(8f, 1f, 8f), shaftM, null);
        Material shardM = Mat(new Color(0.28f, 0.24f, 0.2f), 0f);
        Material glowShardM = Mat(new Color(0.95f, 0.75f, 0.2f), 0.6f);
        glowShardM.EnableKeyword("_EMISSION");
        glowShardM.SetColor("_EmissionColor", new Color(1.2f, 0.85f, 0.25f) * 1.6f);
        System.Random srng = new System.Random(11);
        for (int i = 0; i < 26; i++)
        {
            float sx = (float)(srng.NextDouble() * 4.4 - 2.2);
            float sz2 = 40f + (float)(srng.NextDouble() * 4.4 - 2.2);
            float sy = -8f - (float)srng.NextDouble() * 86f;
            float ss = 0.2f + (float)srng.NextDouble() * 0.45f;
            GameObject sh = Prim(PrimitiveType.Cube, "FallShard", new Vector3(sx, sy, sz2), Vector3.one * ss, i % 5 == 0 ? glowShardM : shardM, null);
            sh.transform.rotation = Quaternion.Euler((float)srng.NextDouble() * 90f, (float)srng.NextDouble() * 90f, 0f);
        }
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, 1.2f, 0);
        player.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1));
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.9f, 0);
        GameObject camGO = new GameObject("PlayerCamera");
        camGO.transform.SetParent(player.transform, false);
        camGO.transform.localPosition = new Vector3(0, 1.65f, 0);
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.015f, 0.03f);
        camGO.AddComponent<AudioListener>();
        Material hoodM = Mat(new Color(0.2f, 0.45f, 0.9f), 0.3f);
        GameObject pbody = Prim(PrimitiveType.Capsule, "PlayerBody", new Vector3(0f, 2.1f, 0f), new Vector3(0.55f, 0.7f, 0.4f), hoodM, player.transform);
        Object.DestroyImmediate(pbody.GetComponent<Collider>());
        Color psk = new Color(0.8f, 0.64f, 0.47f);
        Texture2D pft = FaceBuilder.Face(0, psk, new Color(0.08f, 0.06f, 0.05f), 2, new Color(0.3f, 0.18f, 0.1f), psk);
        FaceBuilder.Head("PlayerHead", pft, new Vector3(0f, 2.6f, 0f), new Vector3(0.42f, 0.46f, 0.42f), player.transform, 0f);
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.cameraTransform = camGO.transform;
        player.AddComponent<Mission6Manager>();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Mission 6 scene built");
    }
}
