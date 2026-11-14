using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aryx_NavalInterceptor1
{
    public class AryxAlphaEffector : MonoBehaviour
    {
        [SerializeField]
        private Aircraft aircraft;
        [SerializeField]
        private AeroPart criticalPart;

        [Header("The shit I gotta do to dance around blueprinter sometimes.")]
        [SerializeField]
        private Transform[] rotators; //transforms to rotate
        [SerializeField]
        private float[] rotationSpeeds; //max angle change per update
        [SerializeField]
        private float[] maxAngles;
        [SerializeField]
        private float[] minAngles;
        [SerializeField]
        private float[] modifiers; //targetangle[i] += modifiers[i] * alpha
        [SerializeField]
        private float[] pitchModifiers; //targetangle[i] += modifiers[i] * aircraft pitch input
        [SerializeField]
        private float[] rollModifiers; //targetangle[i] += modifiers[i] * aircraft roll input

        [SerializeField]
        private float DisableThreshold;
        [SerializeField]
        private float SpeedThreshold;

        public float Airspeed;
        public float Alpha;
        private ControlInputs inputs;

        bool isFunctional = true;

        private void Awake()
        {
            criticalPart.onPartDetached += Detach;
            criticalPart.onApplyDamage += Damage;
            if (aircraft != null)
                isFunctional = true;
        }
        private void Start()
        {
            inputs = aircraft.GetInputs();
        }
        private void FixedUpdate()
        {
            if (!isFunctional || aircraft == null || rotators == null)
                return;


            Airspeed = aircraft.speed;
            Alpha = Airspeed > SpeedThreshold ? TargetCalc.GetAngleOnAxis(aircraft.rb.transform.forward, aircraft.rb.velocity, aircraft.rb.transform.right) : 0f;

            for (int i = 0; i < rotators.Length; i++)
            {
                float targetAngle = Mathf.Clamp((Alpha * modifiers[i]) + (inputs.pitch * pitchModifiers[i]) + (inputs.roll * rollModifiers[i]), minAngles[i], maxAngles[i]);
                Vector3 currentEuler = rotators[i].localEulerAngles;
                float currentX = currentEuler.x;
                if (currentX > 180f)
                    currentX -= 360f;
                float newX = Mathf.MoveTowards(currentX, targetAngle, rotationSpeeds[i] * Time.fixedDeltaTime);
                rotators[i].localRotation = Quaternion.Euler(newX, currentEuler.y, currentEuler.z);
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