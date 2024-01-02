// Decompiled with JetBrains decompiler
// Type: SyncWithServerOnSpawn
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class SyncWithServerOnSpawn : NetworkBehaviour
{
  public RoundManager roundManager;
  private bool hasSynced;

  private void Start()
  {
  }

  public void SyncWithServer()
  {
    if (!this.IsServer)
    {
      Object.Destroy((Object) this.gameObject);
    }
    else
    {
      NetworkObject component = this.gameObject.GetComponent<NetworkObject>();
      if (!((Object) component != (Object) null))
        return;
      component.Spawn(true);
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (SyncWithServerOnSpawn);
}
