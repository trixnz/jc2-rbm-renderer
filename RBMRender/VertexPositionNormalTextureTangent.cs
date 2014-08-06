using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace RBMRender
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionNormalTextureTangent
	{
		public static readonly int Size = Vector3.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes +
		                                  Vector2.SizeInBytes;

		[VertexElement("SV_Position")]
		public Vector3 Position;
		[VertexElement("NORMAL")]
		public Vector3 Normal;
		[VertexElement("TANGENT")]
		public Vector3 Tangent;
		[VertexElement("TEXCOORD0")]
		public Vector2 TextureCoordinate;

		public VertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 uv)
		{
			Position = position;
			Normal = normal;
			Tangent = tangent;
			TextureCoordinate = uv;
		}

		public VertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector2 uv)
		{
			Position = position;
			Normal = normal;
			Tangent = Vector3.Zero;
			TextureCoordinate = uv;
		}
	}
}