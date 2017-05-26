using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace SunShafts2
{
    [AddComponentMenu("Image Effects/Rendering/Sun Shafts")]
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class SunShaftsConfig
    {
        public bool m_Enabled = true;
        public float sunShaftIntensity = 0.5f;
        public float sunShaftBlurRadius = 2.5f;
        public Color sunColor = new Color((float) byte.MaxValue / 256f, 15f / 16f, 0.6428571f);
        public Color sunThreshold = new Color(0.15f, 0.15f, 0.15f, 1f);
        public float height = 1f;

        public static void Serialize(string filename, object instance)
        {
            try
            {
                TextWriter textWriter = (TextWriter) new StreamWriter(filename);
                new XmlSerializer(typeof(SunShaftsConfig)).Serialize(textWriter, instance);
                if (textWriter == null)
                    return;
                textWriter.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static SunShaftsConfig Deserialize(string filename)
        {
            TextReader textReader = (TextReader) null;
            try
            {
                textReader = (TextReader) new StreamReader(filename);
                return (SunShaftsConfig) new XmlSerializer(typeof(SunShaftsConfig)).Deserialize(textReader);
            }
            catch (Exception ex)
            {
            }
            if (textReader != null)
                textReader.Close();
            return (SunShaftsConfig) null;
        }
    }
}