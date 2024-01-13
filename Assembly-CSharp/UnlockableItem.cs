// Decompiled with JetBrains decompiler
// Type: UnlockableItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class UnlockableItem
{
  public string unlockableName;
  public GameObject prefabObject;
  public int unlockableType;
  [Space(5f)]
  public TerminalNode shopSelectionNode;
  public bool alwaysInStock;
  [Space(3f)]
  public bool IsPlaceable;
  [Space(3f)]
  public bool hasBeenMoved;
  public Vector3 placedPosition;
  public Vector3 placedRotation;
  [Space(3f)]
  public bool inStorage;
  public bool canBeStored = true;
  public int maxNumber = 1;
  [Space(3f)]
  public bool hasBeenUnlockedByPlayer;
  [Space(5f)]
  public Material suitMaterial;
  public bool alreadyUnlocked;
  public bool unlockedInChallengeFile;
  public bool spawnPrefab = true;
}
