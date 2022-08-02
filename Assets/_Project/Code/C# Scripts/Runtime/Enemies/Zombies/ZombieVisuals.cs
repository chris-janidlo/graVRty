using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Enemies.Zombies
{
    public class ZombieVisuals : MonoBehaviour
    {
        [SerializeField] float m_MaxAcceleration;
        [SerializeField] Vector2 m_SpeedBounds;
        [SerializeField] Renderer m_Renderer;

        Vector2 velocity;

        void Update ()
        {
            Vector2 acceleration = Random.insideUnitCircle * m_MaxAcceleration;
            velocity += acceleration * Time.deltaTime;

            if (velocity.sqrMagnitude < m_SpeedBounds.x * m_SpeedBounds.x)
            {
                velocity = velocity.normalized * m_SpeedBounds.x;
            }
            else if (velocity.sqrMagnitude > m_SpeedBounds.y * m_SpeedBounds.y)
            {
                velocity = velocity.normalized * m_SpeedBounds.y;
            }

            Vector2 offset = m_Renderer.material.mainTextureOffset + velocity * Time.deltaTime;
            offset = new Vector2(Mathf.Repeat(offset.x, 1), Mathf.Repeat(offset.y, 1));
            m_Renderer.material.mainTextureOffset = offset;
        }
    }
}
