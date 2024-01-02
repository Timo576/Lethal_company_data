// Decompiled with JetBrains decompiler
// Type: DunGen.TilePlacementData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Graph;
using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [Serializable]
  public sealed class TilePlacementData
  {
    [SerializeField]
    private int pathDepth;
    [SerializeField]
    private float normalizedPathDepth;
    [SerializeField]
    private int branchDepth;
    [SerializeField]
    private float normalizedBranchDepth;
    [SerializeField]
    private bool isOnMainPath;
    [SerializeField]
    private Bounds localBounds;
    [SerializeField]
    private GraphNode graphNode;
    [SerializeField]
    private GraphLine graphLine;
    [SerializeField]
    private DungeonArchetype archetype;
    [SerializeField]
    private TileSet tileSet;
    [SerializeField]
    private Vector3 position = Vector3.zero;
    [SerializeField]
    private Quaternion rotation = Quaternion.identity;

    public int PathDepth
    {
      get => this.pathDepth;
      internal set => this.pathDepth = value;
    }

    public float NormalizedPathDepth
    {
      get => this.normalizedPathDepth;
      internal set => this.normalizedPathDepth = value;
    }

    public int BranchDepth
    {
      get => this.branchDepth;
      internal set => this.branchDepth = value;
    }

    public float NormalizedBranchDepth
    {
      get => this.normalizedBranchDepth;
      internal set => this.normalizedBranchDepth = value;
    }

    public bool IsOnMainPath
    {
      get => this.isOnMainPath;
      internal set => this.isOnMainPath = value;
    }

    public Bounds Bounds { get; private set; }

    public Bounds LocalBounds
    {
      get => this.localBounds;
      internal set
      {
        this.localBounds = value;
        this.RecalculateTransform();
      }
    }

    public GraphNode GraphNode
    {
      get => this.graphNode;
      internal set => this.graphNode = value;
    }

    public GraphLine GraphLine
    {
      get => this.graphLine;
      internal set => this.graphLine = value;
    }

    public DungeonArchetype Archetype
    {
      get => this.archetype;
      internal set => this.archetype = value;
    }

    public TileSet TileSet
    {
      get => this.tileSet;
      internal set => this.tileSet = value;
    }

    public Vector3 Position
    {
      get => this.position;
      set
      {
        this.position = value;
        this.RecalculateTransform();
      }
    }

    public Quaternion Rotation
    {
      get => this.rotation;
      set
      {
        this.rotation = value;
        this.RecalculateTransform();
      }
    }

    public Matrix4x4 Transform { get; private set; }

    public int Depth => !this.isOnMainPath ? this.branchDepth : this.pathDepth;

    public float NormalizedDepth
    {
      get => !this.isOnMainPath ? this.normalizedBranchDepth : this.normalizedPathDepth;
    }

    public InjectedTile InjectionData { get; set; }

    public TilePlacementData() => this.RecalculateTransform();

    public TilePlacementData(TilePlacementData copy)
    {
      this.PathDepth = copy.PathDepth;
      this.NormalizedPathDepth = copy.NormalizedPathDepth;
      this.BranchDepth = copy.BranchDepth;
      this.NormalizedBranchDepth = copy.NormalizedDepth;
      this.IsOnMainPath = copy.IsOnMainPath;
      this.LocalBounds = copy.LocalBounds;
      this.Transform = copy.Transform;
      this.GraphNode = copy.GraphNode;
      this.GraphLine = copy.GraphLine;
      this.Archetype = copy.Archetype;
      this.TileSet = copy.TileSet;
      this.InjectionData = copy.InjectionData;
      this.position = copy.position;
      this.rotation = copy.rotation;
      this.RecalculateTransform();
    }

    private void RecalculateTransform()
    {
      this.Transform = Matrix4x4.TRS(this.position, this.rotation, Vector3.one);
      Vector3 vector3 = this.Transform.MultiplyPoint(this.localBounds.min);
      Vector3 size = this.Transform.MultiplyPoint(this.localBounds.max) - vector3;
      Vector3 center = vector3 + size / 2f;
      size.x = Mathf.Abs(size.x);
      size.y = Mathf.Abs(size.y);
      size.z = Mathf.Abs(size.z);
      this.Bounds = new Bounds(center, size);
    }
  }
}
