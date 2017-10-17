using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class SMG_Projectile : MonoBehaviour
    {
        public float m_MaxLifeTime = 0.5f, m_CurrentLifeTime;
        float damage = 10;

        void Update()
        {
            m_CurrentLifeTime += Time.deltaTime * 3;
            if (m_CurrentLifeTime >= m_MaxLifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision coll)
        {
            Destroy(gameObject);
            if (coll.gameObject.name.Contains("CompleteBaja"))
            {
                Rigidbody targetRigidbody = coll.gameObject.GetComponent<Rigidbody>();
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
                targetHealth.TakeDamage(damage);
            }
        }
    }
}