using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slicer3D.Domain
{
	public class Layers
	{
		public float LayerHeight { get; set; }
		public int TopSolidLayers { get; set; }
		public int BottomSolidLayers { get; set; }
		public int PerimeterShells { get; set; }
		public OutlineDirection Direction { get; set; }

		public Layers(float layerHeight, int topSolidLayers, int bottomSolidLayers, int perimeterShells, OutlineDirection direction)
		{
			LayerHeight = layerHeight;
			TopSolidLayers = topSolidLayers;
			BottomSolidLayers = bottomSolidLayers;
			PerimeterShells = perimeterShells;
			Direction = direction;
		}
	}

	public enum OutlineDirection
	{
		InsideOut,
		OutsideIn
	}

}
