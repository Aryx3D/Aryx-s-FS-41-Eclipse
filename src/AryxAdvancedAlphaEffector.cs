using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Aryx_NavalInterceptor1
{
    //alpha effector for actual aero changes outside of rotation manipulation
    public sealed class AryxAdvancedAlphaEffector : MonoBehaviour
    {
        [SerializeField]
        private Aircraft aircraft;
        [SerializeField]
        private AeroPart criticalPart;

        [Header("The shit I gotta do to dance around blueprinter sometimes.")]
        [SerializeField]
        private Transform[] effectors; //transforms to rotate
        [SerializeField]
        private float[] rotationSpeeds; //max angle change per update
        [SerializeField]
        private float[] maxAngles;
        [SerializeField]
        private float[] minAngles;
        [SerializeField]
        private AnimationCurve[] modifiers; //targetangle[i] += modifier curve[i] * alpha
        [SerializeField]
        private float[] maxAlpha; //targetangle[i] += modifier curve[i] * alpha
        [SerializeField]
        private float[] pitchModifiers; //targetangle[i] += modifiers[i] * aircraft pitch input
        [SerializeField]
        private float[] rollModifiers; //targetangle[i] += modifiers[i] * aircraft roll input
        [SerializeField]
        private Vector3[] centreOfPressureShift;

        [SerializeField]
        private float DisableThreshold;
        [SerializeField]
        private float SpeedThreshold;

        private Vector3[] originalCentresOfLift;

        public float Airspeed;
        public float Alpha;

        bool isFunctional = true;

        private void Awake()
        {
            criticalPart.onPartDetached += Detach;
            criticalPart.onApplyDamage += Damage;
            if (aircraft != null)
                isFunctional = true;

            originalCentresOfLift = new Vector3[effectors.Length];
            for (int i = 0; i < effectors.Length; i++)
            {
                originalCentresOfLift[i] = effectors[i].localPosition;
            }
        }

        private void FixedUpdate()
        {
            if (!isFunctional || aircraft == null || effectors == null)
                return;


            Airspeed = aircraft.speed;
            if (aircraft.speed > SpeedThreshold)
                Alpha = TargetCalc.GetAngleOnAxis(aircraft.rb.transform.forward, aircraft.rb.velocity, aircraft.rb.transform.right);

            ControlInputs inputs = aircraft.GetInputs();

            for (int i = 0; i < effectors.Length; i++)
            {

                float alphaRatio = Mathf.Clamp01(Alpha / maxAlpha[i]);
                float targetAngle = Mathf.Clamp(modifiers[i].Evaluate(alphaRatio), minAngles[i], maxAngles[i]);
                targetAngle += Mathf.Clamp(Alpha * pitchModifiers[i] * inputs.pitch, minAngles[i], maxAngles[i]);
                targetAngle += Mathf.Clamp(Alpha * rollModifiers[i] * inputs.roll, minAngles[i], maxAngles[i]);

                float targetRatio = Mathf.Clamp01(targetAngle / maxAngles[i]);

                Vector3 currentEuler = effectors[i].localEulerAngles;
                float currentX = currentEuler.x;
                if (currentX > 180f)
                    currentX -= 360f;
                float newX = Mathf.MoveTowards(currentX, targetAngle, rotationSpeeds[i] * Time.fixedDeltaTime);

                effectors[i].localPosition = Vector3.Lerp(originalCentresOfLift[i], centreOfPressureShift[i], targetRatio);

                effectors[i].localRotation = Quaternion.Euler(newX, currentEuler.y, currentEuler.z);
            }
        }
        private void Damage(UnitPart.OnApplyDamage dmg)
        {
            if (criticalPart.hitPoints < DisableThreshold)
            {
                Disable();
            }
        }
        private void Detach(UnitPart part)
        {
            Disable();
        }
        private void Disable()
        {
            if (!isFunctional) return;

            isFunctional = false;
            criticalPart.onPartDetached -= Detach;
            criticalPart.onApplyDamage -= Damage;
        }
    }
}
