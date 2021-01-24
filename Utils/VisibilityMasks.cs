using System;

namespace Camera2.Utils {
	public enum VisibilityLayers : int {
		ThirdPerson = 3,
		FirstPerson = 6,
		UI = 5,
		Debris = 9,
		Avatar = 10, //???
		Walls = 25
	}

	[Flags]
	public enum VisibilityMasks : int {
		ThirdPerson = 1 << VisibilityLayers.ThirdPerson,
		FirstPerson = 1 << VisibilityLayers.FirstPerson,
		UI = 1 << VisibilityLayers.UI,
		Debris = 1 << VisibilityLayers.Debris,
		Avatar = 1 << VisibilityLayers.Avatar, //???
		Walls = 1 << VisibilityLayers.Walls
	}
}
