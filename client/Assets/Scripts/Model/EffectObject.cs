using Assets.ObjectPool;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class EffectObject : MonoBehaviour, IPoolable
    {
        [SerializeField] private ParticleSystem ps;
        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }
        public void OnSpawn()
        {

        }
        public void OnDeSpawn()
        {

        }
        public void Play()
        {
            ps.Play();
        }
        private void OnParticleSystemStopped()
        {
            PoolManager.Instance.ReleaseGameObject(this);
        }
    }
}