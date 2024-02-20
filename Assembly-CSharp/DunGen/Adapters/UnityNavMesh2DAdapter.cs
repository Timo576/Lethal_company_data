// Decompiled with JetBrains decompiler
// Type: DunGen.Adapters.UnityNavMesh2DAdapter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

#nullable disable
namespace DunGen.Adapters
{
  [AddComponentMenu("DunGen/NavMesh/Unity NavMesh Adapter (2D)")]
  public class UnityNavMesh2DAdapter : NavMeshAdapter
  {
    private static Quaternion rotation = Quaternion.Euler(-90f, 0.0f, 0.0f);
    public bool AddNavMeshLinksBetweenRooms = true;
    public List<UnityNavMesh2DAdapter.NavMeshAgentLinkInfo> NavMeshAgentTypes = new List<UnityNavMesh2DAdapter.NavMeshAgentLinkInfo>()
    {
      new UnityNavMesh2DAdapter.NavMeshAgentLinkInfo()
    };
    public float NavMeshLinkDistanceFromDoorway = 1f;
    [SerializeField]
    private int agentTypeID;
    [SerializeField]
    private bool overrideTileSize;
    [SerializeField]
    private int tileSize = 256;
    [SerializeField]
    private bool overrideVoxelSize;
    [SerializeField]
    private float voxelSize;
    [SerializeField]
    private NavMeshData navMeshData;
    [SerializeField]
    private LayerMask layerMask = (LayerMask) -1;
    [SerializeField]
    private int defaultArea;
    [SerializeField]
    private bool ignoreNavMeshAgent = true;
    [SerializeField]
    private bool ignoreNavMeshObstacle = true;
    [SerializeField]
    private int unwalkableArea = 1;
    private NavMeshDataInstance m_NavMeshDataInstance;
    private Dictionary<Sprite, Mesh> cachedSpriteMeshes = new Dictionary<Sprite, Mesh>();

    public int AgentTypeID
    {
      get => this.agentTypeID;
      set => this.agentTypeID = value;
    }

    public bool OverrideTileSize
    {
      get => this.overrideTileSize;
      set => this.overrideTileSize = value;
    }

    public int TileSize
    {
      get => this.tileSize;
      set => this.tileSize = value;
    }

    public bool OverrideVoxelSize
    {
      get => this.overrideVoxelSize;
      set => this.overrideVoxelSize = value;
    }

    public float VoxelSize
    {
      get => this.voxelSize;
      set => this.voxelSize = value;
    }

    public NavMeshData NavMeshData
    {
      get => this.navMeshData;
      set => this.navMeshData = value;
    }

    public LayerMask LayerMask
    {
      get => this.layerMask;
      set => this.layerMask = value;
    }

    public int DefaultArea
    {
      get => this.defaultArea;
      set => this.defaultArea = value;
    }

    public bool IgnoreNavMeshAgent
    {
      get => this.ignoreNavMeshAgent;
      set => this.ignoreNavMeshAgent = value;
    }

    public bool IgnoreNavMeshObstacle
    {
      get => this.ignoreNavMeshObstacle;
      set => this.ignoreNavMeshObstacle = value;
    }

    public int UnwalkableArea
    {
      get => this.unwalkableArea;
      set => this.unwalkableArea = value;
    }

    public override void Generate(Dungeon dungeon)
    {
      this.BakeNavMesh(dungeon);
      if (this.AddNavMeshLinksBetweenRooms)
      {
        foreach (DoorwayConnection connection in dungeon.Connections)
        {
          foreach (UnityNavMesh2DAdapter.NavMeshAgentLinkInfo navMeshAgentType in this.NavMeshAgentTypes)
            this.AddNavMeshLink(connection, navMeshAgentType);
        }
      }
      if (this.OnProgress == null)
        return;
      this.OnProgress(new NavMeshAdapter.NavMeshGenerationProgress()
      {
        Description = "Done",
        Percentage = 1f
      });
    }

    protected void AddData()
    {
      if (this.m_NavMeshDataInstance.valid || !((UnityEngine.Object) this.navMeshData != (UnityEngine.Object) null))
        return;
      this.m_NavMeshDataInstance = NavMesh.AddNavMeshData(this.navMeshData, this.transform.position, UnityNavMesh2DAdapter.rotation);
      this.m_NavMeshDataInstance.owner = (UnityEngine.Object) this;
    }

