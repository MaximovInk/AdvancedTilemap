using UnityEngine;

namespace AdvancedTilemap.Liquid
{
	[ExecuteAlways]
	public class LiquidChunk : MonoBehaviour
	{
		[HideInInspector,SerializeField]
		private float[] data;
		[HideInInspector, SerializeField]
		private bool[] settledData;
		[HideInInspector, SerializeField]
		private byte[] settleCount;

		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;

		private MeshData meshData;

		public const float MAX_VALUE = 1F;
		public const float MIN_VALUE = 0.005F;

		public const float MAX_COMPRESSION = 0.25F;

		public const float MIN_FLOW = 0.005F;
		public const float MAX_FLOW = 4F;

		public const float FLOW_SPEED = 1F;

		private void OnEnable()
		{
			if (meshData == null)
				meshData = new MeshData();

			if (meshFilter == null)
				meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshRenderer == null)
				meshRenderer = gameObject.GetComponent<MeshRenderer>();
		}

		public void Init(int w,int h)
		{
			if (meshFilter == null)
				meshFilter = gameObject.AddComponent<MeshFilter>();
			if (meshRenderer == null)
				meshRenderer = gameObject.AddComponent<MeshRenderer>();

			data = new float[w*h];
			settledData = new bool[w * h];
			settleCount = new byte[w * h];
		}

		public bool genMesh = false;

		public bool GetSettled(int x, int y)
		{
			return settledData[x + y * ATilemap.CHUNK_SIZE];
		}

		public void SetSettled(int x, int y,bool value)
		{
			if (!value)
				SetSettleCount(x, y, 0);

			settledData[x + y * ATilemap.CHUNK_SIZE] = value;
		}


		public byte GetSettleCount(int x, int y)
		{
			return settleCount[x + y * ATilemap.CHUNK_SIZE];
		}

		public void SetSettleCount(int x, int y, byte value)
		{
			settleCount[x + y * ATilemap.CHUNK_SIZE] = value;
		}

		public void AddLiquid(int x, int y, float value)
		{
			data[x + y * ATilemap.CHUNK_SIZE] = Mathf.Clamp(data[x + y * ATilemap.CHUNK_SIZE]+value,0,MAX_VALUE);
			settledData[x + y * ATilemap.CHUNK_SIZE] = false;
			genMesh = true;
		}

		public float GetLiquid(int x, int y)
		{
			return data[x + y * ATilemap.CHUNK_SIZE];
		}

		public void SetLiquid(int x, int y, float value)
		{
			data[x + y * ATilemap.CHUNK_SIZE] = Mathf.Clamp(value,0,MAX_VALUE);
			genMesh = true;
		}

		private void GenMesh()
		{
			genMesh = false;

			meshData.Clear();

			int c = 0;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] > 0)
				{
					var ix = i % ATilemap.CHUNK_SIZE;
					var iy = i / ATilemap.CHUNK_SIZE;
					meshData.AddSquare(Vector2.zero, Vector2.one, ix, iy, ix + 1f, iy + data[i], 0, 0, 0, 0, 0, Color.white); c++;
				}
			}
		}

		public void SetMaterial(Material material)
		{
			if (material == null)
				meshRenderer.sharedMaterial = null;
			else
				meshRenderer.sharedMaterial = new Material(material);
		}

		public void ApplyData()
		{
			//if(genMesh)
				GenMesh();

			meshData.ApplyToMesh();

			meshFilter.mesh = meshData.GetMesh();
		}
	}
}
