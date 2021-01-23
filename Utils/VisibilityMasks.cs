using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera2.Utils {
	public enum VisibilityMasks {
		ThirdPerson = 1 << 3,
		FirstPerson = 1 << 6,
		UI = 1 << 5,
		Debris = 1 << 9,
		AlwaysVisible = 1 << 10, //???
		Walls = 1 << 25
	}
}
