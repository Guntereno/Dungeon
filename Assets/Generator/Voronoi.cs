using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generator
{
    public class Voronoi
    {
        private struct Relationship
        {
            public float Distance;
            public Vector3 MidPoint;
        }

        private struct Region
        {
            public Vector3 Position;
        }

        // Control points in normalised space (0.0 to 1.0).
        private List<Region> Regions = null;
        private Dictionary<KeyValuePair<int, int>, Relationship> Neighbours = null;

        public Voronoi(int numRegions, int seed)
        {
            System.Random rand = new System.Random(seed);

            // Generate the control points
            Regions = new List<Region>(numRegions);
            for (int i = 0; i < numRegions; ++i)
            {
                Vector3 position = new Vector3(
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 0.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f)
                );

                Regions.Add(new Region
                {
                    Position = position
                });
            }

            // Cache the data for each relationship
            Neighbours = new Dictionary<KeyValuePair<int, int>, Relationship>();
            for (int i=0; i < numRegions; ++i)
            {
                for (int j=i + 1; j < numRegions; ++j)
                {
                    var relationship = new Relationship();

                    Vector3 p1 = Regions[i].Position;
                    Vector3 p2 = Regions[j].Position;

                    var offset = p2 - p1;
                    relationship.Distance = offset.magnitude;
                    relationship.MidPoint = p1 + (offset * 0.5f);

                    Neighbours.Add(
                        new KeyValuePair<int, int>(i, j),
                        relationship);
                }
            }
        }

        public void DebugRender()
        {
            if (Regions == null)
                return;

            int regionCount = Regions.Count;
            for (int i = 0; i < regionCount; ++i)
            {
                for (int j = i + 1; j < regionCount; ++j)
                {
                    Vector3 p1 = Regions[i].Position;
                    Vector3 p2 = Regions[j].Position;
                    Debug.DrawLine(p1, p2, Color.magenta);
                }
            }
        }
    }
}
