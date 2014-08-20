using System;
using System.Globalization;
using System.Windows.Data;
using Gibbed.Avalanche.RenderBlockModel;
using RBMRender.Models;
using RBMRender.RenderBlocks;

namespace RBMRender.Converters
{
	[ValueConversion(typeof (IRenderBlock), typeof (BlockEntry))]
	public class BlockModelConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var blockEntry = new BlockEntry();

			if (value == null)
				blockEntry.Name = "Null.";
			else
				blockEntry.Name = value.GetType().Name;

			blockEntry.Block = value as IRenderBlockDrawable;

			// TODO:
			/*
			 * Move common fields into RenderBlockBase:
			 *	Materials
			 *	Vertice/Indice counts
			 *	Anything else (RIP)
			 *	
			 * Figure out which extra fields should be present in the property list
			 * and handle accordingly.
			 * */

			if (value is RenderBlockGeneral)
			{
				var general = value as RenderBlockGeneral;
				blockEntry.Properties.Add("UndeformedDiffuse", general.Block.Material.UndeformedDiffuseTexture);
				blockEntry.Properties.Add("UndeformedProperty", general.Block.Material.UndeformedPropertiesMap);
				blockEntry.Properties.Add("UndeformedNormal", general.Block.Material.UndeformedNormalMap);

				blockEntry.Properties.Add("DeformedDiffuse", general.Block.Material.DeformedDiffuseTexture);
				blockEntry.Properties.Add("DeformedProperty", general.Block.Material.DeformedPropertiesMap);
				blockEntry.Properties.Add("DeformedNormal", general.Block.Material.DeformedNormalMap);
			}

			if (value is RenderBlockSkinnedGeneral)
			{
				var general = value as RenderBlockSkinnedGeneral;

				blockEntry.Properties.Add("Version", ((int) general.Block.Version).ToString());
			}

			return blockEntry;
		}
	}
}