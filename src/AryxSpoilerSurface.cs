using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aryx_NavalInterceptor1
{
    public class AryxSpoilerSurface : MonoBehaviour
    {
        [SerializeField]
        private Aircraft aircraft;
        [SerializeField]
        private SwingWingController swingController;
        [SerializeField]
        private AeroPart part;
        [SerializeField]
        private Transform surface;


        [SerializeField]
        private float rotationSpeed; //max angle change per update
        [SerializeField]
        private float maxAngle;
        [SerializeField]
        private float minAngle;
        [SerializeField]
        private float rollResponse;
        [SerializeField]
        private bool openWithAirbrake;
        [SerializeField]
        private float wingSweepDisableThreshold;
        [SerializeField]
        private float disableThreshold;

        bool isFunctional = true;
        float maxAllowedAngle;
        ControlInputs inputs;

        private void Awake()
        {
            part.onPartDetached += Detach;
            part.onApplyDamage += Damage;
            if (aircraft != null)
                isFunctional = true;

            if (surface == null)
                surface = transform;
        }
        private void Start()
        {
            inputs = aircraft.GetInputs();
        }
        private void FixedUpdate()
        {
            if (!isFunctional || aircraft == null || surface == null)
                return;

            maxAllowedAngle = maxAngle;
            if (swingController != null && swingController.GetSwingPosition() > wingSweepDisableThreshold)
                maxAllowedAngle = 0;


            Vector3 currentEuler = surface.localEulerAngles;
            float currentX = currentEuler.x;
            if (currentX > 180f)
                currentX -= 360f;
            float newX = Mathf.MoveTowards(currentX, Mathf.Clamp(inputs.roll * rollResponse + (openWithAirbrake && inputs.throttle == 0 ? maxAllowedAngle : 0), minAngle, maxAllowedAngle), rotationSpeed * Time.fixedDeltaTime);
            surface.localRotation = Quaternion.Euler(newX, currentEuler.y, currentEuler.z);

        }


        private void Damage(UnitPart.OnApplyDamage dmg)
        {
            if (part.hitPoints < disableThreshold)
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
            part.onPartDetached -= Detach;
            part.onApplyDamage -= Damage;
        }

    }
}
