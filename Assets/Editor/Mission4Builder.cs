using UnityEngine;

public static class Mission4Builder
{
    public static void BuildInto()
    {
        DinoBuilder.Build();
        GameObject player = GameObject.Find("Player");
        Mission2Manager m2 = player.GetComponent<Mission2Manager>();
        if (m2 != null) Object.DestroyImmediate(m2);
        GameObject mum = GameObject.Find("Sukhdevosauras");
        if (mum != null)
        {
            mum.transform.position = new Vector3(16f, 0f, 18f);
            Vector3 face = new Vector3(20f, 0f, 42f) - mum.transform.position;
            mum.transform.rotation = Quaternion.LookRotation(face);
        }
        Vector3 look = new Vector3(20f, 0f, 42f) - new Vector3(player.transform.position.x, 0f, player.transform.position.z);
        player.transform.rotation = Quaternion.LookRotation(look);
        player.AddComponent<Mission4Manager>();
        Debug.Log("Mission 4 scene built");
    }
}
