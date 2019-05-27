using UnityEngine;

namespace Assets.Generator
{
    public class Controller : MonoBehaviour
    {
        public View ViewPrefab = null;

        private Mesh m_mesh = null;
        private View m_view = null;

        private Voronoi m_voronoi = null;

        // Start is called before the first frame update
        void Start()
        {
            m_view = GameObject.Instantiate<View>(ViewPrefab);
            Generate();
        }

        void Update()
        {
            if(m_voronoi != null)
            {
                m_voronoi.DebugRender();
            }
        }

        public void Generate()
        {
            Voronoi voronoi = new Voronoi(8, 22);

            m_mesh = new Mesh();

            m_mesh.vertices = new Vector3[] {
                new Vector3(  0.0f, 0.0f,  0.0f ),
                new Vector3(  1.0f, 0.0f,  0.0f ),
                new Vector3(  1.0f, 0.0f,  1.0f ),
                new Vector3(  0.0f, 0.0f,  1.0f )
            };
            m_mesh.uv = new Vector2[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };
            m_mesh.normals = new Vector3[]
            {
                new Vector3( 0.0f, 1.0f, 0.0f ),
                new Vector3( 0.0f, 1.0f, 0.0f ),
                new Vector3( 0.0f, 1.0f, 0.0f ),
                new Vector3( 0.0f, 1.0f, 0.0f )
            };
            m_mesh.triangles = new int[] {
                0, 3, 2,
                0, 2, 1
            };

            OnMeshUpdated();

            m_voronoi = voronoi;
        }

        void OnMeshUpdated()
        {
            m_view.MeshUpdated(m_mesh);
        }
    }
}