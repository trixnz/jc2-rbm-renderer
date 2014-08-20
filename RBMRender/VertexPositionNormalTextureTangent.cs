using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace RBMRender
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionNormalTextureTangent
	{
		[VertexElement("SV_Position")]
		public Vector3 Position;
		[VertexElement("NORMAL")]
		public Vector3 Normal;
		[VertexElement("TANGENT")]
		public Vector3 Tangent;

		[VertexElement("TEXCOORD0")]
		public Vector2 TextureCoordinate;	
		[VertexElement("TEXCOORD1")]
		public Vector2 TextureCoordinate2;

		[VertexElement("COLOR0")]
		public Vector3 VertexColor;

		[VertexElement("TEXCOORD2")]
		public Vector4 ChannelMask;
		[VertexElement("TEXCOORD3")]
		public Vector2 SpecularPower;

		[VertexElement("TEXCOORD4")]
		public Vector4 BoneWeights;
		[VertexElement("TEXCOORD5")]
		public Vector4 BoneIndices;

		public VertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 uv)
		{
			Position = position;
			Normal = normal;
			TextureCoordinate = uv;
			TextureCoordinate2 = Vector2.Zero;
			Tangent = Vector3.Zero;
			ChannelMask = Vector4.Zero;
			SpecularPower = Vector2.Zero;
			VertexColor = Vector3.Zero;

			BoneWeights = Vector4.Zero;
			BoneIndices= Vector4.Zero;
		}

		public VertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector2 uv)
		{
			Position = position;
			Normal = normal;
			Tangent = Vector3.Zero;
			TextureCoordinate = uv;
			TextureCoordinate2 = Vector2.Zero;
			ChannelMask = Vector4.Zero;
			SpecularPower = Vector2.Zero;
			VertexColor = Vector3.Zero;

			BoneWeights = Vector4.Zero;
			BoneIndices = Vector4.Zero;
		}
	}
}