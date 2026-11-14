using NuclearOption.UIStyleSystem;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Aryx_NavalInterceptor1
{
    public class AryxCountermeasureStatusApp : HUDApp
    {
        [SerializeField]
        private Image counterImage;

        [SerializeField]
        private Text counterName;

        [SerializeField]
        private Text counterAmmo;

        private Aircraft aircraft;
        private AryxAmmoReadoutCache ammoCache;
        private AryxFloatReadoutCache chargeCache;

        public override void Initialize(Aircraft aircraft)
        {
            this.aircraft = aircraft;
            if (aircraft.countermeasureManager.GetActiveCountermeasure() == null)
            {
                base.gameObject.SetActive(value: false);
            }
        }

        public override void RefreshSettings()
        {
            base.RefreshSettings();

            counterName.fontSize = (int)((float)fontSize * fontSizeMultiplier);
            counterAmmo.fontSize = (int)((float)fontSize * fontSizeMultiplier);
            Show(true);
            Refresh();
        }

        public override void Refresh()
        {
            if (aircraft == null)
            {
                return;
            }
            Countermeasure activeCountermeasure = aircraft.countermeasureManager.GetActiveCountermeasure();
            if (activeCountermeasure != null)
            {
                counterImage.enabled = true;
                Color stateColor = ThemeManager.Active.ColorTheme.AllClear;
                if (activeCountermeasure is FlareEjector flareEjector)
                {
                    if (counterName.text != "FLARE")
                    {
                        if (!counterImage.enabled)
                        {
                            counterImage.enabled = true;
                        }
                        counterImage.sprite = flareEjector.displayImage;
                        counterName.text = "FLARE";
                    }
                    int ammo = flareEjector.GetAmmo();
                    counterAmmo.text = ammoCache.Get(ammo, 0, false);
                    if ((float)ammo == 0f)
                    {
                        stateColor = ThemeManager.Active.ColorTheme.Alert;
                    }
                }
                else
                {
                    if (counterName.text != activeCountermeasure.displayName)
                    {
                        counterImage.sprite = activeCountermeasure.displayImage;
                        counterName.text = activeCountermeasure.displayName;
                    }
                    float charge = aircraft.GetPowerSupply().GetCharge();
                    counterAmmo.text = chargeCache.Get(100f * charge, "{0:F0}%");
                    if (charge < 0.5f)
                    {
                        stateColor = ThemeManager.Active.ColorTheme.Alert;
                    }
                }
                counterImage.color = stateColor;
                counterName.color = stateColor;
                counterAmmo.color = stateColor;
            }
            else
            {
                counterImage.enabled = false;
                counterName.text = "";
                counterAmmo.text = "";
            }
        }

        public void Show(bool arg)
        {
            counterImage.enabled = arg;
            counterName.enabled = arg;
            counterAmmo.enabled = arg;
        }
    }

}
