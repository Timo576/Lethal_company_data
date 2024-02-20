// Decompiled with JetBrains decompiler
// Type: WeatherEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class WeatherEffect
{
  public string name;
  public GameObject effectObject;
  public GameObject effectPermanentObject;
  public bool lerpPosition;
  public bool effectEnabled;
  public string sunAnimatorBool;
  [HideInInspector]
  public bool transitioning;
}
