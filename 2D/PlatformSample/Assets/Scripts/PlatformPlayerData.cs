using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
I picked this structure up from DawnosaurDev's public repository at: https://github.com/DawnosaurDev/platformer-movement/blob/main/Scripts/Run%20Only/PlayerRunData.cs
*/

[CreateAssetMenu(menuName = "Platform Player Data")]
public class PlatformPlayerData : ScriptableObject
{
        [Header("Run")]
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

    private void OnValidate()
    {
        groundAccelAmount = ((1/Time.fixedDeltaTime) * groundAcceleration) / maxGroundSpeed;
        groundDeccelAmount = ((1/Time.fixedDeltaTime) * groundDecceleration) / maxGroundSpeed;


        #region Variable Ranges
        groundAcceleration = Mathf.Clamp(groundAcceleration, 0.01f, maxGroundSpeed);
        groundDecceleration = Mathf.Clamp(groundDecceleration, 0.01f, maxGroundSpeed);
        #endregion
    }
}
