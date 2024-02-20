// Decompiled with JetBrains decompiler
// Type: DocileLocustBeesAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

#nullable disable
public class DocileLocustBeesAI : EnemyAI
{
  private int previousBehaviour;
  public AISearchRoutine bugsRoam;
  public VisualEffect bugsEffect;
  private float timeSinceReturning;
  public ScanNodeProperties scanNode;

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.allPlayersDead || this.daytimeEnemyLeaving)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (!this.bugsRoam.inProgress)
          this.StartSearch(this.transform.position, this.bugsRoam);
        if (!Physics.CheckSphere(this.transform.position, 8f, 520, QueryTriggerInteraction.Collide))
          break;
        this.SwitchToBehaviourState(1);
        break;
      case 1:
        if (Physics.CheckSphere(this.transform.position, 14f, 520, QueryTriggerInteraction.Collide))
          break;
        this.SwitchToBehaviourState(0);
        break;
    }
  }

  public override void Update()
  {
    base.Update();
    this.bugsEffect.SetBool("Alive", (double) Vector3.Distance(StartOfRound.Instance.activeCamera.transform.position, this.transform.position) < 62.0);
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.previousBehaviour != 0)
        {
          this.previousBehaviour = 0;
          this.bugsEffect.SetFloat("MoveToTargetForce", 6f);
          this.creatureVoice.Play();
        }
        this.scanNode.maxRange = 18;
        this.timeSinceReturning += Time.deltaTime;
        this.creatureVoice.volume = Mathf.Min(this.creatureVoice.volume + Time.deltaTime * 0.25f, 1f);
        break;
      case 1:
        if (this.previousBehaviour != 1)
        {
          this.previousBehaviour = 1;
          this.bugsEffect.SetFloat("MoveToTargetForce", -35f);
          if ((double) this.timeSinceReturning > 2.0)
          {
            this.creatureSFX.PlayOneShot(this.enemyType.audioClips[0]);
            WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.enemyType.audioClips[0], 0.8f);
            RoundManager.Instance.PlayAudibleNoise(this.transform.position, 8f, 0.35f, noiseID: 14152);
          }
          this.timeSinceReturning = 0.0f;
        }
        this.scanNode.maxRange = 1;
        if ((double) this.creatureVoice.volume > 0.0)
        {
          this.creatureVoice.volume = Mathf.Max(this.creatureVoice.volume - Time.deltaTime * 1.75f, 0.0f);
          break;
        }
        this.creatureVoice.Stop();
        break;
    }
  }

  public override void DaytimeEnemyLeave()
  {
    base.DaytimeEnemyLeave();
    this.bugsEffect.SetFloat("MoveToTargetForce", -15f);
    this.creatureSFX.PlayOneShot(this.enemyType.audioClips[0], 0.5f);
    WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.enemyType.audioClips[0], 0.4f);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 6f, 0.2f, noiseID: 14152);
    this.StartCoroutine(this.bugsLeave());
  }

  private IEnumerator bugsLeave()
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    DocileLocustBeesAI docileLocustBeesAi = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      docileLocustBeesAi.KillEnemyOnOwnerClient(true);
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = (object) new WaitForSeconds(6f);
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (DocileLocustBeesAI);
}
