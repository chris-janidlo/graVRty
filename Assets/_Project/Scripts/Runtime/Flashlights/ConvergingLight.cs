using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;
using VLB;

namespace GraVRty.Flashlights
{
    public class ConvergingLight : MonoBehaviour
    {
        public Cone Dimensions;
        [SerializeField] Vector2Int m_RenderTextureDimensions = new(512, 512);
        [Min(1)]
        [SerializeField] float m_LightLengthMultiplier = 10;

        [Foldout("References"), SerializeField] Light m_SpotLight;
        [Foldout("References"), SerializeField] Camera m_Camera;
        [Foldout("References"), SerializeField] Shader m_ConeTestShader;
        [Foldout("References"), SerializeField] VolumetricLightBeam m_Beam;

        void Start()
        {
            RenderTexture rt = new(m_RenderTextureDimensions.x, m_RenderTextureDimensions.y, 16);
            rt.Create();

            m_Camera.targetTexture = rt;
            // only render objects whose normal shader's RenderType value matches the value of the cone test shader's RenderType (which is set to Opaque)
            m_Camera.SetReplacementShader(m_ConeTestShader, "RenderType");
            m_SpotLight.cookie = rt;

            m_Camera.aspect = 1;
            m_Camera.nearClipPlane = 0.01f;

            ApplyValues();
        }

        void Update ()
        {
            ApplyValues();
        }

        [ContextMenu("Apply values")]
        void ApplyValues ()
        {
            Vector3 localTipPosition = Vector3.forward * Dimensions.Height;
            Vector3 worldTipPosition = transform.TransformPoint(localTipPosition);

            Shader.SetGlobalFloat("ConeTest_Height", Dimensions.Height);
            Shader.SetGlobalFloat("ConeTest_BaseRadius", Dimensions.Radius);
            Shader.SetGlobalVector("ConeTest_TipPosition", worldTipPosition);
            Shader.SetGlobalVector("ConeTest_AxisDirection", -transform.forward);

            m_Camera.farClipPlane = Dimensions.Height;
            m_SpotLight.range = Dimensions.Height * m_LightLengthMultiplier;

            m_Beam.fallOffEnd = Dimensions.Height;
            m_Beam.transform.localPosition = localTipPosition;
            m_Beam.spotAngle = Dimensions.Angle;

            m_Beam.UpdateAfterManualPropertyChange();

            m_Camera.fieldOfView = m_SpotLight.spotAngle;
        }
    }
}
