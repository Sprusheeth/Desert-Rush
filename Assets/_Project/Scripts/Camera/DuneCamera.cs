using UnityEngine;

namespace DesertRush
{
    [RequireComponent(typeof(Camera))]
    public sealed class DuneCamera : MonoBehaviour
    {
        private Camera lens;
        private Vector3 velocity;
        private float shake;

        private void Awake() => lens = GetComponent<Camera>();

        public void Impact() => shake = .32f;

        public void Rebase(float offset) => transform.position -= Vector3.forward * offset;

        public void Track(Vector3 target, float speed, float dt, bool menu, float time)
        {
            Vector3 position = new Vector3(target.x * .48f, 3.35f + target.y * .46f, target.z - 6.5f);
            Vector3 aim = new Vector3(target.x * .31f, 1.65f + target.y * .38f, target.z + 9f);
            if (menu)
            {
                position = new Vector3(4.7f + Mathf.Sin(time * .27f) * .85f, 2.6f, -7.6f);
                aim = new Vector3(-1.6f, 1.6f, 15f);
            }

            transform.position = Vector3.SmoothDamp(transform.position, position, ref velocity, .15f, 90f, dt);
            Vector3 jolt = new Vector3(Mathf.Sin(time * 79f), Mathf.Cos(time * 63f), 0f) * shake;
            transform.rotation = Quaternion.LookRotation(aim - transform.position + jolt, Vector3.up);
            shake = Mathf.MoveTowards(shake, 0f, dt * 1.4f);
            lens.fieldOfView = Mathf.Lerp(lens.fieldOfView, 54f + speed * .38f, 1f - Mathf.Exp(-3f * dt));
        }
    }
}
