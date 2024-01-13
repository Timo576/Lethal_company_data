// Decompiled with JetBrains decompiler
// Type: DunGen.BasicRoomCullingCamera
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Culling/Adjacent Room Culling (Multi-Camera)")]
  public class BasicRoomCullingCamera : MonoBehaviour
  {
    public int AdjacentTileDepth = 1;
    public bool CullBehindClosedDoors = true;
    public Transform TargetOverride;
    public bool CullInEditor;
    public bool CullLights = true;
    protected bool isCulling;
    protected bool isDirty;
    protected DungeonGenerator generator;
    protected Tile currentTile;
    protected List<Tile> allTiles;
    protected List<Door> allDoors;
    protected List<Tile> visibleTiles;
    protected Dictionary<Tile, List<BasicRoomCullingCamera.RendererData>> rendererVisibilities = new Dictionary<Tile, List<BasicRoomCullingCamera.RendererData>>();
    protected Dictionary<Tile, List<BasicRoomCullingCamera.LightData>> lightVisibilities = new Dictionary<Tile, List<BasicRoomCullingCamera.LightData>>();
    protected Dictionary<Tile, List<BasicRoomCullingCamera.ReflectionProbeData>> reflectionProbeVisibilities = new Dictionary<Tile, List<BasicRoomCullingCamera.ReflectionProbeData>>();
    protected Dictionary<Door, List<BasicRoomCullingCamera.RendererData>> doorRendererVisibilities = new Dictionary<Door, List<BasicRoomCullingCamera.RendererData>>();

    public bool IsReady { get; protected set; }

    protected virtual void Awake()
    {
      RuntimeDungeon objectOfType = UnityEngine.Object.FindObjectOfType<RuntimeDungeon>();
      if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null))
        return;
      this.generator = objectOfType.Generator;
      this.generator.OnGenerationStatusChanged += new GenerationStatusDelegate(this.OnDungeonGenerationStatusChanged);
      if (this.generator.Status != GenerationStatus.Complete)
        return;
      this.SetDungeon(this.generator.CurrentDungeon);
    }

    protected virtual void OnDestroy()
    {
      if (this.generator == null)
        return;
      this.generator.OnGenerationStatusChanged -= new GenerationStatusDelegate(this.OnDungeonGenerationStatusChanged);
    }

    protected virtual void OnEnable()
    {
      if (RenderPipelineManager.currentPipeline != null)
      {
        RenderPipelineManager.beginCameraRendering += new Action<ScriptableRenderContext, Camera>(this.OnBeginCameraRendering);
        RenderPipelineManager.endCameraRendering += new Action<ScriptableRenderContext, Camera>(this.OnEndCameraRendering);
      }
      else
      {
        Camera.onPreCull += new Camera.CameraCallback(this.EnableCulling);
        Camera.onPostRender += new Camera.CameraCallback(this.DisableCulling);
      }
    }

    protected virtual void OnDisable()
    {
      RenderPipelineManager.beginCameraRendering -= new Action<ScriptableRenderContext, Camera>(this.OnBeginCameraRendering);
      RenderPipelineManager.endCameraRendering -= new Action<ScriptableRenderContext, Camera>(this.OnEndCameraRendering);
      Camera.onPreCull -= new Camera.CameraCallback(this.EnableCulling);
      Camera.onPostRender -= new Camera.CameraCallback(this.DisableCulling);
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
      this.EnableCulling(camera);
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
      this.DisableCulling(camera);
    }

    protected virtual void OnDungeonGenerationStatusChanged(
      DungeonGenerator generator,
      GenerationStatus status)
    {
      if (status == GenerationStatus.Complete)
      {
        this.SetDungeon(generator.CurrentDungeon);
      }
      else
      {
        if (status != GenerationStatus.Failed)
          return;
        this.ClearDungeon();
      }
    }

    protected virtual void EnableCulling(Camera camera) => this.SetCullingEnabled(camera, true);

    protected virtual void DisableCulling(Camera camera) => this.SetCullingEnabled(camera, false);

    protected void SetCullingEnabled(Camera camera, bool enabled)
    {
      if (!this.IsReady || !((UnityEngine.Object) camera.gameObject == (UnityEngine.Object) this.gameObject))
        return;
      this.SetIsCulling(enabled);
    }

    protected virtual void LateUpdate()
    {
      if (!this.IsReady)
        return;
      Transform transform = (UnityEngine.Object) this.TargetOverride != (UnityEngine.Object) null ? this.TargetOverride : this.transform;
      if (((UnityEngine.Object) this.currentTile == (UnityEngine.Object) null ? 1 : (!this.currentTile.Bounds.Contains(transform.position) ? 1 : 0)) != 0)
      {
        foreach (Tile allTile in this.allTiles)
        {
          if (!((UnityEngine.Object) allTile == (UnityEngine.Object) null) && allTile.Bounds.Contains(transform.position))
          {
            this.currentTile = allTile;
            break;
          }
        }
        this.isDirty = true;
      }
      if (!this.isDirty)
        return;
      this.UpdateCulling();
      foreach (Tile allTile in this.allTiles)
      {
        if (!this.visibleTiles.Contains(allTile))
          this.UpdateRendererList(allTile);
      }
    }

    protected void UpdateRendererList(Tile tile)
    {
      List<BasicRoomCullingCamera.RendererData> rendererDataList;
      if (!this.rendererVisibilities.TryGetValue(tile, out rendererDataList))
        this.rendererVisibilities[tile] = rendererDataList = new List<BasicRoomCullingCamera.RendererData>();
      else
        rendererDataList.Clear();
      foreach (Renderer componentsInChild in tile.GetComponentsInChildren<Renderer>())
        rendererDataList.Add(new BasicRoomCullingCamera.RendererData(componentsInChild, componentsInChild.enabled));
      if (this.CullLights)
      {
        List<BasicRoomCullingCamera.LightData> lightDataList;
        if (!this.lightVisibilities.TryGetValue(tile, out lightDataList))
          this.lightVisibilities[tile] = lightDataList = new List<BasicRoomCullingCamera.LightData>();
        else
          lightDataList.Clear();
        foreach (Light componentsInChild in tile.GetComponentsInChildren<Light>())
          lightDataList.Add(new BasicRoomCullingCamera.LightData(componentsInChild, componentsInChild.enabled));
      }
      List<BasicRoomCullingCamera.ReflectionProbeData> reflectionProbeDataList;
      if (!this.reflectionProbeVisibilities.TryGetValue(tile, out reflectionProbeDataList))
        this.reflectionProbeVisibilities[tile] = reflectionProbeDataList = new List<BasicRoomCullingCamera.ReflectionProbeData>();
      else
        reflectionProbeDataList.Clear();
      foreach (ReflectionProbe componentsInChild in tile.GetComponentsInChildren<ReflectionProbe>())
        reflectionProbeDataList.Add(new BasicRoomCullingCamera.ReflectionProbeData(componentsInChild, componentsInChild.enabled));
    }

    protected void SetIsCulling(bool isCulling)
    {
      this.isCulling = isCulling;
      for (int index = 0; index < this.allTiles.Count; ++index)
      {
        Tile allTile = this.allTiles[index];
        if (!this.visibleTiles.Contains(allTile))
        {
          List<BasicRoomCullingCamera.RendererData> rendererDataList;
          if (this.rendererVisibilities.TryGetValue(allTile, out rendererDataList))
          {
            foreach (BasicRoomCullingCamera.RendererData rendererData in rendererDataList)
              rendererData.Renderer.enabled = !isCulling && rendererData.Enabled;
          }
          List<BasicRoomCullingCamera.LightData> lightDataList;
          if (this.CullLights && this.lightVisibilities.TryGetValue(allTile, out lightDataList))
          {
            foreach (BasicRoomCullingCamera.LightData lightData in lightDataList)
              lightData.Light.enabled = !isCulling && lightData.Enabled;
          }
          List<BasicRoomCullingCamera.ReflectionProbeData> reflectionProbeDataList;
          if (this.reflectionProbeVisibilities.TryGetValue(allTile, out reflectionProbeDataList))
          {
            foreach (BasicRoomCullingCamera.ReflectionProbeData reflectionProbeData in reflectionProbeDataList)
              reflectionProbeData.Probe.enabled = !isCulling && reflectionProbeData.Enabled;
          }
        }
      }
      foreach (Door allDoor in this.allDoors)
      {
        bool flag = this.visibleTiles.Contains(allDoor.DoorwayA.Tile) || this.visibleTiles.Contains(allDoor.DoorwayB.Tile);
        List<BasicRoomCullingCamera.RendererData> rendererDataList;
        if (this.doorRendererVisibilities.TryGetValue(allDoor, out rendererDataList))
        {
          foreach (BasicRoomCullingCamera.RendererData rendererData in rendererDataList)
            rendererData.Renderer.enabled = isCulling ? flag : rendererData.Enabled;
        }
      }
    }

    protected void UpdateCulling()
    {
      this.isDirty = false;
      this.visibleTiles.Clear();
      if ((UnityEngine.Object) this.currentTile != (UnityEngine.Object) null)
        this.visibleTiles.Add(this.currentTile);
      int num = 0;
      for (int index1 = 0; index1 < this.AdjacentTileDepth; ++index1)
      {
        int count = this.visibleTiles.Count;
        for (int index2 = num; index2 < count; ++index2)
        {
          foreach (Doorway usedDoorway in this.visibleTiles[index2].UsedDoorways)
          {
            Tile tile = usedDoorway.ConnectedDoorway.Tile;
            if (!this.visibleTiles.Contains(tile))
            {
              if (this.CullBehindClosedDoors)
              {
                Door doorComponent = usedDoorway.DoorComponent;
                if ((UnityEngine.Object) doorComponent != (UnityEngine.Object) null && doorComponent.ShouldCullBehind)
                  continue;
              }
              this.visibleTiles.Add(tile);
            }
          }
        }
        num = count;
      }
    }

    public void SetDungeon(Dungeon dungeon)
    {
      if (this.IsReady)
        this.ClearDungeon();
      if ((UnityEngine.Object) dungeon == (UnityEngine.Object) null)
        return;
      this.allTiles = new List<Tile>((IEnumerable<Tile>) dungeon.AllTiles);
      this.allDoors = new List<Door>(this.GetAllDoorsInDungeon(dungeon));
      this.visibleTiles = new List<Tile>(this.allTiles.Count);
      this.doorRendererVisibilities.Clear();
      foreach (Door allDoor in this.allDoors)
      {
        List<BasicRoomCullingCamera.RendererData> rendererDataList = new List<BasicRoomCullingCamera.RendererData>();
        this.doorRendererVisibilities[allDoor] = rendererDataList;
        foreach (Renderer componentsInChild in allDoor.GetComponentsInChildren<Renderer>(true))
          rendererDataList.Add(new BasicRoomCullingCamera.RendererData(componentsInChild, componentsInChild.enabled));
        allDoor.OnDoorStateChanged += new Door.DoorStateChangedDelegate(this.OnDoorStateChanged);
      }
      this.IsReady = true;
      this.isDirty = true;
    }

    protected IEnumerable<Door> GetAllDoorsInDungeon(Dungeon dungeon)
    {
      foreach (GameObject door in dungeon.Doors)
      {
        if (!((UnityEngine.Object) door == (UnityEngine.Object) null))
        {
          Door component = door.GetComponent<Door>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            yield return component;
        }
      }
    }

    protected virtual void ClearDungeon()
    {
      foreach (Door allDoor in this.allDoors)
        allDoor.OnDoorStateChanged -= new Door.DoorStateChangedDelegate(this.OnDoorStateChanged);
      this.IsReady = false;
    }

    protected virtual void OnDoorStateChanged(Door door, bool isOpen) => this.isDirty = true;

    protected struct RendererData
    {
      public Renderer Renderer;
      public bool Enabled;

      public RendererData(Renderer renderer, bool enabled)
      {
        this.Renderer = renderer;
        this.Enabled = enabled;
      }
    }

    protected struct LightData
    {
      public Light Light;
      public bool Enabled;

      public LightData(Light light, bool enabled)
      {
        this.Light = light;
        this.Enabled = enabled;
      }
    }

    protected struct ReflectionProbeData
    {
      public ReflectionProbe Probe;
      public bool Enabled;

      public ReflectionProbeData(ReflectionProbe probe, bool enabled)
      {
        this.Probe = probe;
        this.Enabled = enabled;
      }
    }
  }
}
