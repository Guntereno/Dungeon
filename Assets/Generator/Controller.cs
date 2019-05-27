using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generator
{
    public class Controller : MonoBehaviour
    {
        [SerializeField]
        private View ViewPrefab = null;

        private Transform m_rootNode;
        private Mesh m_mesh;
        private View m_view;

        // Start is called before the first frame update
        void Start()
        {
            m_mesh = new Mesh();
            m_view = GameObject.Instantiate(ViewPrefab);
            OnMeshUpdated(m_mesh);
        }

        private void OnMeshUpdated(Mesh mesh)
        {
            m_view.MeshUpdated(mesh);
        }
    }
}