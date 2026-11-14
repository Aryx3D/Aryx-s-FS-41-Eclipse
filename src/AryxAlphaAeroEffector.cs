using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aryx_NavalInterceptor1
{
    //AlphaEffector's dumb brother that adjusts lift instead.
    public class AryxAlphaAeroEffector : MonoBehaviour
    {
        [SerializeField]
        private Aircraft aircraft;
        [SerializeField]
        private AeroPart criticalPart;

        [SerializeField]
        private AeroPart[] aeroParts;
        [SerializeField]
        private float[] areaChanges;
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
        private float[] originalAreas;

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
            originalAreas = new float[aeroParts.Length];
            for (int i = 0; i < aeroParts.Length; i++)
            {
                originalAreas[i] = aeroParts[i].WingArea;
            }
        }
        private void FixedUpdate()
        {
            if (!isFunctional || aircraft == null || aeroParts == null)
                return;

            Airspeed = aircraft.speed;
            Alpha = Airspeed > SpeedThreshold ? TargetCalc.GetAngleOnAxis(aircraft.rb.transform.forward, aircraft.rb.velocity, aircraft.rb.transform.right) : 0f;

            for (int i = 0; i < aeroParts.Length; i++)
            {
                if (aeroParts[i] == null)
                    continue;

                float targetAngle = Mathf.Clamp((Alpha * modifiers[i]) + (inputs.pitch * pitchModifiers[i]) + (inputs.roll * rollModifiers[i]), minAngles[i], maxAngles[i]);
                float deflection;

                if (targetAngle >= 0f)
                    deflection = maxAngles[i] > 0f ? targetAngle / maxAngles[i] : 0f;
                else
                    deflection = minAngles[i] < 0f ? targetAngle / minAngles[i] : 0f;

                deflection = Mathf.Clamp01(deflection);

                aeroParts[i].SetWingArea(originalAreas[i] + areaChanges[i] * deflection);
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
            if (!isFunctional)
                return;

            isFunctional = false;

            for (int i = 0; i < aeroParts.Length; i++)
            {
                if (aeroParts[i] != null)
                    aeroParts[i].SetWingArea(originalAreas[i]);
            }

            criticalPart.onPartDetached -= Detach;
            criticalPart.onApplyDamage -= Damage;
        }
    }
}
