namespace MaximovInk
{
    public static class Bitmask
    {
        public const byte LEFT_TOP = 1;
        public const byte TOP = 2;
        public const byte RIGHT_TOP = 4;
        public const byte LEFT = 8;
        public const byte RIGHT = 16;
        public const byte LEFT_BOTTOM = 32;
        public const byte BOTTOM = 64;
        public const byte RIGHT_BOTTOM = 128;

        public static bool HasBit(byte bitmask, byte position)
        {
            return (bitmask & position) == position;
        }
    }
}
