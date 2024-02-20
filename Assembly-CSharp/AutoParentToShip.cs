// Decompiled with JetBrains decompiler
// Type: AutoParentToShip
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class AutoParentToShip : NetworkBehaviour
{
  public bool disableObject;
  public Vector3 positionOffset;
  public Vector3 rotationOffset;
  [HideInInspector]
  public Vector3 startingPosition;
  [HideInInspector]
  public Vector3 startingRotation;
  public bool overrideOffset;

  private void Awake()
  {
    if (!this.overrideOffset)
    {
      this.positionOffset = StartOfRound.Instance.elevatorTransform.InverseTransformPoint(this.transform.position);
      this.rotationOffset = StartOfRound.Instance.elevatorTransform.InverseTransformDirection(this.transform.eulerAngles);
    }
    this.MoveToOffset();
    PlaceableShipObject component = this.gameObject.GetComponent<PlaceableShipObject>();
    if ((Object) component != (Object) null && (Object) component.parentObjectSecondary != (Object) null)
    {
      this.startingPosition = component.parentObjectSecondary.position;
      this.startingRotation = component.parentObjectSecondary.eulerAngles;
    }
    else
    {
      this.startingPosition = this.positionOffset;
      this.startingRotation = this.rotationOffset;
    }
  }

  private void LateUpdate()
  {
    if (StartOfRound.Instance.suckingFurnitureOutOfShip)
      return;
    if (this.disableObject)
      this.transform.position = new Vector3(800f, -100f, 0.0f);
    else
      this.MoveToOffset();
  }

  public void StartSuckingOutOfShip() => this.StartCoroutine(this.SuckObjectOutOfShip());

  private IEnumerator SuckObjectOutOfShip()
  {
    AutoParentToShip autoParentToShip = this;
    Vector3 dir = Vector3.Normalize((StartOfRound.Instance.middleOfSpaceNode.position - autoParentToShip.transform.position) * 10000f);
    Debug.Log((object) dir);
    Quaternion randomRotation = Random.rotation;
    while (StartOfRound.Instance.suckingFurnitureOutOfShip)
    {
      yield return (object) null;
      autoParentToShip.transform.position = autoParentToShip.transform.position + dir * (float) ((double) Time.deltaTime * (double) Mathf.Clamp(StartOfRound.Instance.suckingPower, 1.1f, 100f) * 17.0);
      autoParentToShip.transform.rotation = Quaternion.Lerp(autoParentToShip.transform.rotation, autoParentToShip.transform.rotation * randomRotation, Time.deltaTime * StartOfRound.Instance.suckingPower);
      Debug.DrawRay(autoParentToShip.transform.position + Vector3.up * 0.2f, StartOfRound.Instance.middleOfSpaceNode.position - autoParentToShip.transform.position, Color.blue);
      Debug.DrawRay(autoParentToShip.transform.position, dir, Color.green);
    }
  }

  public void MoveToOffset()
  {
    this.transform.rotation = StartOfRound.Instance.elevatorTransform.rotation;
    this.transform.Rotate(this.rotationOffset);
    this.transform.position = StartOfRound.Instance.elevatorTransform.position;
    this.transform.position += StartOfRound.Instance.elevatorTransform.rotation * this.positionOffset;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (AutoParentToShip);
}
