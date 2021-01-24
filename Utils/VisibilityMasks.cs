using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera2.Utils {
	public enum VisibilityLayers {
		ThirdPerson = 3,
		FirstPerson = 6,
		UI = 5,
		Debris = 9,
		AlwaysVisible = 10, //???
		Walls = 25
	}

	public enum VisibilityMasks {
		ThirdPerson = 1 << VisibilityLayers.ThirdPerson,
		FirstPerson = 1 << VisibilityLayers.FirstPerson,
		UI = 1 << VisibilityLayers.UI,
		Debris = 1 << VisibilityLayers.Debris,
		AlwaysVisible = 1 << VisibilityLayers.AlwaysVisible, //???
		Walls = 1 << VisibilityLayers.Walls
	}
}
