using EFT;
using EFT.Animations;
using EFT.UI;
using Fika.Core.Coop.Players;
using UnityEngine;

namespace StanceReplication
{
    public class RSR_Observed_Component : MonoBehaviour
    {

        float allowStanceTimer = 0f;
        float cancelStanceTimer = 0f;
        bool canUpdateStance = false;

        float sprintAnim = 1f;
        bool isPatrol = false;
        bool canDoPatrol = false;
        Vector3 targetPosition = Vector3.zero;
        Vector3 packetPosition = Vector3.zero;
        Quaternion packetRotation = Quaternion.identity;
        Quaternion targetRotation = Quaternion.identity;
        ObservedCoopPlayer observedCoopPlayer;

        private void Start()
        {
            observedCoopPlayer = GetComponent<ObservedCoopPlayer>();
            observedCoopPlayer.OnPlayerDead += DeleteThis;
            Plugin.ObservedComponents.Add(observedCoopPlayer.ProfileId, this);
        }

        private void DeleteThis(EFT.Player player, EFT.IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            observedCoopPlayer.OnPlayerDead -= DeleteThis;
            Destroy(this);
        }

        private void Update()
        {
            ProceduralWeaponAnimation pwa = observedCoopPlayer.ProceduralWeaponAnimation;
            if (observedCoopPlayer.IsSprintEnabled || !observedCoopPlayer.ProceduralWeaponAnimation.OverlappingAllowsBlindfire)
            {
                cancelStanceTimer += Time.deltaTime;
                if(cancelStanceTimer >= Plugin.CancelTimer.Value)
                {
                    allowStanceTimer = 0f;
                    canUpdateStance = false;
                }
            }
            else if (!canUpdateStance)
            {
                cancelStanceTimer = 0f;
                allowStanceTimer += Time.deltaTime;
                if (allowStanceTimer >= Plugin.ResetTimer.Value)
                {
                    canUpdateStance = true;
                }
            }

            if (canUpdateStance)
            {
                canDoPatrol = isPatrol;
                targetPosition = Vector3.Lerp(targetPosition, packetPosition, 0.5f);
                targetRotation = Quaternion.Slerp(targetRotation, packetRotation, 0.5f);
                pwa.HandsContainer.HandsPosition.Zero = pwa.PositionZeroSum + targetPosition * pwa.Single_3;
                pwa.HandsContainer.WeaponRootAnim.rotation = Quaternion.Slerp(pwa.HandsContainer.WeaponRootAnim.rotation, packetRotation, 1f);
            }
            else 
            {
                canDoPatrol = false;
                targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, 0.5f);
                targetRotation = Quaternion.Slerp(targetRotation, Quaternion.identity, 0.5f);
            }

            observedCoopPlayer.MovementContext.SetPatrol(canDoPatrol);
            observedCoopPlayer.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, sprintAnim);
        }

        public void SetAnimValues(Vector3 weapPos, Quaternion rot, bool patrol, float anim)
        {
            packetPosition = weapPos;
            packetRotation = rot;
            isPatrol  = patrol;
            sprintAnim = anim;  

        }
    }
}
