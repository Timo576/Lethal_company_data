// Decompiled with JetBrains decompiler
// Type: WeatherEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
