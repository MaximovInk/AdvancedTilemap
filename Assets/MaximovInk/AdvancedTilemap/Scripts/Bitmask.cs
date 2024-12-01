namespace MaximovInk
{
    public static class Bitmask
    {
        /*
  1 | 2 | 4
  8 | t | 16
  32| 64| 128
        */

        public const byte LEFT_TOP = 1;
        public const byte TOP = 2;
        public const byte RIGHT_TOP = 4;
        public const byte LEFT = 8;
        public const byte RIGHT = 16;
        public const byte LEFT_BOTTOM = 32;
        public const byte BOTTOM = 64;
        public const byte RIGHT_BOTTOM = 128;
        public const byte FILL = 255;

        public static bool HasBit(this byte bitmask, byte position)
        {
            return (bitmask & position) != 0;
        }
    }
}
