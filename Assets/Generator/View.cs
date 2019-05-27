using UnityEngine;

namespace Assets.Generator
{
    class View : MonoBehaviour
    {
        private Mesh m_mesh;
        private Transform m_resultsRoot = null;

        public void MeshUpdated(Mesh mesh)
        {
            // If the mesh has changed...
            if(m_mesh != mesh)
            {
                // Rebuild the scene heirarchy
                m_mesh = mesh;
                if(m_resultsRoot != null)
                {
                    GameObject.Destroy(m_resultsRoot);
                }
            }
        }
    }
}
