
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class ThreadMeshJob : IChunkProcessorJob
    {
        public string Name => "ThreadMeshProcessor";
        public bool IsRunning { get; private set; } = false;
        public bool IsValid => true;

        private readonly object _lock = new();
        private readonly List<Thread> _blockThreads = new();

        private AChunkProcessorData _input;

        private void ThreadGenerate(AChunkProcessorData input)
        {
            var tiles = input.ChunkData.data;

            var tileset = input.ChunkPersistenceData.Layer.Tileset;

            for (int i = 0; i < input.ChunkData.ArraySize; i++)
            {
                if (tiles[i] == ATile.EMPTY)
                    continue;

                var gx = i % AChunk.CHUNK_SIZE;
                var gy = i / AChunk.CHUNK_SIZE;

                var id = tiles[i];

                var tile = tileset.GetTile(id);

                var tileDriver = tile.TileDriver;

                var bitmask = input.ChunkData.bitmaskData[i];
                var variation = input.ChunkData.variations[i];
                var transform = input.ChunkData.transforms[i];
                var color = input.ChunkData.colors[i];

                lock (_lock)
                {
                    tileDriver.SetTile(new ATileDriverData()
                    {
                        x = gx,
                        y = gy,
                        color = color,
                        mesh = input.MeshData,
                        tile = tile,
                        bitmask = bitmask,
                        variation = variation,
                        tileset = tileset,
                        blend = true,
                        tileData = transform
                    });
                }


            }
        }

        public void Generate(AChunkProcessorData input)
        {
            IsRunning = true;

            _input = input;

            input.MeshData.Clear();

            if (input.ChunkData.IsEmpty)
            {
                input.MeshData.ApplyData();
                return;
            }

            var thread = new Thread(()=>ThreadGenerate(input));

            _blockThreads.Add(thread);

            thread.Start();
        }

        public void WaitComplete()
        {
            foreach (var thread in _blockThreads)
            {
                while (thread.IsAlive) { }
            }

            _blockThreads.Clear();

            _input.MeshData.ApplyData();
        }
    }
}
