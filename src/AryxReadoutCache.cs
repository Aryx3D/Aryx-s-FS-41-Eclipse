using System.Globalization;

namespace Aryx_NavalInterceptor1
{
    // Experiment in optimising
    internal struct AryxAmmoReadoutCache
    {
        private int amount;
        private int capacity;
        private bool magazine;
        private CultureInfo culture;
        private string text;

        public string Get(int amount, int capacity, bool magazine)
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            if (text == null || this.amount != amount || this.magazine != magazine || (magazine && this.capacity != capacity) || !ReferenceEquals(culture, currentCulture) || !currentCulture.IsReadOnly)
            {
                this.amount = amount;
                this.capacity = capacity;
                this.magazine = magazine;
                culture = currentCulture;
                text = magazine ? amount.ToString(currentCulture) + "/" + capacity.ToString(currentCulture) : amount.ToString(currentCulture);
            }
            return text;
        }
    }

    internal struct AryxFloatReadoutCache
    {
        private float value;
        private string format;
        private CultureInfo culture;
        private string text;

        public string Get(float value, string format)
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            if (text == null || System.BitConverter.SingleToInt32Bits(this.value) != System.BitConverter.SingleToInt32Bits(value) || this.format != format || !ReferenceEquals(culture, currentCulture) || !currentCulture.IsReadOnly)
            {
                this.value = value;
                this.format = format;
                culture = currentCulture;
                text = string.Format(currentCulture, format, value);
            }
            return text;
        }
    }
}
