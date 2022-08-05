using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Engine
{
    public static class ColorExtensions
    {
        public static Color GetColorFromString(string str) {
            Color color = Color.black;
            if (str == null) return color;
            ColorUtility.TryParseHtmlString("#" + (str.GetHashCode() & 0x00FFFFFF).ToString("X6"), out color);
            return color;
        }

        public static Color GetOppositeColor(this Color color) => (color.PercievedBrightness() > 130 ? Color.black : Color.white);

        public static int PercievedBrightness(this Color c) {
            return (int)Mathf.Sqrt(
                c.r * c.r * .299f +
                c.g * c.g * .587f +
                c.b * c.b * .114f);
        }

        public static Color ChangeBrightness(this Color color, float brightness)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            return Color.HSVToRGB(h, s, brightness);
        }
        
        public static Color Lightness(this Color c,  float value)
        {
            var hsl = c.RGBtoHSL();
            hsl.l = value;
            return hsl.HSLtoRGB();
        }

        public static Color Brightness(this Color c,  float value)
        {
            var hsv = c.RGBtoHSV();
            hsv.v = value;
            return hsv.HSVtoRGB();
        }
    }
    
    public class ColorHSL
    {
        private float _h;
        private float _s;
        private float _l;

        public ColorHSL(float h = 0, float s = 0, float l = 0)
        {
            _h = h;
            _s = s;
            _l = l;
        }

        public float h
        {
            get { return _h; }
            set { _h = value; }
        }

        public float s
        {
            get { return _s; }
            set { _s = value; }
        }

        public float l {
            get { return _l; }
            set { _l = value; }
        }
    }

    public class ColorHSV
    {
        private float _h;
        private float _s;
        private float _v;

        public ColorHSV(float h = 0, float s = 0, float v = 0)
        {
            _h = h;
            _s = s;
            _v = v;
        }

        public float h
        {
            get { return _h; }
            set { _h = value; }
        }

        public float s
        {
            get { return _s; }
            set { _s = value; }
        }

        public float v
        {
            get { return _v; }
            set { _v = value; }
        }
    }

    public static class ColorUtils
    {
        // Adaptado de:
        // http://axonflux.com/handy-rgb-to-hsl-and-rgb-to-hsv-color-model-c

        public static ColorHSL RGBtoHSL(this Color color)
        {
            float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
            float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);

            float h;
            float s;
            float l = (max + min) / 2f;

            if (max == min)
            {
                h = s = 0;
            }
            else
            {
                float d = max - min;
                s = l > .5f ? d / (2f - max - min) : d / (max + min);

                if (max == color.r)
                {
                    h = (color.g - color.b) / d + (color.g < color.b ? 6f : 0);
                }
                else if (max == color.g)
                {
                    h = (color.b - color.r) / d + 2f;
                }
                else
                {
                    h = (color.r - color.g) / d + 4f;
                }
                h /= 6;
            }

            return new ColorHSL(h, s, l);
        }

        public static Color HSLtoRGB(this ColorHSL color)
        {

            float r;
            float g;
            float b;

            if (color.s == 0)
            {
                r = g = b = color.l;
            }
            else
            {
                float q = color.l < .5f ? color.l * (1f + color.s) : color.l + color.s - color.l * color.s;
                float p = 2f * color.l - q;

                r = HUEtoRGB(p, q, color.h + 1f / 3f);
                g = HUEtoRGB(p, q, color.h);
                b = HUEtoRGB(p, q, color.h + 1f / 3f);
            }

            return new Color(r, g, b);
        }

        private static float HUEtoRGB(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }

        public static ColorHSV RGBtoHSV(this Color color)
        {
            float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
            float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);

            float h;
            float s;
            float v = max;

            float d = max - min;
            s = max == 0 ? 0 : d / max;

            if (max == min)
            {
                h = 0;
            }
            else
            {
                if (max == color.r)
                {
                    h = (color.g - color.b) / d + (color.g < color.b ? 6f : 0);
                }
                else if (max == color.g)
                {
                    h = (color.b - color.r) / d + 2f;
                }
                else
                {
                    h = (color.r - color.g) / d + 4f;
                }
                h /= 6f;
            }

            return new ColorHSV(h, s, v);
        }

        public static Color HSVtoRGB(this ColorHSV color)
        {
            float r = 0;
            float g = 0;
            float b = 0;

            int i = Mathf.FloorToInt(color.h * 6f);
            float f = color.h * 6f - i;
            float p = color.v * (1f - color.s);
            float q = color.v * (1f - f * color.s);
            float t = color.v * (1f - (1f - f) * color.s);

            switch (i % 6)
            {
                case 0:
                    r = color.v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = color.v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = color.v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = color.v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = color.v;
                    break;
                case 5:
                    r = color.v;
                    g = p;
                    b = q;
                    break;
            }

            return new Color(r, g, b);
        }
    }
}
