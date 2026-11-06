using UnityEngine;

namespace DesertRush
{
    public struct RunnerInput
    {
        public int Lane;
        public bool Jump, Slide, Pause;
    }

    public interface IRunnerInput
    {
        RunnerInput Read();
    }

    public sealed class DesktopInput : IRunnerInput
    {
        private int heldAxis;

        public RunnerInput Read()
        {
            float axis = Input.GetAxisRaw("Horizontal");
            int direction = axis > .55f ? 1 : axis < -.55f ? -1 : 0;
            var result = new RunnerInput
            {
                Lane = direction != heldAxis ? direction : 0,
                Jump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)
                    || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.JoystickButton0),
                Slide = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)
                    || Input.GetKeyDown(KeyCode.JoystickButton1),
                Pause = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7)
            };
            heldAxis = direction;
            return result;
        }
    }
}
