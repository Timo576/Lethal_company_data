// Decompiled with JetBrains decompiler
// Type: AISearchRoutine
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
[Serializable]
public class AISearchRoutine
{
  public List<GameObject> unsearchedNodes = new List<GameObject>();
  public GameObject currentTargetNode;
  public GameObject nextTargetNode;
  public bool waitingForTargetNode;
  public bool choseTargetNode;
  public Vector3 currentSearchStartPosition;
  public bool loopSearch = true;
  public int timesFinishingSearch;
  public int nodesEliminatedInCurrentSearch;
  public bool inProgress;
  public bool calculatingNodeInSearch;
  [Space(5f)]
  public float searchWidth = 200f;
  public float searchPrecision = 5f;
  public bool randomized;

  public float GetCurrentDistanceOfSearch()
  {
    return Vector3.Distance(this.currentSearchStartPosition, this.currentTargetNode.transform.position);
  }
}
