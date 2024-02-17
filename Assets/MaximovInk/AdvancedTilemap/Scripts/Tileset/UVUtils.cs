namespace MaximovInk.AdvancedTilemap
{
    public static class UVUtils
    {
        public static ATileUV ApplyTransforms(ATileUV uv, UVTransform transformData)
        {
            if (transformData._rot90)
            {
                var temp = uv.LeftBottom;
                uv.LeftBottom = uv.RightBottom;
                uv.RightBottom = uv.RightTop;
                uv.RightTop = uv.LeftTop;
                uv.LeftTop = temp;
            }

            if (transformData._flipHorizontal)
            {
                var temp = uv.LeftBottom;
                uv.LeftBottom = uv.RightBottom;
                uv.RightBottom = temp;
                temp = uv.LeftTop;
                uv.LeftTop = uv.RightTop;
                uv.RightTop = temp;
            }

            if (transformData._flipVertical)
            {
                var temp = uv.LeftBottom;
                uv.LeftBottom = uv.LeftTop;
                uv.LeftTop = temp;
                temp = uv.RightBottom;
                uv.RightBottom = uv.RightTop;
                uv.RightTop = temp;
            }
            return uv;
        }
    }
}