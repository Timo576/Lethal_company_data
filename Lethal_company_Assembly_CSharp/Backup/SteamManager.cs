// Decompiled with JetBrains decompiler
// Type: SteamManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
