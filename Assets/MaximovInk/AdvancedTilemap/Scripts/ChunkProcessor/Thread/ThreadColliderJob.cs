using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class ThreadCollisionJob : IChunkProcessorJob
    {
        public string Name { get; }
        public bool IsRunning { get; private set; }
        public bool IsValid { get; }

        List<List<Vector2>> paths = new List<List<Vector2>>();

        private const int CHUNK_SIZE = AChunk.CHUNK_SIZE;

        private AChunkProcessorData _input;

        public void Generate(AChunkProcessorData input)
        {
            IsRunning = true;

            _input = input;

            if (_input.ChunkData.IsEmpty)
            {
                _input.Chunk.Collider.pathCount = 0;

                return;
            }

            var thread = new Thread(ThreadGenerate);
            thread.Start();
        }

        private void ThreadGenerate()
        {
            var collider = _input.Chunk.Collider;

            if (collider == null)
            {
                IsRunning = false;
                return;
            }
            
            List<ColliderSegment> segments = GetSegments(_input.ChunkData);
            paths = FindPath(segments);

            var scale = (Vector2)_input.Chunk.Layer.Tileset.TileSize
                / (Vector2)_input.Chunk.Layer.Tileset.PixelPerUnit;

            paths = ScaleToTiles(paths, scale);

            IsRunning = false;
        }

        public void WaitComplete()
        {
            while (IsRunning) { }

            var collider = _input.Chunk.Collider;

            collider.pathCount = paths.Count;
            for (int i = 0; i < paths.Count; i++)
            {
                collider.SetPath(i, paths[i].ToArray());
            }
        }

        private List<List<Vector2>> ScaleToTiles(List<List<Vector2>> input, Vector2 scale)
        {
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Count; j++)
                {
                    var p = input[i][j];


                    input[i][j] = new Vector2(p.x * scale.x, p.y * scale.y);
                }
            }

            return input;

        }

        private List<List<Vector2>> FindPath(List<ColliderSegment> segments)
        {
            List<List<Vector2>> paths = new List<List<Vector2>>();

            while (segments.Count > 0)
            {
                Vector2 currentPoint = segments[0].point2;
                List<Vector2> currentPath = new List<Vector2> { segments[0].point1, segments[0].point2 };
                segments.Remove(segments[0]);

                bool pathComplete = false;
                while (!pathComplete)
                {
                    pathComplete = true;
                    for (int s = 0; s < segments.Count; s++)
                    {
                        if (segments[s].point1 == currentPoint)
                        {
                            pathComplete = false;
                            currentPath.Add(segments[s].point2);
                            currentPoint = segments[s].point2;
                            segments.Remove(segments[s]);
                        }
                        else if (segments[s].point2 == currentPoint)
                        {
                            pathComplete = false;
                            currentPath.Add(segments[s].point1);
                            currentPoint = segments[s].point1;
                            segments.Remove(segments[s]);
                        }
                    }
                }
                paths.Add(currentPath);
            }
            return paths;
        }

        private List<ColliderSegment> GetSegments(AChunkData _data)
        {
            List<ColliderSegment> segments = new List<ColliderSegment>();

            for (int i = 0; i < _data.data.Length; i++)
            {
                if (!_data.collision[i])
                    continue;

                int x = i % CHUNK_SIZE;
                int y = i / CHUNK_SIZE;
                //top
                if (y + 1 >= CHUNK_SIZE || !_data.collision[x + (y + 1) * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y + 1), new Vector2(x + 1, y + 1)));
                }
                //bottom
                if (y - 1 < 0 || !_data.collision[x + (y - 1) * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x + 1, y)));
                }
                //right
                if (x + 1 >= CHUNK_SIZE || !_data.collision[x + 1 + y * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x + 1, y), new Vector2(x + 1, y + 1)));
                }
                //left
                if (x - 1 < 0 || !_data.collision[x - 1 + y * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x, y + 1)));
                }
            }

            return segments;
        }

    }
}
