using ThunderNut.WorldGraph.Attributes;
using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {

    public class CollisionHandler : MonoBehaviour {
        private ContactFilter2D contactFilter;
        private ContactPoint2D? groundContact;
        private ContactPoint2D? ceilingContact;
        public ContactPoint2D? wallContact;
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
        
        [InspectorGroup("Properties", true, 12)]
        public Rigidbody2D m_Rigidbody2D;
        [SerializeField] private float maxWalkCos = 0.2f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] public LayerMask enemyMask;

        public bool IsGrounded => groundContact.HasValue;
        public bool IsTouchingWall => wallContact.HasValue;
        public bool IsTouchingCeiling => ceilingContact.HasValue;

        public Vector2 Velocity => m_Rigidbody2D.velocity;

        private void Awake() {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(groundMask);
        }

        private void FixedUpdate() {
            FindContacts();
        }

        private void FindContacts() {
            groundContact = null;
            ceilingContact = null;
            wallContact = null;

            float groundProjection = maxWalkCos;
            float wallProjection = maxWalkCos;
            float ceilingProjection = -maxWalkCos;

            int numberOfContacts = m_Rigidbody2D.GetContacts(contactFilter, contacts);
            for (var i = 0; i < numberOfContacts; i++) {
                var contact = contacts[i];
                float projection = Vector2.Dot(Vector2.up, contact.normal);

                if (projection > groundProjection) {
                    groundContact = contact;
                    groundProjection = projection;
                }
                else if (projection < ceilingProjection) {
                    ceilingContact = contact;
                    ceilingProjection = projection;
                }
                else if (projection <= wallProjection) {
                    wallContact = contact;
                    wallProjection = projection;
                }
            }
        }
    }

}