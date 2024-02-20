// Decompiled with JetBrains decompiler
// Type: DunGen.Doorway
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Tags;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Doorway")]
  public class Doorway : MonoBehaviour, ISerializationCallbackReceiver
  {
    public const int CurrentFileVersion = 1;
    public int DoorPrefabPriority;
    public List<GameObjectWeight> ConnectorPrefabWeights = new List<GameObjectWeight>();
    public List<GameObjectWeight> BlockerPrefabWeights = new List<GameObjectWeight>();
    public bool AvoidRotatingDoorPrefab;
    public bool AvoidRotatingBlockerPrefab;
    [FormerlySerializedAs("AddWhenInUse")]
    public List<GameObject> ConnectorSceneObjects = new List<GameObject>();
    [FormerlySerializedAs("AddWhenNotInUse")]
    public List<GameObject> BlockerSceneObjects = new List<GameObject>();
    public TagContainer Tags = new TagContainer();
    public int? LockID;
    [SerializeField]
    [FormerlySerializedAs("SocketGroup")]
    private DoorwaySocketType socketGroup_obsolete = ~DoorwaySocketType.Default;
    [SerializeField]
    [FormerlySerializedAs("DoorPrefabs")]
    private List<GameObject> doorPrefabs_obsolete = new List<GameObject>();
    [SerializeField]
    [FormerlySerializedAs("BlockerPrefabs")]
    private List<GameObject> blockerPrefabs_obsolete = new List<GameObject>();
    [SerializeField]
    private DoorwaySocket socket;
    [SerializeField]
    private GameObject doorPrefabInstance;
    [SerializeField]
    private Door doorComponent;
    [SerializeField]
    private Tile tile;
    [SerializeField]
    private Doorway connectedDoorway;
    [SerializeField]
    private bool hideConditionalObjects;
    [SerializeField]
    private int fileVersion;
    internal bool placedByGenerator;

    public bool HasSocketAssigned => (Object) this.socket != (Object) null;

    public DoorwaySocket Socket
    {
      get
      {
        return !((Object) this.socket != (Object) null) ? DunGenSettings.Instance.DefaultSocket : this.socket;
      }
    }

    public Tile Tile
    {
      get => this.tile;
      internal set => this.tile = value;
    }

    public bool IsLocked => this.LockID.HasValue;

    public bool HasDoorPrefabInstance => (Object) this.doorPrefabInstance != (Object) null;

    public GameObject UsedDoorPrefabInstance => this.doorPrefabInstance;

    public Door DoorComponent => this.doorComponent;

    public Dungeon Dungeon { get; internal set; }

    public Doorway ConnectedDoorway
    {
      get => this.connectedDoorway;
      internal set => this.connectedDoorway = value;
    }

    public bool HideConditionalObjects
    {
      get => this.hideConditionalObjects;
      set
      {
        this.hideConditionalObjects = value;
        foreach (GameObject connectorSceneObject in this.ConnectorSceneObjects)
        {
          if ((Object) connectorSceneObject != (Object) null)
            connectorSceneObject.SetActive(!this.hideConditionalObjects);
        }
        foreach (GameObject blockerSceneObject in this.BlockerSceneObjects)
        {
          if ((Object) blockerSceneObject != (Object) null)
            blockerSceneObject.SetActive(!this.hideConditionalObjects);
        }
      }
    }

    private void OnDrawGizmos()
    {
      if (this.placedByGenerator)
        return;
      this.DebugDraw();
    }

    internal void SetUsedPrefab(GameObject doorPrefab)
    {
      this.doorPrefabInstance = doorPrefab;
      if (!((Object) doorPrefab != (Object) null))
        return;
      this.doorComponent = doorPrefab.GetComponent<Door>();
    }

    internal void RemoveUsedPrefab()
    {
      if ((Object) this.doorPrefabInstance != (Object) null)
        UnityUtil.Destroy((Object) this.doorPrefabInstance);
      this.doorPrefabInstance = (GameObject) null;
    }

    internal void DebugDraw()
    {
      Vector2 size = this.Socket.Size;
      Vector2 vector2 = size * 0.5f;
      float num = Mathf.Min(size.x, size.y);
      Gizmos.color = EditorConstants.DoorDirectionColour;
      Gizmos.DrawLine(this.transform.position + this.transform.up * vector2.y, this.transform.position + this.transform.up * vector2.y + this.transform.forward * num);
      Gizmos.color = EditorConstants.DoorUpColour;
      Gizmos.DrawLine(this.transform.position + this.transform.up * vector2.y, this.transform.position + this.transform.up * size.y);
      Gizmos.color = EditorConstants.DoorRectColour;
      Vector3 vector3_1 = this.transform.position - this.transform.right * vector2.x + this.transform.up * size.y;
      Vector3 vector3_2 = this.transform.position + this.transform.right * vector2.x + this.transform.up * size.y;
      Vector3 vector3_3 = this.transform.position - this.transform.right * vector2.x;
      Vector3 vector3_4 = this.transform.position + this.transform.right * vector2.x;
      Gizmos.DrawLine(vector3_1, vector3_2);
      Gizmos.DrawLine(vector3_2, vector3_4);
      Gizmos.DrawLine(vector3_4, vector3_3);
      Gizmos.DrawLine(vector3_3, vector3_1);
    }

    public void OnBeforeSerialize() => this.fileVersion = 1;

    public void OnAfterDeserialize()
    {
      if (this.fileVersion >= 1)
        return;
      foreach (GameObject gameObject in this.doorPrefabs_obsolete)
        this.ConnectorPrefabWeights.Add(new GameObjectWeight(gameObject));
      foreach (GameObject gameObject in this.blockerPrefabs_obsolete)
        this.BlockerPrefabWeights.Add(new GameObjectWeight(gameObject));
      this.doorPrefabs_obsolete.Clear();
      this.blockerPrefabs_obsolete.Clear();
    }
  }
}
