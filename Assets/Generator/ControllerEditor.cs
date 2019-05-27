using UnityEngine;

namespace Assets.Generator
{
    public class ControllerEditor : MonoBehaviour
    {
        [SerializeField]
        private Controller m_controller = null;

        void OnGUI()
        {
            if (m_controller == null)
                return;

            bool clicked;

            {

                // Make a group on the center of the screen
                GUI.BeginGroup(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 100, 100));
                // All rectangles are now adjusted to the group. (0,0) is the topleft corner of the group.

                GUI.Box(new Rect(0, 0, 100, 100), "Voronoi");
                clicked = GUI.Button(new Rect(10, 40, 80, 30), "Generate");

                // End the group we started above. This is very important to remember!
                GUI.EndGroup();

            }

            if(clicked)
            {
                m_controller.Generate();
            }
        }
    }
}
