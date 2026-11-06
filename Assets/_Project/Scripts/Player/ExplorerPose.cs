using UnityEngine;

namespace DesertRush
{
    public sealed class ExplorerPose : MonoBehaviour
    {
        public Transform hips, chest, head, scarf;
        public Transform leftLeg, rightLeg, leftShin, rightShin, leftArm, rightArm, leftForearm, rightForearm;
        public Renderer[] outfitPanels;
        private float gait, fall, landing;

        public void Dress(Material color)
        {
            foreach (var panel in outfitPanels) panel.sharedMaterial = color;
        }

        public void ResetPose() => gait = fall = landing = 0f;

        public void Pose(RunnerMotor motor, float speed, float time, float dt, bool dead, bool gliding)
        {
            bool run = speed > .1f;
            gait += dt * (5f + speed * .35f);
            fall = dead ? Mathf.Min(1f, fall + dt * 2.4f) : 0f;
            landing = motor.Landed && run ? 1f : Mathf.MoveTowards(landing, 0f, dt * 6f);
            float stride = Mathf.Sin(gait);
            float hipY = .94f + (run ? Mathf.Abs(stride) * .047f - landing * .10f : Mathf.Sin(time * 2f) * .013f);
            float tilt = Mathf.Clamp(-motor.LateralVelocity * 2f, -20f, 20f);
            float pitch = run ? 9f : 0f;
            float l = run ? stride * 44f : -6f;
            float r = run ? -stride * 44f : 7f;
            float kl = run ? Mathf.Max(0f, -stride) * 66f + 6f : 10f;
            float kr = run ? Mathf.Max(0f, stride) * 66f + 6f : 9f;
            float al = run ? -stride * 33f - 15f : 3f;
            float ar = run ? stride * 33f - 15f : -8f;
            if (run && !motor.Grounded) { l = -48; r = 26; kl = 53; kr = 65; al = 27; ar = -65; }
            if (run && motor.Sliding)
            {
                hipY = .30f; pitch = -65; l = -68; r = -64; kl = 31; kr = 18; al = 49; ar = 52;
            }
            if (gliding)
            {
                hipY += 1.04f + Mathf.Sin(time * 8f) * .07f;
                pitch = 17; l = -36; r = -22; kl = 44; kr = 48; al = 24; ar = 26;
            }
            float eased = fall * fall;
            hipY = Mathf.Lerp(hipY, .19f, eased);
            pitch = Mathf.Lerp(pitch, 87f, eased);
            tilt = Mathf.Lerp(tilt, -15f, eased);
            float blend = 1f - Mathf.Exp(-19f * dt);
            hips.localPosition = Vector3.Lerp(hips.localPosition, new Vector3(0, hipY, 0), blend);
            Bend(hips, pitch, 0, tilt, blend);
            Bend(chest, 0, run ? stride * 10f : 0f, 0, blend);
            Bend(head, -pitch * .29f, tilt * .48f, -tilt * .5f, blend);
            Bend(leftLeg, l - pitch, 0, 2, blend);
            Bend(rightLeg, r - pitch, 0, -2, blend);
            Bend(leftShin, kl, 0, 0, blend);
            Bend(rightShin, kr, 0, 0, blend);
            Bend(leftArm, al, 0, gliding ? -57 : 9, blend);
            Bend(rightArm, ar, 0, gliding ? 57 : -9, blend);
            Bend(leftForearm, run ? -65 : -14, 0, 0, blend);
            Bend(rightForearm, run ? -65 : -14, 0, 0, blend);
            Bend(scarf, Mathf.Sin(time * 12f) * 10f + speed, Mathf.Sin(time * 9f) * 12f, 0, blend);
        }

        private static void Bend(Transform part, float x, float y, float z, float blend) =>
            part.localRotation = Quaternion.Slerp(part.localRotation, Quaternion.Euler(x, y, z), blend);
    }
}
