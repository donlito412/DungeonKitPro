using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Dungeon Kit Pro - Procedural Dungeon Generator
/// Version 1.0 | Professional Dungeon Creation Tool
/// </summary>
public class DungeonKitPro : EditorWindow
{
    public enum DungeonStyle { Stone, Crypt, Mine, Temple, Sewer }
    public enum RoomShape { Square, Rectangular, LShape, Cross }
    
    private DungeonStyle style = DungeonStyle.Stone;
    private int roomCount = 8;
    private int roomMinSize = 6;
    private int roomMaxSize = 12;
    private float corridorWidth = 3f;
    private float wallHeight = 4f;
    private bool addTorches = true;
    private bool addPillars = true;
    private bool addDoors = true;
    private bool addTreasure = true;
    
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Dungeon Kit Pro")]
    public static void ShowWindow()
    {
        DungeonKitPro window = GetWindow<DungeonKitPro>("Dungeon Kit Pro");
        window.minSize = new Vector2(380, 550);
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 18;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🏰 DUNGEON KIT PRO", headerStyle);
        GUILayout.Label("Procedural Dungeon Generator", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(10);
        
        // Style Section
        GUILayout.Label("🎨 Dungeon Style", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        style = (DungeonStyle)EditorGUILayout.EnumPopup("Style", style);
        DrawStylePreview();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Layout Settings
        GUILayout.Label("📐 Layout Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        roomCount = EditorGUILayout.IntSlider("Room Count", roomCount, 3, 20);
        roomMinSize = EditorGUILayout.IntSlider("Min Room Size", roomMinSize, 4, 10);
        roomMaxSize = EditorGUILayout.IntSlider("Max Room Size", roomMaxSize, 8, 20);
        corridorWidth = EditorGUILayout.Slider("Corridor Width", corridorWidth, 2f, 6f);
        wallHeight = EditorGUILayout.Slider("Wall Height", wallHeight, 3f, 8f);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Features
        GUILayout.Label("✨ Features", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        addTorches = EditorGUILayout.Toggle("Add Torches", addTorches);
        addPillars = EditorGUILayout.Toggle("Add Pillars", addPillars);
        addDoors = EditorGUILayout.Toggle("Add Doorways", addDoors);
        addTreasure = EditorGUILayout.Toggle("Add Treasure", addTreasure);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // Generate Buttons
        GUI.backgroundColor = new Color(0.5f, 0.3f, 0.6f);
        if (GUILayout.Button("🏰 GENERATE DUNGEON", GUILayout.Height(45)))
        {
            GenerateDungeon();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Room", GUILayout.Height(30)))
        {
            GenerateSingleRoom(new Vector3(0, 0, 0), Random.Range(roomMinSize, roomMaxSize), Random.Range(roomMinSize, roomMaxSize));
        }
        if (GUILayout.Button("Generate Corridor", GUILayout.Height(30)))
        {
            GenerateCorridor(Vector3.zero, new Vector3(20, 0, 0));
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("🗑️ Clear Dungeon", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Dungeon", "Delete the entire dungeon?", "Yes", "Cancel"))
            {
                ClearDungeon();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(20);
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawStylePreview()
    {
        string desc = style switch
        {
            DungeonStyle.Stone => "🪨 Classic stone dungeon with cobblestone walls",
            DungeonStyle.Crypt => "💀 Dark crypt with bone decorations",
            DungeonStyle.Mine => "⛏️ Abandoned mine with wooden supports",
            DungeonStyle.Temple => "🏛️ Ancient temple with golden accents",
            DungeonStyle.Sewer => "🚰 Underground sewer with water channels",
            _ => ""
        };
        EditorGUILayout.HelpBox(desc, MessageType.None);
    }
    
    private (Color floor, Color wall, Color accent) GetStyleColors()
    {
        return style switch
        {
            DungeonStyle.Stone => (new Color(0.3f, 0.3f, 0.32f), new Color(0.4f, 0.4f, 0.42f), new Color(0.5f, 0.5f, 0.5f)),
            DungeonStyle.Crypt => (new Color(0.2f, 0.2f, 0.22f), new Color(0.25f, 0.25f, 0.28f), new Color(0.8f, 0.8f, 0.7f)),
            DungeonStyle.Mine => (new Color(0.35f, 0.25f, 0.15f), new Color(0.4f, 0.3f, 0.2f), new Color(0.6f, 0.45f, 0.25f)),
            DungeonStyle.Temple => (new Color(0.6f, 0.55f, 0.45f), new Color(0.5f, 0.45f, 0.35f), new Color(0.9f, 0.75f, 0.3f)),
            DungeonStyle.Sewer => (new Color(0.25f, 0.3f, 0.25f), new Color(0.3f, 0.35f, 0.3f), new Color(0.4f, 0.5f, 0.4f)),
            _ => (Color.gray, Color.gray, Color.white)
        };
    }
    
    private void GenerateDungeon()
    {
        EditorUtility.DisplayProgressBar("Dungeon Kit Pro", "Creating dungeon...", 0.1f);
        
        ClearDungeon();
        
        GameObject parent = new GameObject("Dungeon");
        parent.transform.position = Vector3.zero;
        
        List<Rect> rooms = new List<Rect>();
        List<Vector3> roomCenters = new List<Vector3>();
        
        // Generate rooms
        EditorUtility.DisplayProgressBar("Dungeon Kit Pro", "Generating rooms...", 0.3f);
        
        for (int i = 0; i < roomCount; i++)
        {
            int attempts = 0;
            bool placed = false;
            
            while (!placed && attempts < 50)
            {
                float w = Random.Range(roomMinSize, roomMaxSize);
                float h = Random.Range(roomMinSize, roomMaxSize);
                float x = Random.Range(-50f, 50f - w);
                float z = Random.Range(-50f, 50f - h);
                
                Rect newRoom = new Rect(x, z, w, h);
                bool overlaps = false;
                
                foreach (Rect room in rooms)
                {
                    if (room.Overlaps(new Rect(newRoom.x - 3, newRoom.y - 3, newRoom.width + 6, newRoom.height + 6)))
                    {
                        overlaps = true;
                        break;
                    }
                }
                
                if (!overlaps)
                {
                    rooms.Add(newRoom);
                    Vector3 center = new Vector3(newRoom.center.x, 0, newRoom.center.y);
                    roomCenters.Add(center);
                    GenerateSingleRoom(center, newRoom.width, newRoom.height, parent);
                    placed = true;
                }
                
                attempts++;
            }
        }
        
        // Generate corridors
        EditorUtility.DisplayProgressBar("Dungeon Kit Pro", "Creating corridors...", 0.6f);
        
        for (int i = 0; i < roomCenters.Count - 1; i++)
        {
            GenerateCorridor(roomCenters[i], roomCenters[i + 1], parent);
        }
        
        // Add features
        EditorUtility.DisplayProgressBar("Dungeon Kit Pro", "Adding features...", 0.8f);
        
        if (addTorches) AddTorchesToDungeon(parent);
        if (addTreasure) AddTreasureToDungeon(parent, roomCenters);
        
        EditorUtility.ClearProgressBar();
        
        Selection.activeGameObject = parent;
        Debug.Log("✅ Dungeon generated with " + rooms.Count + " rooms!");
    }
    
    private void GenerateSingleRoom(Vector3 center, float width, float depth, GameObject parent = null)
    {
        if (parent == null)
        {
            parent = GameObject.Find("Dungeon") ?? new GameObject("Dungeon");
        }
        
        var colors = GetStyleColors();
        
        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = colors.floor;
        Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wallMat.color = colors.wall;
        
        GameObject room = new GameObject("Room");
        room.transform.parent = parent.transform;
        room.transform.position = center;
        
        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = room.transform;
        floor.transform.localPosition = new Vector3(0, -0.25f, 0);
        floor.transform.localScale = new Vector3(width, 0.5f, depth);
        floor.GetComponent<Renderer>().material = floorMat;
        
        // Walls
        CreateWall(room.transform, new Vector3(0, wallHeight/2, depth/2), new Vector3(width, wallHeight, 0.5f), wallMat);
        CreateWall(room.transform, new Vector3(0, wallHeight/2, -depth/2), new Vector3(width, wallHeight, 0.5f), wallMat);
        CreateWall(room.transform, new Vector3(width/2, wallHeight/2, 0), new Vector3(0.5f, wallHeight, depth), wallMat);
        CreateWall(room.transform, new Vector3(-width/2, wallHeight/2, 0), new Vector3(0.5f, wallHeight, depth), wallMat);
        
        // Pillars
        if (addPillars && width > 8 && depth > 8)
        {
            Material pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pillarMat.color = colors.accent;
            
            float inset = 1.5f;
            CreatePillar(room.transform, new Vector3(width/2 - inset, 0, depth/2 - inset), pillarMat);
            CreatePillar(room.transform, new Vector3(-width/2 + inset, 0, depth/2 - inset), pillarMat);
            CreatePillar(room.transform, new Vector3(width/2 - inset, 0, -depth/2 + inset), pillarMat);
            CreatePillar(room.transform, new Vector3(-width/2 + inset, 0, -depth/2 + inset), pillarMat);
        }
    }
    
    private void CreateWall(Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.parent = parent;
        wall.transform.localPosition = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material = mat;
    }
    
    private void CreatePillar(Transform parent, Vector3 pos, Material mat)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Pillar";
        pillar.transform.parent = parent;
        pillar.transform.localPosition = pos + Vector3.up * wallHeight/2;
        pillar.transform.localScale = new Vector3(0.8f, wallHeight/2, 0.8f);
        pillar.GetComponent<Renderer>().material = mat;
    }
    
    private void GenerateCorridor(Vector3 start, Vector3 end, GameObject parent = null)
    {
        if (parent == null)
        {
            parent = GameObject.Find("Dungeon") ?? new GameObject("Dungeon");
        }
        
        var colors = GetStyleColors();
        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = colors.floor;
        
        GameObject corridor = new GameObject("Corridor");
        corridor.transform.parent = parent.transform;
        
        // L-shaped corridor
        Vector3 corner = new Vector3(end.x, 0, start.z);
        
        // Horizontal segment
        Vector3 hCenter = (start + corner) / 2;
        float hLength = Mathf.Abs(end.x - start.x);
        
        if (hLength > 0.5f)
        {
            GameObject hFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hFloor.name = "CorridorFloor";
            hFloor.transform.parent = corridor.transform;
            hFloor.transform.position = hCenter + Vector3.down * 0.25f;
            hFloor.transform.localScale = new Vector3(hLength + corridorWidth, 0.5f, corridorWidth);
            hFloor.GetComponent<Renderer>().material = floorMat;
        }
        
        // Vertical segment
        Vector3 vCenter = (corner + end) / 2;
        float vLength = Mathf.Abs(end.z - start.z);
        
        if (vLength > 0.5f)
        {
            GameObject vFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vFloor.name = "CorridorFloor";
            vFloor.transform.parent = corridor.transform;
            vFloor.transform.position = vCenter + Vector3.down * 0.25f;
            vFloor.transform.localScale = new Vector3(corridorWidth, 0.5f, vLength + corridorWidth);
            vFloor.GetComponent<Renderer>().material = floorMat;
        }
    }
    
    private void AddTorchesToDungeon(GameObject dungeon)
    {
        Material torchMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        torchMat.color = new Color(0.4f, 0.25f, 0.1f);
        
        foreach (Transform room in dungeon.transform)
        {
            if (!room.name.StartsWith("Room")) continue;
            
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 pos = room.position + new Vector3(Mathf.Cos(angle) * 3, 2.5f, Mathf.Sin(angle) * 3);
                
                GameObject torch = new GameObject("Torch");
                torch.transform.parent = room;
                torch.transform.position = pos;
                
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.transform.parent = torch.transform;
                handle.transform.localPosition = Vector3.zero;
                handle.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
                handle.GetComponent<Renderer>().material = torchMat;
                DestroyImmediate(handle.GetComponent<Collider>());
                
                Light light = torch.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.7f, 0.4f);
                light.intensity = 1.5f;
                light.range = 8f;
            }
        }
    }
    
    private void AddTreasureToDungeon(GameObject dungeon, List<Vector3> roomCenters)
    {
        Material chestMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        chestMat.color = new Color(0.5f, 0.35f, 0.15f);
        Material goldMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        goldMat.color = new Color(1f, 0.85f, 0.2f);
        
        // Add treasure to some rooms
        for (int i = 0; i < roomCenters.Count; i++)
        {
            if (Random.value > 0.4f) continue;
            
            GameObject treasure = new GameObject("Treasure");
            treasure.transform.parent = dungeon.transform;
            treasure.transform.position = roomCenters[i] + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.parent = treasure.transform;
            body.transform.localPosition = new Vector3(0, 0.3f, 0);
            body.transform.localScale = new Vector3(0.8f, 0.5f, 0.5f);
            body.GetComponent<Renderer>().material = chestMat;
            
            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.transform.parent = treasure.transform;
            lid.transform.localPosition = new Vector3(0, 0.6f, 0);
            lid.transform.localScale = new Vector3(0.85f, 0.15f, 0.55f);
            lid.GetComponent<Renderer>().material = goldMat;
        }
    }
    
    private void ClearDungeon()
    {
        GameObject dungeon = GameObject.Find("Dungeon");
        if (dungeon != null) DestroyImmediate(dungeon);
        Debug.Log("✅ Dungeon cleared!");
    }
}
