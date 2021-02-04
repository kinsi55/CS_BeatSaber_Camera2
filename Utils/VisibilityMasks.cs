using System;

namespace Camera2.Utils {
	enum VisibilityLayers : int {
		ThirdPerson = 3,
		Floor = 4, // Called "Water" ingame
		FirstPerson = 6,
		UI = 5,
		Notes = 8,
		Debris = 9,
		Avatar = 10,
		Walls = 11,
		WallTextures = 25
	}

	[Flags]
	enum VisibilityMasks : int {
		ThirdPersonAvatar = 1 << VisibilityLayers.ThirdPerson,
		FirstPersonAvatar = 1 << VisibilityLayers.FirstPerson,
		Floor = 1 << VisibilityLayers.Floor, // Called "Water" ingame
		UI = 1 << VisibilityLayers.UI,
		Notes = 1 << VisibilityLayers.Notes,
		Debris = 1 << VisibilityLayers.Debris,
		Avatar = 1 << VisibilityLayers.Avatar,
		Walls = 1 << VisibilityLayers.Walls,
		WallTextures = 1 << VisibilityLayers.WallTextures
	}
}