    protected void RemoveData()
    {
      this.m_NavMeshDataInstance.Remove();
      this.m_NavMeshDataInstance = new NavMeshDataInstance();
      foreach (KeyValuePair<Sprite, Mesh> cachedSpriteMesh in this.cachedSpriteMeshes)
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) cachedSpriteMesh.Value);
      this.cachedSpriteMeshes.Clear();
    }

    protected virtual void BakeNavMesh(Dungeon dungeon)
    {
      List<NavMeshBuildSource> sources = this.CollectSources();
      Bounds worldBounds = this.CalculateWorldBounds(sources);
      NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(this.GetBuildSettings(), sources, worldBounds, this.transform.position, UnityNavMesh2DAdapter.rotation);
      if ((UnityEngine.Object) navMeshData != (UnityEngine.Object) null)
      {
        navMeshData.name = this.gameObject.name;
        this.RemoveData();
        this.navMeshData = navMeshData;
        if (this.isActiveAndEnabled)
          this.AddData();
      }
      if (this.OnProgress == null)
        return;
      this.OnProgress(new NavMeshAdapter.NavMeshGenerationProgress()
      {
        Description = "Done",
        Percentage = 1f
      });
    }

    protected void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
    {
      List<NavMeshModifierVolume> meshModifierVolumeList = new List<NavMeshModifierVolume>((IEnumerable<NavMeshModifierVolume>) this.GetComponentsInChildren<NavMeshModifierVolume>());
      meshModifierVolumeList.RemoveAll((Predicate<NavMeshModifierVolume>) (x => !x.isActiveAndEnabled));
      foreach (NavMeshModifierVolume meshModifierVolume in meshModifierVolumeList)
      {
        if (((int) this.layerMask & 1 << meshModifierVolume.gameObject.layer) != 0 && meshModifierVolume.AffectsAgentType(this.agentTypeID))
        {
          Vector3 pos = meshModifierVolume.transform.TransformPoint(meshModifierVolume.center);
          Vector3 lossyScale = meshModifierVolume.transform.lossyScale;
          Vector3 vector3 = new Vector3(meshModifierVolume.size.x * Mathf.Abs(lossyScale.x), meshModifierVolume.size.y * Mathf.Abs(lossyScale.y), meshModifierVolume.size.z * Mathf.Abs(lossyScale.z));
          sources.Add(new NavMeshBuildSource()
          {
            shape = NavMeshBuildSourceShape.ModifierBox,
            transform = Matrix4x4.TRS(pos, meshModifierVolume.transform.rotation, Vector3.one),
            size = vector3,
            area = meshModifierVolume.area
          });
        }
      }
    }

    protected virtual List<NavMeshBuildSource> CollectSources()
    {
      List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
      List<NavMeshBuildMarkup> navMeshBuildMarkupList = new List<NavMeshBuildMarkup>();
      List<NavMeshModifier> navMeshModifierList = new List<NavMeshModifier>((IEnumerable<NavMeshModifier>) this.GetComponentsInChildren<NavMeshModifier>());
      navMeshModifierList.RemoveAll((Predicate<NavMeshModifier>) (x => !x.isActiveAndEnabled));
      foreach (NavMeshModifier navMeshModifier in navMeshModifierList)
      {
        if (((int) this.layerMask & 1 << navMeshModifier.gameObject.layer) != 0 && navMeshModifier.AffectsAgentType(this.agentTypeID))
          navMeshBuildMarkupList.Add(new NavMeshBuildMarkup()
          {
            root = navMeshModifier.transform,
            overrideArea = navMeshModifier.overrideArea,
            area = navMeshModifier.area,
            ignoreFromBuild = navMeshModifier.ignoreFromBuild
          });
      }
      NavMeshBuildSource navMeshBuildSource1;
      foreach (SpriteRenderer spriteRenderer in UnityEngine.Object.FindObjectsOfType<SpriteRenderer>())
      {
        Mesh mesh = this.GetMesh(spriteRenderer.sprite);
        if ((UnityEngine.Object) mesh != (UnityEngine.Object) null)
        {
          int unwalkableArea = ((int) this.layerMask & 1 << spriteRenderer.gameObject.layer) == 0 ? this.unwalkableArea : 0;
          List<NavMeshBuildSource> navMeshBuildSourceList = sources;
          navMeshBuildSource1 = new NavMeshBuildSource();
          navMeshBuildSource1.transform = spriteRenderer.transform.localToWorldMatrix;
          navMeshBuildSource1.size = mesh.bounds.extents * 2f;
          navMeshBuildSource1.shape = NavMeshBuildSourceShape.Mesh;
          navMeshBuildSource1.area = unwalkableArea;
          navMeshBuildSource1.sourceObject = (UnityEngine.Object) mesh;
          navMeshBuildSource1.component = (Component) spriteRenderer;
          NavMeshBuildSource navMeshBuildSource2 = navMeshBuildSource1;
          navMeshBuildSourceList.Add(navMeshBuildSource2);
        }
      }
      navMeshBuildSource1 = new NavMeshBuildSource();
      navMeshBuildSource1.shape = NavMeshBuildSourceShape.Mesh;
      navMeshBuildSource1.area = 0;
      NavMeshBuildSource navMeshBuildSource3 = navMeshBuildSource1;
      foreach (Tilemap tilemap in UnityEngine.Object.FindObjectsOfType<Tilemap>())
      {
        BoundsInt cellBounds = tilemap.cellBounds;
        int xMin = cellBounds.xMin;
        while (true)
        {
          int num1 = xMin;
          cellBounds = tilemap.cellBounds;
          int xMax = cellBounds.xMax;
          if (num1 < xMax)
          {
            cellBounds = tilemap.cellBounds;
            int yMin = cellBounds.yMin;
            while (true)
            {
              int num2 = yMin;
              cellBounds = tilemap.cellBounds;
              int yMax = cellBounds.yMax;
              if (num2 < yMax)
              {
                Vector3Int position = new Vector3Int(xMin, yMin, 0);
                if (tilemap.HasTile(position))
                {
                  UnityEngine.Tilemaps.Tile tile = tilemap.GetTile<UnityEngine.Tilemaps.Tile>(position);
                  Mesh mesh = this.GetMesh(tilemap.GetSprite(position));
                  if ((UnityEngine.Object) mesh != (UnityEngine.Object) null)
                  {
                    navMeshBuildSource3.transform = Matrix4x4.TRS(tilemap.GetCellCenterWorld(position) - tilemap.layoutGrid.cellGap, tilemap.transform.rotation, tilemap.transform.lossyScale) * tilemap.orientationMatrix * tilemap.GetTransformMatrix(position);
                    navMeshBuildSource3.sourceObject = (UnityEngine.Object) mesh;
                    navMeshBuildSource3.component = (Component) tilemap;
                    navMeshBuildSource3.area = tile.colliderType == UnityEngine.Tilemaps.Tile.ColliderType.None ? 0 : this.unwalkableArea;
                    sources.Add(navMeshBuildSource3);
                  }
                }
                ++yMin;
              }
              else
                break;
            }
            ++xMin;
          }
          else
            break;
        }
      }
      if (this.ignoreNavMeshAgent)
        sources.RemoveAll((Predicate<NavMeshBuildSource>) (x => (UnityEngine.Object) x.component != (UnityEngine.Object) null && (UnityEngine.Object) x.component.gameObject.GetComponent<NavMeshAgent>() != (UnityEngine.Object) null));
      if (this.ignoreNavMeshObstacle)
        sources.RemoveAll((Predicate<NavMeshBuildSource>) (x => (UnityEngine.Object) x.component != (UnityEngine.Object) null && (UnityEngine.Object) x.component.gameObject.GetComponent<NavMeshObstacle>() != (UnityEngine.Object) null));
      this.AppendModifierVolumes(ref sources);
      return sources;
    }

    protected Mesh GetMesh(Sprite sprite)
    {
      if ((UnityEngine.Object) sprite == (UnityEngine.Object) null)
        return (Mesh) null;
      Mesh mesh;
      if (!this.cachedSpriteMeshes.TryGetValue(sprite, out mesh))
      {
        mesh = new Mesh()
        {
          vertices = ((IEnumerable<Vector2>) sprite.vertices).Select<Vector2, Vector3>((Func<Vector2, Vector3>) (v => new Vector3(v.x, v.y, 0.0f))).ToArray<Vector3>(),
          triangles = ((IEnumerable<ushort>) sprite.triangles).Select<ushort, int>((Func<ushort, int>) (i => (int) i)).ToArray<int>()
        };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        this.cachedSpriteMeshes[sprite] = mesh;
      }
      return mesh;
    }

    protected void AddNavMeshLink(
      DoorwayConnection connection,
      UnityNavMesh2DAdapter.NavMeshAgentLinkInfo agentLinkInfo)
    {
      GameObject gameObject1 = connection.A.gameObject;
      NavMeshBuildSettings settingsById = NavMesh.GetSettingsByID(agentLinkInfo.AgentTypeID);
      float num = Mathf.Max(connection.A.Socket.Size.x - settingsById.agentRadius * 2f, 0.01f);
      NavMeshLink link = gameObject1.AddComponent<NavMeshLink>();
      link.agentTypeID = agentLinkInfo.AgentTypeID;
      link.bidirectional = true;
      link.area = agentLinkInfo.AreaTypeID;
      link.startPoint = new Vector3(0.0f, 0.0f, -this.NavMeshLinkDistanceFromDoorway);
      link.endPoint = new Vector3(0.0f, 0.0f, this.NavMeshLinkDistanceFromDoorway);
      link.width = num;
      if (!agentLinkInfo.DisableLinkWhenDoorIsClosed)
        return;
      GameObject gameObject2 = (UnityEngine.Object) connection.A.UsedDoorPrefabInstance != (UnityEngine.Object) null ? connection.A.UsedDoorPrefabInstance : ((UnityEngine.Object) connection.B.UsedDoorPrefabInstance != (UnityEngine.Object) null ? connection.B.UsedDoorPrefabInstance : (GameObject) null);
      if (!((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null))
        return;
      Door component = gameObject2.GetComponent<Door>();
      link.enabled = component.IsOpen;
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      component.OnDoorStateChanged += (Door.DoorStateChangedDelegate) ((d, o) => link.enabled = o);
    }

    public NavMeshBuildSettings GetBuildSettings()
    {
      NavMeshBuildSettings settingsById = NavMesh.GetSettingsByID(this.agentTypeID);
      if (settingsById.agentTypeID == -1)
      {
        Debug.LogWarning((object) ("No build settings for agent type ID " + this.AgentTypeID.ToString()), (UnityEngine.Object) this);
        settingsById.agentTypeID = this.agentTypeID;
      }
      if (this.OverrideTileSize)
      {
        settingsById.overrideTileSize = true;
        settingsById.tileSize = this.TileSize;
      }
      if (this.OverrideVoxelSize)
      {
        settingsById.overrideVoxelSize = true;
        settingsById.voxelSize = this.VoxelSize;
      }
      return settingsById;
    }

    protected Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
    {
      Matrix4x4 matrix4x4 = Matrix4x4.TRS(this.transform.position, UnityNavMesh2DAdapter.rotation, Vector3.one);
      matrix4x4 = matrix4x4.inverse;
      Bounds worldBounds = new Bounds();
      foreach (NavMeshBuildSource source in sources)
      {
        switch (source.shape)
        {
          case NavMeshBuildSourceShape.Mesh:
            Mesh sourceObject1 = source.sourceObject as Mesh;
            worldBounds.Encapsulate(UnityNavMesh2DAdapter.GetWorldBounds(matrix4x4 * source.transform, sourceObject1.bounds));
            continue;
          case NavMeshBuildSourceShape.Terrain:
            TerrainData sourceObject2 = source.sourceObject as TerrainData;
            worldBounds.Encapsulate(UnityNavMesh2DAdapter.GetWorldBounds(matrix4x4 * source.transform, new Bounds(0.5f * sourceObject2.size, sourceObject2.size)));
            continue;
          case NavMeshBuildSourceShape.Box:
          case NavMeshBuildSourceShape.Sphere:
          case NavMeshBuildSourceShape.Capsule:
          case NavMeshBuildSourceShape.ModifierBox:
            worldBounds.Encapsulate(UnityNavMesh2DAdapter.GetWorldBounds(matrix4x4 * source.transform, new Bounds(Vector3.zero, source.size)));
            continue;
          default:
            continue;
        }
      }
      worldBounds.Expand(0.1f);
      return worldBounds;
    }

    private static Vector3 Abs(Vector3 v)
    {
      return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    private static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
    {
      Vector3 vector3_1 = UnityNavMesh2DAdapter.Abs(mat.MultiplyVector(Vector3.right));
      Vector3 vector3_2 = UnityNavMesh2DAdapter.Abs(mat.MultiplyVector(Vector3.up));
      Vector3 vector3_3 = UnityNavMesh2DAdapter.Abs(mat.MultiplyVector(Vector3.forward));
      return new Bounds(mat.MultiplyPoint(bounds.center), vector3_1 * bounds.size.x + vector3_2 * bounds.size.y + vector3_3 * bounds.size.z);
    }

    [Serializable]
    public sealed class NavMeshAgentLinkInfo
    {
      public int AgentTypeID;
      public int AreaTypeID;
      public bool DisableLinkWhenDoorIsClosed = true;
    }
  }
}
