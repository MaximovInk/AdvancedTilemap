using UnityEngine;

namespace AdvancedTilemap.Liquid
{
	public class LiquidChunk : MonoBehaviour
	{
		[HideInInspector,SerializeField]
		private float[,] data;

		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;

		private MeshData meshData;

		public const float MAX_LIQUID = 1F;

		public void Init(int w,int h)
		{
			if (meshData == null)
				meshData = new MeshData();

			if (meshFilter == null)
				meshFilter = gameObject.AddComponent<MeshFilter>();
			if (meshRenderer == null)
				meshRenderer = gameObject.AddComponent<MeshRenderer>();

			data = new float[w, h];
		}

		public bool genMesh = false;

		public void AddLiguid(int x, int y, float value)
		{
			data[x, y] += value;
			genMesh = true;
		}

		public float GetLiquid(int x, int y)
		{
			return data[x, y];
		}

		public void SetLiquid(int x, int y, float value)
		{
			data[x, y] = value;
			genMesh = true;
		}

		private void GenMesh()
		{
			genMesh = false;

			meshData.Clear();

			int c = 0;

			for (int ix = 0; ix < data.GetLength(0); ix++)
			{
				for (int iy = 0; iy < data.GetLength(1); iy++)
				{
					if (data[ix, iy] > 0) {
						meshData.AddSquare(Vector2.zero, Vector2.one, ix, iy, ix + 1f, iy + 1f, 0, 0, 0, 0, 0, Color.white);c++;
					}
						
				}
			}
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
