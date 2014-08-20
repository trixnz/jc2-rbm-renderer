using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.Avalanche.FileFormats;
using Gibbed.Avalanche.RenderBlockModel.Blocks;
using Gibbed.IO;
using Havok;
using Havok.Animation;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using DepthStencilState = SharpDX.Toolkit.Graphics.DepthStencilState;
using Vector4 = SharpDX.Vector4;

namespace RBMRender.RenderBlocks
{
	public class RenderBlockSkinnedGeneral : RenderBlockBase<SkinnedGeneral>
	{
		private static readonly HavokWrapper HavokWrapper = new HavokWrapper();
		private static HkaSkeleton _skeleton;
		private static HkaAnimationBinding _animationBinding;
		private static Animator _animator;
		// Shitty hack. Mark the creator of the animator so we don't play it super fast for each block
		private static RenderBlockSkinnedGeneral _ownerObj;
		private static readonly List<Bone> Bones = new List<Bone>();
		private float _t;

		public RenderBlockSkinnedGeneral(GameWorld game, SmallArchiveWrapper smallArchive, SkinnedGeneral block)
			: base(game, smallArchive, block)
		{
			Matrix.Translation(0, 0, 0);
		}

		public override void Load(List<VertexPositionNormalTextureTangent> vertices, List<short> indices)
		{
			if (Block.HasBigVertices)
			{
				for (int i = 0; i < Block.VertexData0Big.Count; i++)
				{
					SkinnedGeneralData0Big generalDataBig = Block.VertexData0Big[i];
					SkinnedGeneralData1 generalData = Block.VertexData1[i];

					var vertex = new VertexPositionNormalTextureTangent();
					vertex.Position = new Vector3(generalDataBig.PositionX, generalDataBig.PositionY, generalDataBig.PositionZ);
					vertex.Normal = Vector3.Zero;
					vertex.TextureCoordinate = new Vector2(generalData.U, generalData.V);
					vertices.Add(vertex);
				}
			}
			else
			{
				for (int i = 0; i < Block.VertexData0Small.Count; i++)
				{
					SkinnedGeneralData0Small generalDataSmall = Block.VertexData0Small[i];
					SkinnedGeneralData1 generalData = Block.VertexData1[i];

					var vertex = new VertexPositionNormalTextureTangent();
					vertex.Position = new Vector3(generalDataSmall.PositionX, generalDataSmall.PositionY, generalDataSmall.PositionZ);

					vertex.Normal = new Vector3(generalData.Normal.X, generalData.Normal.Y, generalData.Normal.Z);
					vertex.Tangent = new Vector3(generalData.Tangent.X, generalData.Tangent.Y, generalData.Tangent.Z);

					vertex.TextureCoordinate = new Vector2(generalData.U, generalData.V);

					var boneWeight = new Vector4(generalDataSmall.TexCoord1A, generalDataSmall.TexCoord1B, generalDataSmall.TexCoord1C,
						generalDataSmall.TexCoord1D);
					boneWeight *= 1/255.0f;

					float r1 = 1.0f/Vector4.Dot(Vector4.One, boneWeight);
					Vector4 translatedWeight = boneWeight*r1;
					vertex.BoneWeights = translatedWeight;

					var boneIndices = new Vector4(generalDataSmall.TexCoord2A, generalDataSmall.TexCoord2B, generalDataSmall.TexCoord2C,
						generalDataSmall.TexCoord2D);
					vertex.BoneIndices = boneIndices;

					vertices.Add(vertex);
				}
			}

			indices.AddRange(Block.Faces);

			if (_skeleton == null)
			{
				_ownerObj = this;

				var skeletonFile = ArchiveManager.Instance.GetArchive("pc0.tab")
					.LoadFile(@"animations\skeletons\syncreadthis\biped.bsk");

				_skeleton = HavokWrapper.LoadAnimationContainer(skeletonFile).GetSkeleton(0);

				LoadAnimation();

				HkaBone[] hkaBones = _skeleton.Bones;
				HkQsTransform[] hkaReference = _skeleton.ReferencePose;
				short[] hkaParents = _skeleton.ParentIndices;

				for (int i = 0; i < _skeleton.NumBones; i++)
				{
					var translation = new Vector3(hkaReference[i].Translation.X, hkaReference[i].Translation.Y,
						hkaReference[i].Translation.Z);
					var rotation = new Quaternion(hkaReference[i].Rotation.X, hkaReference[i].Rotation.Y,
						hkaReference[i].Rotation.Z, hkaReference[i].Rotation.W);
					var scale = new Vector3(hkaReference[i].Scale.X, hkaReference[i].Scale.Y, hkaReference[i].Scale.Z);

					Matrix transformation = // Matrix.Scaling(scale)*Matrix.RotationQuaternion(rotation)*
						Matrix.Translation(translation);

					var bone = new Bone
					           {
						           Index = Bones.Count,
						           Name = hkaBones[i].Name,
						           Transformation = transformation
					           };
					Bones.Add(bone);
				}

				for (int i = 0; i < Bones.Count; i++)
				{
					Bone bone = Bones[i];
					short parentIndex = hkaParents[i];

					if (parentIndex != -1)
					{
						// Fixup parent reference
						bone.Parent = Bones[parentIndex];

						// Combine the parent's combined matrix with the current bone
						bone.CombinedTransformation = bone.Transformation*Bones[parentIndex].CombinedTransformation;
					}
					else
					{
						bone.CombinedTransformation = bone.Transformation;
					}

					bone.BindPose = bone.CombinedTransformation;
					bone.InvBindPose = Matrix.Invert(bone.BindPose);
				}
			}
		}

