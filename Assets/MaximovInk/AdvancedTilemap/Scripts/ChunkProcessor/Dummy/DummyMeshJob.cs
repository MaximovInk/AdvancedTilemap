using System;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class DummyMeshJob : IChunkProcessorJob
    {
        public string Name => "DummyMeshProcessor";
        public bool IsRunning { get; protected set; }
        public bool IsValid => true;

        private AChunkProcessorData _input;


        public void Generate(AChunkProcessorData input)
        {
            IsRunning = true;

            _input = input;

            input.MeshData.Clear();

            if (input.ChunkData.IsEmpty)
            {
                input.MeshData.ApplyData();
                IsRunning = false;
                return;
            }

            var tiles = input.ChunkData.tiles;

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

                var bitmask = input.ChunkData.bitmask[i];
                var selfBitmask = input.ChunkData.selfBitmask[i];

                switch (tile.BitmaskMode)
                {
                    case BitmaskMode.Self:
                        bitmask = selfBitmask;
                        break;
                    case BitmaskMode.All:
                        //already
                        //bitmask = bitmask;
                        break;
                    case BitmaskMode.Other:
                        //I don't tested this [
                        bitmask = (byte)(bitmask & ~selfBitmask);
                        break;
                }

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
                    selfBitmask = selfBitmask,
                    variation = variation,
                    tileset = tileset,
                    tileData = transform,
                    chunkX = input.Chunk.GridX,
                    chunkY = input.Chunk.GridY,
                });
            }


            IsRunning = false;
        }

        public void WaitComplete()
        {
            _input.MeshData.ApplyData();
        }
    }
}
