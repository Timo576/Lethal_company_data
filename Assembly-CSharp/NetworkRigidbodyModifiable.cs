// Decompiled with JetBrains decompiler
// Type: NetworkRigidbodyModifiable
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

#nullable disable
[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (NetworkTransform))]
public class NetworkRigidbodyModifiable : NetworkBehaviour
{
  private Rigidbody m_Rigidbody;
  private NetworkTransform m_NetworkTransform;
  private bool m_OriginalKinematic;
  public bool kinematicOnOwner;
  public bool nonKinematicWhenDropping;
  private RigidbodyInterpolation m_OriginalInterpolation;
  private bool m_IsAuthority;

  private bool HasAuthority => this.m_NetworkTransform.CanCommitToTransform;

  private void Awake()
  {
    this.m_Rigidbody = this.GetComponent<Rigidbody>();
    this.m_NetworkTransform = this.GetComponent<NetworkTransform>();
  }

  private void FixedUpdate()
  {
    if (!this.NetworkManager.IsListening || this.HasAuthority == this.m_IsAuthority)
      return;
    this.m_IsAuthority = this.HasAuthority;
    this.UpdateRigidbodyKinematicMode();
  }

  public void UpdateRigidbodyKinematicMode()
  {
    if (!this.m_IsAuthority)
    {
      this.m_OriginalKinematic = this.m_Rigidbody.isKinematic;
      this.m_Rigidbody.isKinematic = true;
      this.m_OriginalInterpolation = this.m_Rigidbody.interpolation;
      this.m_Rigidbody.interpolation = RigidbodyInterpolation.None;
    }
    else
    {
      if (this.kinematicOnOwner)
        this.m_Rigidbody.isKinematic = true;
      else if (this.nonKinematicWhenDropping)
      {
        this.m_Rigidbody.isKinematic = false;
        this.nonKinematicWhenDropping = false;
      }
      else
        this.m_Rigidbody.isKinematic = this.m_OriginalKinematic;
      this.m_Rigidbody.interpolation = this.m_OriginalInterpolation;
    }
  }

  public override void OnNetworkSpawn()
  {
    this.m_IsAuthority = this.HasAuthority;
    this.m_OriginalKinematic = this.m_Rigidbody.isKinematic;
    this.m_OriginalInterpolation = this.m_Rigidbody.interpolation;
    this.UpdateRigidbodyKinematicMode();
  }

  public override void OnNetworkDespawn() => this.UpdateRigidbodyKinematicMode();

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (NetworkRigidbodyModifiable);
}
