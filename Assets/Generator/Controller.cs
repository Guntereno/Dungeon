using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generator
{
    public class Controller : MonoBehaviour
    {
        public Material GroundMaterial;

        // Voronoi Generation

        private const int kRegionCount = 40;
        private Vector2[] m_sites;
        private GK.VoronoiDiagram m_voronoiDiagram;

        // Scene Heirarchy Generation

        private Transform m_rootNode;

        // MonoBehaviour Methods

        void Start()
        {
            Generate();
        }

        void Update()
        {
            RenderDebugVoronoi();
        }

        // Public Interface

        public void Generate()
        {
            m_voronoiDiagram = GenerateVoronoi();
            GenerateScene(m_voronoiDiagram);
        }

        // Private Methods

        private GK.VoronoiDiagram GenerateVoronoi()
        {
            // Generate the control points
            var m_sites = new Vector2[kRegionCount];
            for (int i = 0; i < kRegionCount; ++i)
            {
                m_sites[i] = new Vector2(
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f)
                );
            }

            var calculator = new GK.VoronoiCalculator();
            GK.VoronoiDiagram voronoiDiagram = calculator.CalculateDiagram(m_sites);

            return voronoiDiagram;
        }

        private void RenderDebugVoronoi()
        {
            if (m_voronoiDiagram != null)
            {
                if (m_voronoiDiagram.Edges != null)
                {
                    int edgeCount = m_voronoiDiagram.Edges.Count;
                    for (int i = 0; i < edgeCount; ++i)
                    {
                        GK.VoronoiDiagram.Edge edge = m_voronoiDiagram.Edges[i];
                        Vector3 v0, v1;
                        switch (edge.Type)
                        {
                            case GK.VoronoiDiagram.EdgeType.Segment:
                                {
                                    Vector2 uv0 = m_voronoiDiagram.Vertices[edge.Vert0];
                                    Vector2 uv1 = m_voronoiDiagram.Vertices[edge.Vert1];
                                    v0 = new Vector3(uv0.x, 0.0f, uv0.y);
                                    v1 = new Vector3(uv1.x, 0.0f, uv1.y);

                                    Debug.DrawLine(v0, v1, Color.green);
                                    break;
                                }


                            case GK.VoronoiDiagram.EdgeType.RayCW:
                            case GK.VoronoiDiagram.EdgeType.RayCCW:
                                {
                                    Vector2 posUv= m_voronoiDiagram.Vertices[edge.Vert0];

                                    Vector3 pos = new Vector3(posUv.x, 0.0f, posUv.y);
                                    Vector3 dir = new Vector3(edge.Direction.x, 0.0f, edge.Direction.y);

                                    Debug.DrawRay(pos, dir, Color.magenta);
                                    break;
                                }

                            case GK.VoronoiDiagram.EdgeType.Line:
                                // Not supported
                                break;

                            default:
                                throw new System.Exception("Unupported EdgeType!");
                        }
                    }
                }
            }
        }

        public static Vector3 DiagramToWorld(Vector2 diagram)
        {
            return new Vector3(diagram.x, 0.0f, diagram.y);
        }


        // Generate a loop of vertex positions for the given site of the given diagram.
        // Edges which aren't composed wholy of segments will be rejected.
        private static bool GenerateLoopForSite(GK.VoronoiDiagram diagram, int siteIndex, out Vector3[] loop)
        {
            int siteCount = diagram.Sites.Count;
            int edgeCount = diagram.Edges.Count;

            int firstEdgeIndex = diagram.FirstEdgeBySite[siteIndex];
            int lastEdgeIndex =
                (siteIndex == (siteCount - 1)) ?
                edgeCount - 1 :
                lastEdgeIndex = diagram.FirstEdgeBySite[siteIndex + 1] - 1;

            int numVerts = (lastEdgeIndex - firstEdgeIndex) + 1;
            loop = new Vector3[numVerts];
            int loopIndex = 0;

            for (int edgeIndex = firstEdgeIndex; edgeIndex < lastEdgeIndex; ++edgeIndex)
            {
                GK.VoronoiDiagram.Edge edge = diagram.Edges[edgeIndex];

                Vector3 v0 = Vector3.zero;
                Vector3 v1 = Vector3.zero;

                switch (edge.Type)
                {
                    case GK.VoronoiDiagram.EdgeType.Segment:
                        {
                            if (edgeIndex == firstEdgeIndex)
                            {
                                loop[loopIndex++] = DiagramToWorld(diagram.Vertices[edge.Vert0]);
                            }
                            loop[loopIndex++] = DiagramToWorld(diagram.Vertices[edge.Vert1]);
                            break;
                        }

                    case GK.VoronoiDiagram.EdgeType.RayCW:
                    case GK.VoronoiDiagram.EdgeType.RayCCW:
                    case GK.VoronoiDiagram.EdgeType.Line:
                        // Not supported
                        return false;

                    default:
                        throw new System.Exception("Unupported EdgeType!");
                }
            }

            return true;
        }

        private void GenerateScene(GK.VoronoiDiagram diagram)
        {
            if(m_rootNode != null)
            {
                GameObject.Destroy(m_rootNode.gameObject);
            }
            m_rootNode = new GameObject("GeneratorRoot").transform;

            int siteCount = m_voronoiDiagram.Sites.Count;

            for (int siteIndex=0; siteIndex<siteCount; ++siteIndex)
            {
                GenerateSite(diagram, siteIndex);
            }
        }

        private void GenerateSite(GK.VoronoiDiagram diagram, int siteIndex)
        {
            Mesh mesh = GenerateSiteMesh(diagram, siteIndex);
            if(mesh != null)
            {
                var siteRoot = new GameObject("Site" + siteIndex);
                siteRoot.transform.SetParent(m_rootNode, false);

                var meshRenderer = siteRoot.AddComponent<MeshRenderer>();
                meshRenderer.material = GroundMaterial;

                var meshFilter = siteRoot.AddComponent<MeshFilter>();
                meshFilter.mesh = GenerateSiteMesh(diagram, siteIndex);
            }
        }

        private static Mesh GenerateSiteMesh(GK.VoronoiDiagram diagram, int siteIndex)
        {
            bool validLoop = GenerateLoopForSite(diagram, siteIndex, out Vector3[] loop);
            Vector3 center = DiagramToWorld(diagram.Sites[siteIndex]);
            if (validLoop)
            {
                int vertCount = loop.Length + 1; // loop + center

                var vertices = new Vector3[vertCount];
                var uv = new Vector2[vertCount];
                var normals = new Vector3[vertCount];

                for (int vertIndex = 0; vertIndex < vertCount; ++vertIndex)
                {
                    // UVs
                    // TODO: Calculate correct uvs for each vert
                    uv[vertIndex] = Vector2.zero;

                    // Positions
                    if (vertIndex == 0)
                    {
                        vertices[vertIndex] = center;
                    }
                    else
                    {
                        vertices[vertIndex] = loop[vertIndex - 1];
                    }

                    normals[vertIndex] = new Vector3(0.0f, 1.0f, 0.0f);
                }

                // Tris
                int segmentCount = loop.Length;
                var triangles = new int[segmentCount * 3];

                for (int index = 0; index < loop.Length; ++index)
                {
                    for (int segmentIndex = 0; segmentIndex < loop.Length; ++segmentIndex)
                    {
                        int nextIndex;
                        if (segmentIndex == (loop.Length - 1))
                            nextIndex = 0;
                        else
                            nextIndex = segmentIndex + 1;

                        Vector3 v0 = loop[segmentIndex];
                        Vector3 v1 = loop[nextIndex];

                        triangles[0 + (segmentIndex * 3)] = 0; // Center point
                        triangles[2 + (segmentIndex * 3)] = segmentIndex + 1; // Segment + center
                        triangles[1 + (segmentIndex * 3)] = nextIndex + 1;

                        // Debug Render
                        Debug.DrawLine(loop[segmentIndex], loop[nextIndex], Color.cyan, 4.0f);
                    }
                }

                var mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.normals = normals;
                mesh.triangles = triangles;

                return mesh;
            }

            return null;
        }
    }
}