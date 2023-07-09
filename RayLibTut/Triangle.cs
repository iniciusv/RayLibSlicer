using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slicer3D
{
	public class Triangle
	{
		public Points Poin1;
		public Points Poin2;
		public Points Poin3;
		public Points Normal;
		public int Id;

		public Triangle(int id)
		{
			Poin1 = new Points();
			Poin2 = new Points();
			Poin3 = new Points();
			Id = id;
		}
	}

}
