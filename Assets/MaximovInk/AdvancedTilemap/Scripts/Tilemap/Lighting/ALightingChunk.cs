using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class ALightingChunk : MonoBehaviour
    {
        public bool meshIsDirty = false;

        [HideInInspector, SerializeField]
        public AChunk Chunk;
        [HideInInspector, SerializeField]
        private byte[] data;
        /*[HideInInspector, SerializeField]
        private MeshFilter meshFilter;
        [HideInInspector, SerializeField]
        private MeshRenderer meshRenderer;
*/

        [HideInInspector, SerializeField]
        private MeshData meshData;

        [HideInInspector, SerializeField]
        private MeshFilter meshFilter;
        [HideInInspector, SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Texture2D _texture;

        [SerializeField, HideInInspector] private int _width;
        [SerializeField, HideInInspector] private int _height;

        private void OnEnable()
        {
            if (meshFilter == null)
                meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (Chunk == null)
                Chunk = GetComponentInParent<AChunk>();

        }

        public void Init(int w, int h, AChunk chunk)
        {
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Chunk = chunk;

            _width = w;
            _height = h;

            data = new byte[w * h];

            ValidateMesh();
            ValidateTexture();

            gameObject.layer = Chunk.Layer.Tilemap.Lighting.LightingMask;


        }

        private void ValidateTexture()
        {
            if (_texture == null)
            {
                _texture = new Texture2D(_width, _height);

                for (var i = 0; i < data.Length; i++)
                {
                    var ix = i % AChunk.CHUNK_SIZE;
                    var iy = i / AChunk.CHUNK_SIZE;
                    _texture.SetPixel(ix, iy, Color.clear);
                }

                _texture.Apply();
            }

            _texture.filterMode = FilterMode.Point;


        }

        private void ValidateMesh()
        {
            if (meshData == null)
            {
                meshData = new MeshData();

                meshData.AddSquare(new MeshDataParameters
                {
                    color = Color.white,
                    uv = ATileUV.Identity,
                    vX0 = 0,
                    vX1 = AChunk.CHUNK_SIZE,
                    vY0 = 0,
                    vY1 = AChunk.CHUNK_SIZE,
                    z = 0,
                    unit = Chunk.Layer.Tileset.GetTileUnit()
                });

                meshData.ApplyData();

                meshFilter.sharedMesh = meshData.GetMesh();

            }
        }

        public void SetMaterial(Material material)
        {
            meshRenderer.sharedMaterial = material == null ?
                null : new Material(material);

            if (meshRenderer.sharedMaterial != null)
                meshRenderer.sharedMaterial.mainTexture = _texture;
        }

        public byte GetLight(int x, int y)
        {
            return data[x + y * AChunk.CHUNK_SIZE];
        }

        public void SetLight(int x, int y, byte value)
        {
            meshIsDirty = true;
            data[x + y * AChunk.CHUNK_SIZE] = value;
        }

        public void Generate()
        {
            ValidateTexture();
            ValidateMesh();

            for (var i = 0; i < data.Length; i++)
            {
                var ix = i % AChunk.CHUNK_SIZE;
                var iy = i / AChunk.CHUNK_SIZE;
                var value =  Mathf.Clamp01(data[i] / 255f);

                _texture.SetPixel(ix,iy, Color.Lerp(Color.clear, Color.white, value));
            }

            _texture.Apply();
        }

        private void LateUpdate()
        {
            if (meshIsDirty)
            {
                meshIsDirty = false;

                Generate();
            }
        }
    }
}
