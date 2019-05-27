using UnityEngine;

namespace Assets.Generator
{
    public class View : MonoBehaviour
    {
        public MeshFilter MeshFilter = null;

        private Mesh m_mesh;

        public void MeshUpdated(Mesh mesh)
        {
            // If the mesh has changed...
            if(m_mesh != mesh)
            {
                m_mesh = mesh;

                if (MeshFilter != null)
                {
                    MeshFilter.mesh = m_mesh;
                }
            }
        }
    }
}
