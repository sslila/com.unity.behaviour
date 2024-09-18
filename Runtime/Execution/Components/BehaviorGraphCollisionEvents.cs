using System;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// This class is used to dispatch collision events to the behavior graph.
    /// </summary>
    public class BehaviorGraphCollisionEvents : MonoBehaviour
    {
        /// <summary>
        /// Register to this event to receive OnCollisionEnter information.
        /// </summary>
        public event Action<GameObject> OnCollisionEnterEvent;
        /// <summary>
        /// Register to this event to receive OnCollisionExit information.
        /// </summary>
        public event Action<GameObject> OnCollisionExitEvent;
        /// <summary>
        /// Register to this event to receive OnCollisionStay information.
        /// </summary>
        public event Action<GameObject> OnCollisionStayEvent;
        /// <summary>
        /// Register to this event to receive OnTriggerEnter information.
        /// </summary>
        public event Action<GameObject> OnTriggerEnterEvent;
        /// <summary>
        /// Register to this event to receive OnTriggerExit information.
        /// </summary>
        public event Action<GameObject> OnTriggerExitEvent;
        /// <summary>
        /// Register to this event to receive OnTriggerStay information.
        /// </summary>
        public event Action<GameObject> OnTriggerStayEvent;

#if UNITY_PHYSICS_2D
        /// <summary>
        /// Register to this event to receive OnCollisionEnter2D information.
        /// </summary>
        public event Action<GameObject> OnCollisionEnterEvent2D;
        /// <summary>
        /// Register to this event to receive OnCollisionExit information.
        /// </summary>
        public event Action<GameObject> OnCollisionExitEvent2D;
        /// <summary>
        /// Register to this event to receive OnCollisionStay information.
        /// </summary>
        public event Action<GameObject> OnCollisionStayEvent2D;
        /// <summary>
        /// Register to this event to receive OnTriggerEnter information.
        /// </summary>
        public event Action<GameObject> OnTriggerEnterEvent2D;
        /// <summary>
        /// Register to this event to receive OnTriggerExit information.
        /// </summary>
        public event Action<GameObject> OnTriggerExitEvent2D;
        /// <summary>
        /// Register to this event to receive OnTriggerStay information.
        /// </summary>
        public event Action<GameObject> OnTriggerStayEvent2D;
#endif

        // 3D collision event messages.
        private void OnCollisionEnter(Collision other) => OnCollisionEnterEvent?.Invoke(other.gameObject);
        private void OnCollisionExit(Collision other) => OnCollisionExitEvent?.Invoke(other.gameObject);
        private void OnCollisionStay(Collision other) => OnCollisionStayEvent?.Invoke(other.gameObject);
        private void OnTriggerEnter(Collider other) => OnTriggerEnterEvent?.Invoke(other.gameObject);
        private void OnTriggerExit(Collider other) => OnTriggerExitEvent?.Invoke(other.gameObject);
        private void OnTriggerStay(Collider other) => OnTriggerStayEvent?.Invoke(other.gameObject);

#if UNITY_PHYSICS_2D
        // 2D collision event messages.
        private void OnCollisionEnter2D(Collision2D other) => OnCollisionEnterEvent2D?.Invoke(other.gameObject);
        private void OnCollisionExit2D(Collision2D other) => OnCollisionExitEvent2D?.Invoke(other.gameObject);
        private void OnCollisionStay2D(Collision2D other) => OnCollisionStayEvent2D?.Invoke(other.gameObject);
        private void OnTriggerEnter2D(Collider2D other) => OnTriggerEnterEvent2D?.Invoke(other.gameObject);
        private void OnTriggerExit2D(Collider2D other) => OnTriggerExitEvent2D?.Invoke(other.gameObject);
        private void OnTriggerStay2D(Collider2D other) => OnTriggerStayEvent2D?.Invoke(other.gameObject);
#endif
    }
}