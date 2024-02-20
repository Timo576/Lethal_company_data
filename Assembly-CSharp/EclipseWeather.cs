// Decompiled with JetBrains decompiler
// Type: EclipseWeather
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class EclipseWeather : MonoBehaviour
{
  private void OnEnable()
  {
    RoundManager.Instance.minOutsideEnemiesToSpawn = (int) TimeOfDay.Instance.currentWeatherVariable;
    RoundManager.Instance.minEnemiesToSpawn = (int) TimeOfDay.Instance.currentWeatherVariable;
  }
}
