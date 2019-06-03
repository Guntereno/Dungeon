using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generator
{
    public enum DistributionAlgorithm
    {
        UniformRandom,
        Mitchell
    }

    [ExecuteInEditMode]
    public class Controller : MonoBehaviour
    {
        [Header("Scene")]

        public Camera Camera;
        public Material GroundMaterial;

        [Header("Distribution")]

        public Vector2 Bounds = new Vector2(1.0f, 1.0f);
        public int Seed = 0;
        public DistributionAlgorithm DistributionAlgorithm;
        public int SiteCount = 10;
        public int CandidateCount = 10;

        // Voronoi Generation

        private Vector2[] m_sites;
        private GK.VoronoiDiagram m_voronoiDiagram;
        private Vector2 m_bounds;

        // Scene Heirarchy Generation

        [HideInInspector]
        private Transform m_rootNode;

        // MonoBehaviour Methods

        void Start()
        {}

        void Update()
        {
            if(m_voronoiDiagram != null)
            {
                RenderDebugBounds(m_bounds);
                RenderDebugVoronoi(m_voronoiDiagram);
            }
        }

        // Public Interface

        public void Generate()
        {
            var random = new System.Random(Seed);

            switch (DistributionAlgorithm)
            {
                case DistributionAlgorithm.UniformRandom:
                    m_sites = GenerateControlPointsUniformRandom(random, Bounds, SiteCount);
                    break;

                case DistributionAlgorithm.Mitchell:
                    m_sites = GenerateControlPointsMitchell(random, Bounds, SiteCount, CandidateCount);
                    break;

                default:
                    throw new System.Exception("Unhandled DistributionAlgorithm!");
            }

            m_voronoiDiagram = GenerateVoronoi(m_sites);
            m_bounds = Bounds;

            GenerateScene(m_voronoiDiagram);

            if(Camera != null)
            {
                // Position camera so it can see the whole width of the shape
                // HACK
                var center = new Vector3(Bounds.x * 0.5f, 0.0f, Bounds.y * 0.5f);
                float fov = (Camera.fieldOfView * Camera.aspect);
                var fovVRad = Camera.fieldOfView * Mathf.Deg2Rad;
                var fovHRad = 2.0f * Mathf.Atan(Mathf.Tan(fovVRad / 2.0f) * Camera.aspect);

                float widest = Mathf.Max(center.x, center.z);
                float distance = Mathf.Tan(fovHRad * 0.5f) * widest;

                var dir = new Vector3(1.0f, 0.0f, 1.0f);
                dir.Normalize();
                Vector3 pos = dir * distance;
                pos.y = widest;
                Camera.transform.position = pos;
                Camera.transform.LookAt(center);
            }
        }

        // Private Methods

        private static Vector2 GetUniformRandomPoint(System.Random random, Vector2 bounds)
        {
            return new Vector2(
                (float)random.NextDouble() * bounds.x,
                (float)random.NextDouble() * bounds.y
            );
        }

        private static Vector2[] GenerateControlPointsUniformRandom(System.Random random, Vector2 bounds, int siteCount)
        {
            // Generate the control points
            var sites = new Vector2[siteCount];
            for (int i = 0; i < siteCount; ++i)
            {
                sites[i] = GetUniformRandomPoint(random, bounds);
            }
            return sites;
        }

        private static Vector2[] GenerateControlPointsMitchell(System.Random random, Vector2 bounds, int siteCount, int numCandidates)
        {
            // TODO: Optimise with a quadtree
            var sites = new Vector2[siteCount];

            sites[0] = GetUniformRandomPoint(random, bounds);

            for (int siteIndex = 1; siteIndex < siteCount; ++siteIndex)
            {
                Vector2 bestCandidate = Vector2.positiveInfinity;
                float bestDistance = float.MinValue;
                for (int candidateIndex = 0; candidateIndex < numCandidates; ++candidateIndex)
                {
                    Vector2 candidate = GetUniformRandomPoint(random, bounds);
                    float minDistSq = float.MaxValue;
                    for (int checkSiteIndex = 0; checkSiteIndex < siteIndex; ++checkSiteIndex)
                    {
                        float distSq = (sites[checkSiteIndex] - candidate).SqrMagnitude();
                        if (distSq < minDistSq)
                        {
                            minDistSq = distSq;
                        }
                    }
                    if (minDistSq > bestDistance)
                    {
                        bestCandidate = candidate;
                        bestDistance = minDistSq;
                    }
                }
                sites[siteIndex] = bestCandidate;
            }
            return sites;
        }

        private static GK.VoronoiDiagram GenerateVoronoi(Vector2[] sites)
        {
            var calculator = new GK.VoronoiCalculator();
            GK.VoronoiDiagram voronoiDiagram = calculator.CalculateDiagram(sites);

            return voronoiDiagram;
        }

        private static void RenderDebugBounds(Vector2 bounds)
        {
            var points = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0.0f, 0.0f, bounds.y),
                new Vector3(bounds.x, 0.0f, bounds.y),
                new Vector3(bounds.x, 0.0f, 0.0f)
            };
            Debug.DrawLine(points[0], points[1], Color.cyan);
            Debug.DrawLine(points[1], points[2], Color.cyan);
            Debug.DrawLine(points[2], points[3], Color.cyan);
            Debug.DrawLine(points[3], points[0], Color.cyan);
        }

        private static void RenderDebugVoronoi(GK.VoronoiDiagram diagram)
        {
            if (diagram != null)
            {
                // Draw the edges
                if (diagram.Edges != null)
                {
                    int edgeCount = diagram.Edges.Count;
                    for (int i = 0; i < edgeCount; ++i)
                    {
                        GK.VoronoiDiagram.Edge edge = diagram.Edges[i];
                        Vector3 v0, v1;
                        switch (edge.Type)
                        {
                            // a "segment" is a regular line segment
                            case GK.VoronoiDiagram.EdgeType.Segment:
                                {
                                    Vector2 uv0 = diagram.Vertices[edge.Vert0];
                                    Vector2 uv1 = diagram.Vertices[edge.Vert1];
                                    v0 = new Vector3(uv0.x, 0.0f, uv0.y);
                                    v1 = new Vector3(uv1.x, 0.0f, uv1.y);

                                    Debug.DrawLine(v0, v1, Color.green);
                                    break;
                                }

                            // A "ray" is a voronoi edge starting at a given vertex and extending infinitely
                            // in one direction
                            case GK.VoronoiDiagram.EdgeType.RayCW:
                            case GK.VoronoiDiagram.EdgeType.RayCCW:
                                {
                                    Vector2 posUv = diagram.Vertices[edge.Vert0];

                                    Vector3 pos = new Vector3(posUv.x, 0.0f, posUv.y);
                                    Vector3 dir = new Vector3(edge.Direction.x, 0.0f, edge.Direction.y);

                                    Debug.DrawRay(pos, dir, Color.magenta);
                                    break;
                                }

                            // A "line" is an infinite line in both directions (only valid for Voronoi diagrams
                            // with 2 vertices or ones with all collinear points)
                            case GK.VoronoiDiagram.EdgeType.Line:
                                {
                                    Vector2 posUv = diagram.Vertices[edge.Vert0];

                                    Vector3 pos = new Vector3(posUv.x, 0.0f, posUv.y);
                                    Vector3 dir = new Vector3(edge.Direction.x, 0.0f, edge.Direction.y);
                                    Debug.DrawRay(pos, dir, Color.yellow);
                                    Debug.DrawRay(pos, -dir, Color.yellow);
                                    break;
                                }


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
            if (m_rootNode != null)
            {
                GameObject.DestroyImmediate(m_rootNode.gameObject);
            }
            m_rootNode = new GameObject("GeneratorRoot").transform;

            int siteCount = m_voronoiDiagram.Sites.Count;

            for (int siteIndex = 0; siteIndex < siteCount; ++siteIndex)
            {
                GenerateSite(diagram, siteIndex);
            }
        }

        private void GenerateSite(GK.VoronoiDiagram diagram, int siteIndex)
        {
            Mesh mesh = GenerateSiteMesh(diagram, siteIndex);
            if (mesh != null)
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
                        //Debug.DrawLine(loop[segmentIndex], loop[nextIndex], Color.cyan, 4.0f);
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