using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace MaximovInk.AdvancedTilemap
{
    public class ChunkProcessor 
    {
        private const float MAX_SAVE_GEN_TIME = 0.5f;

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