		private void LoadAnimation()
		{
			if (_animator == null)
			{
				var archive = ArchiveManager.Instance.GetArchive("pc0.tab");
				var animationArchive = new SmallArchiveWrapper
				                       {
					                       SmallArchive = new SmallArchiveFile(),
					                       Reader = new MemoryStream(archive.LoadFile(@"global\animations.blz"))
				                       };
				animationArchive.SmallArchive.Deserialize(animationArchive.Reader);

				var entry = animationArchive.SmallArchive.Entries.FirstOrDefault(e => e.Name == "run_fwd.ban");
				animationArchive.Reader.Seek(entry.Offset, SeekOrigin.Begin);

				var animationFile = animationArchive.Reader.ReadBytes(entry.Size);
				HkaAnimationContainer animationContainer = HavokWrapper.LoadAnimationContainer(animationFile);

				_animationBinding = animationContainer.GetAnimationBinding(0);

				_animator = new Animator(_skeleton, _animationBinding);
			}
		}

		public override void Draw(ref int baseVertex, ref int baseIndex)
		{
			SetTexture("DiffuseTexture", Block.Material.UndeformedDiffuseTexture);
			SetTexture("PropertiesTexture", Block.Material.UndeformedPropertiesMap);
			SetTexture("NormalsTexture", Block.Material.UndeformedNormalMap);

			if (_ownerObj == this)
				UpdateBonePositions();

			int paletteIndex = 0;
			var matrixIndices = new Matrix[18];
			foreach (short boneIndex in Block.SkinBatches[0].BoneIndices)
			{
				if (boneIndex != -1)
				{
					matrixIndices[paletteIndex] = Bones[boneIndex].AnimationTransform;
				}

				paletteIndex++;
			}
			Game.NormalMappingEffect.Parameters["BoneMatrices"].SetValue(matrixIndices);

			if (_t > Math.PI*2)
				_t -= (float) Math.PI*2;
			if (_t < Math.PI*2)
				_t += (float) Math.PI*2;
			_t += 0.05f;

			foreach (EffectPass pass in Game.NormalMappingEffect.Techniques["SkinnedGeneral"].Passes)
			{
				pass.Apply();

				Game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, Block.Faces.Count, baseIndex, baseVertex);
			}

			if (GlobalSettings.Instance.NormalDebugging)
				base.DrawNormals(Block.Faces.Count, baseIndex, baseVertex);
			
			baseIndex += Block.Faces.Count;
			if (Block.HasBigVertices)
				baseVertex += Block.VertexData0Big.Count;
			else
				baseVertex += Block.VertexData0Small.Count;
		}

		private void UpdateBonePositions()
		{
			// Uncomment to play with animations
			/*
			HkaPose pose = _animator.Step(0.006f);

			foreach (Bone bone in Bones)
			{
				HkQsTransform transform = pose.ModelSpace[bone.Index];

				var rotation = new Quaternion(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, transform.Rotation.W);
				var translation = new Vector3(transform.Translation.X, transform.Translation.Y, transform.Translation.Z);
				var scale = new Vector3(transform.Scale.X, transform.Scale.Y, transform.Scale.Z);

				bone.CombinedTransformation = // Matrix.Scaling(scale)*Matrix.RotationQuaternion(rotation)*
					Matrix.Translation(translation);
			}

			foreach (Bone bone in Bones)
			{
				bone.AnimationTransform = bone.CombinedTransformation*bone.InvBindPose;
			}
			*/
		}

		public void DrawBones()
		{
			if (_skeleton != null)
			{
				DepthStencilStateDescription desc = DepthStencilStateDescription.Default();
				desc.IsDepthEnabled = false;

				DepthStencilState state = DepthStencilState.New(Game.GraphicsDevice, desc);
				Game.GraphicsDevice.SetDepthStencilState(state);

				var vertices = new List<VertexPositionColor>();
				foreach (Bone bone in Bones)
				{
					Vector3 start = bone.CombinedTransformation.TranslationVector;
					Vector3 end = Vector3.Zero;
					if (bone.Parent != null)
						end = bone.Parent.CombinedTransformation.TranslationVector;

					vertices.AddRange(new[]
					                  {
						                  new VertexPositionColor(start, Color.Yellow),
						                  new VertexPositionColor(end, Color.Yellow)
					                  });
				}

				using (var f = new PrimitiveBatch<VertexPositionColor>(Game.GraphicsDevice))
				{
					Game.NormalMappingEffect.Techniques["Basic"].Passes[0].Apply();
					f.Begin();
					f.Draw(PrimitiveType.LineList, vertices.ToArray());
					f.End();

					Game.Batch.Begin();
					for (int i = 0; i < vertices.Count; i += 2)
					{
						VertexPositionColor vert = vertices[i];

						Vector3 pos = Game.GraphicsDevice.Viewport.Project(vert.Position, Game.Camera.Projection, Game.Camera.View,
							Game.Camera.World);

						Game.Batch.DrawString(Game.Font, _skeleton.Bones.ElementAt(i/2).Name, new Vector2(pos.X, pos.Y), Color.Red, 0,
							Vector2.Zero, 0.5f, 0, 0);
					}
					Game.Batch.End();
				}
			}
		}

		private class Bone
		{
			public int Index { get; set; }
			public string Name { get; set; }

			public Matrix Transformation { get; set; }
			public Matrix CombinedTransformation { get; set; }

			public Matrix BindPose { get; set; }
			public Matrix InvBindPose { get; set; }

			public Matrix AnimationTransform { get; set; }

			public Bone Parent { get; set; }
		}
	}
}