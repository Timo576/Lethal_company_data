// Decompiled with JetBrains decompiler
// Type: SyncWithServerOnSpawn
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
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
