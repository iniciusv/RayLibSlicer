using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slicer3D
{
	public class IntersectingPoint : Points
	{
		public int TriangleId;

		public IntersectingPoint(float x, float y, float z, int triangleId) : base(x, y, z)
		{
			this.TriangleId = triangleId;
		}
	}
}

