// Decompiled with JetBrains decompiler
// Type: ScanNodeProperties
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ScanNodeProperties : MonoBehaviour
{
  public int maxRange = 7;
  public int minRange = 5;
  public bool requiresLineOfSight = true;
  [Space(5f)]
  public string headerText;
  public string subText;
  public int scrapValue;
  [Space(5f)]
  public int creatureScanID = -1;
  [Space(3f)]
  public int nodeType;
}
