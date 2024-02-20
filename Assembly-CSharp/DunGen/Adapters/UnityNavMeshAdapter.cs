// Decompiled with JetBrains decompiler
// Type: DunGen.Adapters.UnityNavMeshAdapter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
namespace DunGen.Adapters
{
  [AddComponentMenu("DunGen/NavMesh/Unity NavMesh Adapter")]
  public class UnityNavMeshAdapter : NavMeshAdapter
  {
    public UnityNavMeshAdapter.RuntimeNavMeshBakeMode BakeMode = UnityNavMeshAdapter.RuntimeNavMeshBakeMode.AddIfNoSurfaceExists;
    public LayerMask LayerMask = (LayerMask) -1;
    public bool AddNavMeshLinksBetweenRooms = true;
    public List<UnityNavMeshAdapter.NavMeshAgentLinkInfo> NavMeshAgentTypes = new List<UnityNavMeshAdapter.NavMeshAgentLinkInfo>()
    {
      new UnityNavMeshAdapter.NavMeshAgentLinkInfo()
    };
    public float NavMeshLinkDistanceFromDoorway = 2.5f;
    public bool AutoGenerateFullRebakeSurfaces = true;
    public List<NavMeshSurface> FullRebakeTargets = new List<NavMeshSurface>();
    private List<NavMeshSurface> addedSurfaces = new List<NavMeshSurface>();
    private List<NavMeshSurface> fullBakeSurfaces = new List<NavMeshSurface>();

    public override void Generate(Dungeon dungeon)
    {
      if (this.BakeMode == UnityNavMeshAdapter.RuntimeNavMeshBakeMode.FullDungeonBake)
      {
        this.BakeFullDungeon(dungeon);
      }
      else
      {
        if (this.BakeMode != UnityNavMeshAdapter.RuntimeNavMeshBakeMode.PreBakedOnly)
        {
          foreach (Tile allTile in dungeon.AllTiles)
          {
            NavMeshSurface[] componentsInChildren = allTile.gameObject.GetComponentsInChildren<NavMeshSurface>();
            IEnumerable<NavMeshSurface> navMeshSurfaces = (IEnumerable<NavMeshSurface>) this.AddMissingSurfaces(allTile, componentsInChildren);
            if (this.BakeMode == UnityNavMeshAdapter.RuntimeNavMeshBakeMode.AlwaysRebake)
              navMeshSurfaces = navMeshSurfaces.Concat<NavMeshSurface>((IEnumerable<NavMeshSurface>) componentsInChildren);
            else if (this.BakeMode == UnityNavMeshAdapter.RuntimeNavMeshBakeMode.AddIfNoSurfaceExists)
            {
              IEnumerable<NavMeshSurface> second = ((IEnumerable<NavMeshSurface>) componentsInChildren).Where<NavMeshSurface>((Func<NavMeshSurface, bool>) (x => (UnityEngine.Object) x.navMeshData == (UnityEngine.Object) null));
              navMeshSurfaces = navMeshSurfaces.Concat<NavMeshSurface>(second);
            }
            foreach (NavMeshSurface navMeshSurface in navMeshSurfaces.Distinct<NavMeshSurface>())
              navMeshSurface.BuildNavMesh();
          }
        }
        if (this.AddNavMeshLinksBetweenRooms)
        {
          foreach (DoorwayConnection connection in dungeon.Connections)
          {
            foreach (UnityNavMeshAdapter.NavMeshAgentLinkInfo navMeshAgentType in this.NavMeshAgentTypes)
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
    }

    private void BakeFullDungeon(Dungeon dungeon)
    {
      if (this.AutoGenerateFullRebakeSurfaces)
      {
        foreach (NavMeshSurface fullBakeSurface in this.fullBakeSurfaces)
        {
          if ((UnityEngine.Object) fullBakeSurface != (UnityEngine.Object) null)
            fullBakeSurface.RemoveData();
        }
        this.fullBakeSurfaces.Clear();
        int settingsCount = NavMesh.GetSettingsCount();
        for (int index = 0; index < settingsCount; ++index)
        {
          NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index);
          NavMeshSurface navMeshSurface = ((IEnumerable<NavMeshSurface>) dungeon.gameObject.GetComponents<NavMeshSurface>()).Where<NavMeshSurface>((Func<NavMeshSurface, bool>) (s => s.agentTypeID == settings.agentTypeID)).FirstOrDefault<NavMeshSurface>();
          if ((UnityEngine.Object) navMeshSurface == (UnityEngine.Object) null)
          {
            navMeshSurface = dungeon.gameObject.AddComponent<NavMeshSurface>();
            navMeshSurface.agentTypeID = settings.agentTypeID;
            navMeshSurface.collectObjects = CollectObjects.Children;
            navMeshSurface.layerMask = this.LayerMask;
          }
          this.fullBakeSurfaces.Add(navMeshSurface);
          navMeshSurface.BuildNavMesh();
        }
      }
      else
      {
        foreach (NavMeshSurface fullRebakeTarget in this.FullRebakeTargets)
          fullRebakeTarget.BuildNavMesh();
      }
      if (this.OnProgress == null)
        return;
      this.OnProgress(new NavMeshAdapter.NavMeshGenerationProgress()
      {
        Description = "Done",
        Percentage = 1f
      });
    }

    private NavMeshSurface[] AddMissingSurfaces(Tile tile, NavMeshSurface[] existingSurfaces)
    {
      this.addedSurfaces.Clear();
      int settingsCount = NavMesh.GetSettingsCount();
      for (int index = 0; index < settingsCount; ++index)
      {
        NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index);
        if (!((IEnumerable<NavMeshSurface>) existingSurfaces).Where<NavMeshSurface>((Func<NavMeshSurface, bool>) (x => x.agentTypeID == settings.agentTypeID)).Any<NavMeshSurface>())
        {
          NavMeshSurface navMeshSurface = tile.gameObject.AddComponent<NavMeshSurface>();
          navMeshSurface.agentTypeID = settings.agentTypeID;
          navMeshSurface.collectObjects = CollectObjects.Children;
          navMeshSurface.layerMask = this.LayerMask;
          this.addedSurfaces.Add(navMeshSurface);
        }
      }
      return this.addedSurfaces.ToArray();
    }

    private void AddNavMeshLink(
      DoorwayConnection connection,
      UnityNavMeshAdapter.NavMeshAgentLinkInfo agentLinkInfo)
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

    public enum RuntimeNavMeshBakeMode
    {
      PreBakedOnly,
      AddIfNoSurfaceExists,
      AlwaysRebake,
      FullDungeonBake,
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
