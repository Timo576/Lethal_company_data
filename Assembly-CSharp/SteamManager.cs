// Decompiled with JetBrains decompiler
// Type: SteamManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using UnityEngine;

#nullable disable
public class SteamManager : MonoBehaviour
{
  public static SteamManager Instance { get; private set; }

  private void Awake()
  {
    if ((Object) SteamManager.Instance == (Object) null)
      SteamManager.Instance = this;
    else
      Object.Destroy((Object) this.gameObject);
  }

  private void OnDisable() => SteamClient.Shutdown();

  private void Update() => SteamClient.RunCallbacks();
}
