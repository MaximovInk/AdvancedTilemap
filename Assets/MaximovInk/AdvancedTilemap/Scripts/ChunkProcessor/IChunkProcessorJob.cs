namespace MaximovInk.AdvancedTilemap
{
    public interface IChunkProcessorJob
    {
        public string Name { get; }
        public bool IsRunning { get; }
        public bool IsValid { get; }
        
        public void Generate(AChunkProcessorData input);

        public void WaitComplete();
    }
}
