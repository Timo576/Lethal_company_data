// Decompiled with JetBrains decompiler
// Type: PlaceableShipObject
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class PlaceableShipObject : MonoBehaviour
{
  public int unlockableID;
  public AudioClip placeObjectSFX;
  public AutoParentToShip parentObject;
  public Transform parentObjectSecondary;
  [Space(3f)]
  public MeshFilter mainMesh;
  public Transform mainTransform;
  public Collider placeObjectCollider;
  public float yOffset;
  [Space(3f)]
  public bool overrideWallOffset;
  public float wallOffset;
  public Vector3 collisionPointCheck;
  public bool doCollisionPointCheck;
  [Space(5f)]
  public bool AllowPlacementOnWalls;
  public bool AllowPlacementOnCounters = true;
  [Space(3f)]
  public bool inUse;
}
