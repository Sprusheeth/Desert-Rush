using UnityEngine;

namespace DesertRush
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RunnerMotor : MonoBehaviour
    {
        public const float LaneWidth = 2.65f;
        public const float NormalHeight = 1.88f;
        public const float SlideHeight = .76f;
        public float LateralVelocity { get; private set; }
        public float VerticalVelocity { get; private set; }
        public bool Sliding => slideTime > 0f;
        public bool Grounded { get; private set; }
        public bool Jumped { get; private set; }
        public bool Landed { get; private set; }
        public bool StartedSlide { get; private set; }

        private CharacterController body;
        private int lane;
        private float jumpBuffer, slideBuffer, slideTime, laneVelocity;

        private void Awake() => body = GetComponent<CharacterController>();

        public void ResetMotor()
        {
            body.enabled = false;
            transform.position = Vector3.zero;
            body.enabled = true;
            lane = 0;
            jumpBuffer = slideBuffer = slideTime = laneVelocity = 0f;
            LateralVelocity = VerticalVelocity = 0f;
            Grounded = true;
            SetHeight(NormalHeight);
        }

        public void Tick(RunnerInput input, float speed, float dt)
        {
            Jumped = Landed = StartedSlide = false;
            lane = Mathf.Clamp(lane + input.Lane, -1, 1);
            jumpBuffer = input.Jump ? .17f : Mathf.Max(0f, jumpBuffer - dt);
            slideBuffer = input.Slide ? .17f : Mathf.Max(0f, slideBuffer - dt);
            slideTime = Mathf.Max(0f, slideTime - dt);

            if (Grounded && jumpBuffer > 0f && !Sliding)
            {
                VerticalVelocity = 9.5f;
                jumpBuffer = 0f;
                Jumped = true;
            }
            else if (Grounded && slideBuffer > 0f && !Sliding)
            {
                slideTime = .87f;
                slideBuffer = 0f;
                StartedSlide = true;
            }

            SetHeight(Sliding ? SlideHeight : NormalHeight);
            Vector3 position = transform.position;
            float targetX = Mathf.SmoothDamp(position.x, lane * LaneWidth, ref laneVelocity,
                .09f, 23f, dt);
            if (Grounded && !Jumped) VerticalVelocity = -2f;
            else VerticalVelocity -= 24f * dt;
            var flags = body.Move(new Vector3(targetX - position.x, VerticalVelocity * dt, speed * dt));
            bool onGround = (flags & CollisionFlags.Below) != 0;
            Landed = onGround && !Grounded;
            Grounded = onGround;
            LateralVelocity = (transform.position.x - position.x) / dt;
        }

        public void Rebase(float offset)
        {
            body.enabled = false;
            transform.position -= Vector3.forward * offset;
            body.enabled = true;
        }

        private void SetHeight(float height)
        {
            body.height = height;
            body.center = Vector3.up * (height * .5f);
        }
    }
}
