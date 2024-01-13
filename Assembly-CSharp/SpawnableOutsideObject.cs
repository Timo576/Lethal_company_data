// Decompiled with JetBrains decompiler
// Type: SpawnableOutsideObject
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/SpawnableOutsideObject", order = 2)]
public class SpawnableOutsideObject : ScriptableObject
{
  public GameObject prefabToSpawn;
  public bool spawnFacingAwayFromWall;
  [Tooltip("This is used to determine how close this object can spawn to edges of the nav mesh.")]
  public int objectWidth;
  [Tooltip("If null, allows spawning this object on any surface.")]
  public string[] spawnableFloorTags;
  public Vector3 rotationOffset;
}
