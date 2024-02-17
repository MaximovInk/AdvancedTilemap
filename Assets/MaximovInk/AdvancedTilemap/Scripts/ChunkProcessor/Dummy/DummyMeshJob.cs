using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class DummyMeshJob : IChunkProcessorJob
    {
        public string Name => "DummyMeshProcessorJob";
        public bool IsRunning { get; protected set; }
        public bool IsValid => true;

        public void Generate(AChunkProcessorData input)
        {
            IsRunning = true;

            input.MeshData.Clear();

            if (input.ChunkData.IsEmpty)
            {
                input.MeshData.ApplyData();
                IsRunning = false;
                return;
            }

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

            input.MeshData.ApplyData();

            IsRunning = false;
        }

        public void WaitComplete()
        {
            while (IsRunning){}
        }
    }
}
