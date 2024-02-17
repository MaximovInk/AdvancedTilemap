using System.Collections.Generic;
using System.Linq;

namespace MaximovInk.AdvancedTilemap
{
    public struct AChunkProcessorData
    {
        public MeshData MeshData;
        public AChunkData ChunkData;
        public AChunkPersistenceData ChunkPersistenceData;
    }

    public class ChunkProcessor 
    {
        public bool IsRunning => _processors.Any(n => n.IsRunning);

        private readonly List<IChunkProcessorJob> _processors = new();

        private AChunk _attachedChunk;

        public ChunkProcessor(AChunk attachedChunk)
        {
            _attachedChunk = attachedChunk;
        }

        public void AddJob(IChunkProcessorJob chunkProcessorJob)
        {
            _processors.Add(chunkProcessorJob);
        }

        public void ProcessData(AChunkProcessorData input)
        {
            foreach (var processorJob in _processors)
            {
                processorJob.Generate(input);
            }

            foreach (var processorJob in _processors)
            {
                processorJob.WaitComplete();
            }
        }
    }
}
