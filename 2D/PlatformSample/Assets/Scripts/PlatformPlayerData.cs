using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
I picked this structure up from DawnosaurDev's public repository at: https://github.com/DawnosaurDev/platformer-movement/blob/main/Scripts/Run%20Only/PlayerRunData.cs
*/

[CreateAssetMenu(menuName = "Platform Player Data")]
public class PlatformPlayerData : ScriptableObject
{
        [Header("Lateral movement")]
        public float maxGroundSpeed; // Player's maximum speed on the ground.
        public float groundAcceleration; // How fast the player reach max ground speed (higher values means they do so FASTER);
        [HideInInspector] public float groundAccelAmount; // The force that will be applied to the player (calculated with the speed difference) to help them speed up.
        public float groundDecceleration; // How fast the player will go from their maximum ground speed to a complete stop (higher value means they do so FASTER).
        [HideInInspector] public float groundDeccelAmount; // The force that will be applied to the player to help them come to a stop.
        [Space(10)]
        [Range(0.01f, 1)] public float airAccelModifier; // The modifier applied to air acceleration. Do note that a max air speed could be defined if necessary.
        [Range(0.01f, 1)] public float airDeccelModifier; /* The modifier applied to air decceleration. One more note: these are currently contained between 0 and 1,
                                                            but your air accelaration absolutely could be higher than your ground acceleration.*/
        public bool conserveMomentum;

        [Header("Upwards movement")]
        public float jumpPower; // The amount of force (impulse) to add to the player when they jump.
        public float jumpGravity; // Helps making the player fall faster without affecting their gravity.
        public float fallGravityModifier; // How much higher should the player's gravity when they fall.
        public float maxFallSpeed; // What's the fast the player may fall.
        public float hangGravityModifier; // How much to cut the gravity when in the jump's hang time.
        public float hangAbsVelocity; // What speeds to consider as the jump's hang time.
        [Range(0.1f, 1)] public float jumpSlowdown; // How hard should the player slow down once they release the jump button.

        [Header("Input buffering")]
        [Range(0.1f, 1)] public float coyotteTime; // Time (in seconds) after leaving a platform during which the player can still perform a jump.
        [Range(0.1f, 1)] public float jumpBufferTime; // How long the input buffer for jumps should be before hitting the ground.

        [Header("Wall sliding & wall jumps")]
        public float wallSlidingSpeed; // How fast should the player be going down when they are wallsliding.
        public float wallKickOff; // How far back should the player be pushed when walljumping.
        public float wallJumpPower; // How much height should they get from a wall jump.
        public float wallDashPower;
        /* Next two variables need to be explained together. Basically, when doing a wall jump, we reduce the player's air acceleration by wallAccelModifier for a short duration
           wallAccelTime. This allows us to control how easily a player can get back to the same wall they jumped off from without kicking them too far back. Setting the duration
           to 0 and the modifier to 1, and with sufficient air acceleration, the player may be able to reach the wall higher than where they left from.*/
        [Range(0f, 1)] public float wallAccelModifier;
        [Range(0f, 1)] public float wallAccelTime;

        [Header("Double jumping")]
        public float doubleJumpPower; // How much height should the player get from a double jump.
        public int doubleJumpMax; // How many double jumps can a player perform without landing to replenish.

        [Header("Dashing")]
        // Both dashPower and dashDuration will change how far a dash travels, but dashPower will also provide momentum to player if they jump-cancel a dash.
        public float dashPower; // How much impulse should a dash provide.
        public float dashDuration; // How long should a dash last.
        public float dashGraceTime; // A tiny grace period during which gravity is still off so the player can react to the dash.
        public float dashCooldown; // How long after dashing can a player dash again.
        public int dashMax; // How many dashes can a player perform without landing to replenish.

    private void OnValidate()
    {
        #region Calculating acceleration
        groundAccelAmount = ((1/Time.fixedDeltaTime) * groundAcceleration) / maxGroundSpeed;
        groundDeccelAmount = ((1/Time.fixedDeltaTime) * groundDecceleration) / maxGroundSpeed;
        #endregion

        #region Variable Ranges
        groundAcceleration = Mathf.Clamp(groundAcceleration, 0.01f, maxGroundSpeed);
        groundDecceleration = Mathf.Clamp(groundDecceleration, 0.01f, maxGroundSpeed);
        #endregion
    }
}
