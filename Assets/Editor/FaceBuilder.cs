using UnityEngine;

public static class FaceBuilder
{
    public static Texture2D Face(int style, Color skin, Color hair, int hairStyle, Color iris)
    {
        return Face(style, skin, hair, hairStyle, iris, skin);
    }
    public static Texture2D Face(int style, Color skin, Color hair, int hairStyle, Color iris, Color bg, bool eyes = true)
    {
        int W = 128, H = 128;
        Texture2D t = new Texture2D(W, H, TextureFormat.RGBA32, false);
        for (int i = 0; i < W * H; i++) t.SetPixel(i % W, i / W, bg);
        System.Func<int,int,int,int,Color,bool> disc = (cx, cy, rx, ry, c) =>
        {
            bool any = false;
            for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (x - cx) / (float)rx, dy = (y - cy) / (float)ry;
                    if (dx * dx + dy * dy <= 1f && x >= 0 && x < W && y >= 0 && y < H) { t.SetPixel(x, y, c); any = true; }
                }
            return any;
        };
        System.Action<int,int,int,int,Color> rect = (x0, y0, w, h, c) =>
        {
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                    if (x >= 0 && x < W && y >= 0 && y < H) t.SetPixel(x, y, c);
            };
        Color white = Color.white, black = new Color(0.05f, 0.04f, 0.04f);
        disc(64, 58, 46, 52, skin);
        if (style == 2)
        {
            rect(20, 30, 88, 10, new Color(0.72f, 0.66f, 0.52f));
            rect(20, 74, 88, 8, new Color(0.72f, 0.66f, 0.52f));
        }
        if (eyes)
        {
            int ey = 64, ex = 19;
            foreach (int s in new int[] { -1, 1 })
            {
                int cx = 64 + s * ex;
                disc(cx, ey, 11, 9, white);
                disc(cx, ey, 5, 5, iris);
                disc(cx, ey, 2, 2, black);
                rect(cx - 9, ey + 10, 18, 3, black);
            }
        }
        if (style == 1)
        {
            rect(45, 30, 38, 4, new Color(0.35f, 0.1f, 0.1f));
            disc(64, 27, 10, 6, new Color(0.6f, 0.2f, 0.2f));
            rect(58, 44, 12, 6, new Color(skin.r * 0.85f, skin.g * 0.8f, skin.b * 0.8f));
        }
        else if (style == 2)
        {
            disc(64, 26, 14, 5, new Color(0.4f, 0.3f, 0.22f));
        }
        else
        {
            disc(64, 28, 12, 7, new Color(0.45f, 0.12f, 0.12f));
            disc(64, 31, 6, 3, black);
            rect(59, 46, 10, 8, new Color(skin.r * 0.85f, skin.g * 0.8f, skin.b * 0.8f));
        }
        if (hairStyle > 0)
        {
            disc(64, 98, 44, 22, hair);
            if (hairStyle == 2) { disc(30, 84, 12, 16, hair); disc(98, 84, 12, 16, hair); }
        }
        t.Apply();
        return t;
    }

    public static Material HeadMat(Texture2D tex)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.mainTexture = tex;
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
        return m;
    }

    public static GameObject Head(string name, Texture2D tex, Vector3 pos, Vector3 scale, Transform parent, float yaw)
    {
        GameObject h = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        h.name = name;
        Object.DestroyImmediate(h.GetComponent<Collider>());
        h.transform.SetParent(parent, true);
        h.transform.position = pos;
        h.transform.localScale = scale;
        h.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        h.GetComponent<Renderer>().sharedMaterial = HeadMat(tex);
        return h;
    }

    public static GameObject Quad(string name, Texture2D tex, Vector3 pos, Vector2 size, Transform parent)
    {
        GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = name;
        Object.DestroyImmediate(q.GetComponent<Collider>());
        q.transform.SetParent(parent, true);
        q.transform.position = pos;
        q.transform.localScale = new Vector3(size.x, size.y, 1f);
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Unlit/Texture");
        Material m = new Material(sh);
        m.mainTexture = tex;
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
        m.enableInstancing = false;
        q.GetComponent<Renderer>().sharedMaterial = m;
        return q;
    }
}
