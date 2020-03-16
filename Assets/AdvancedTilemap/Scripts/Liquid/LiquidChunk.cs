using System;
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

		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;

		private MeshData meshData;

		public Chunk Chunk;

		public const float MAX_VALUE = 1F;
		public const float MIN_VALUE = 0.005F;

		public const float MAX_COMPRESSION = 0.25F;

		public const float MIN_FLOW = 0.005F;
		public const float MAX_FLOW = 4F;

		public const float FLOW_SPEED = 1F;

		public const float STABLE_FLOW = 0.0001F;

		private void OnEnable()
		{
			if (meshData == null)
				meshData = new MeshData();

			if (meshFilter == null)
				meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshRenderer == null)
				meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (Chunk == null)
				Chunk = GetComponentInParent<Chunk>();
		}

		public void Init(int w,int h,Chunk chunk)
		{
			if (meshFilter == null)
				meshFilter = gameObject.AddComponent<MeshFilter>();
			if (meshRenderer == null)
				meshRenderer = gameObject.AddComponent<MeshRenderer>();

			Chunk = chunk;
			settledData = new bool[w * h];
			data = new float[w * h];
		}

		public bool meshRebuild = false;



		public bool GetSettled(int x, int y)
		{
			return settledData[x + y * ATilemap.CHUNK_SIZE];
		}

		public void SetSettled(int x, int y,bool value)
		{

			settledData[x + y * ATilemap.CHUNK_SIZE] = value;

			meshRebuild = true;
		}

		public void AddLiquid(int x, int y, float value)
		{
			data[x + y * ATilemap.CHUNK_SIZE] += value;
			settledData[x + y * ATilemap.CHUNK_SIZE] = false;

			meshRebuild = true;
		}

		public float GetLiquid(int x, int y)
		{
			return data[x + y * ATilemap.CHUNK_SIZE];
		}

		public void SetLiquid(int x, int y, float value)
		{
			meshRebuild = true;
			data[x + y * ATilemap.CHUNK_SIZE] = value;
		}

		public void GenMesh()
		{
			meshRebuild = false;

			meshData.Clear();

			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] > 0)
				{
					int ix = i % ATilemap.CHUNK_SIZE;
					int iy = i / ATilemap.CHUNK_SIZE;
					float val = Mathf.Clamp01(data[i]);

					var topEmpty = Chunk.Layer.Tilemap.GetLiquid(ix+Chunk.GridPosX,iy+1+Chunk.GridPosY, (Chunk.Layer.Index)) == 0;

					Color color = Color.Lerp(Chunk.Layer.Tilemap.LiquidMinColor, Chunk.Layer.Tilemap.LiquidMaxColor, data[i] / 4f);
					meshData.AddSquare(Vector2.zero, Vector2.one, ix, iy, ix + 1f,iy+ (topEmpty ? val : 1), 0, 0, 0, 0, 0, color);
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
			meshData.ApplyToMesh();

			meshFilter.mesh = meshData.GetMesh();
		}

		private void LateUpdate()
		{


			if (meshRebuild)
			{
				GenMesh();
				ApplyData();
			}
		}
	}
}
