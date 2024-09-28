﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using Camera2.Managers;
using HMUI;
using System.Collections.Generic;
using System.Linq;

namespace Camera2.UI {
	class CustomSceneUIEntry {
		public string _name { get; private set; }

		public CustomSceneUIEntry(string name) {
			_name = name;
			bg = null;
		}

		bool exists => ScenesManager.settings.customScenes.ContainsKey(name);

		string name => _name == null ? "Switch to default scene" : _name;

		string camCount {
			get {
				if(!exists)
					return "";

				var c = ScenesManager.settings.customScenes[name].Count;

				var s = $"{c} camera";

				if(c == 1) return s;

				return s + "s";
			}
		}

		string camNames => !exists ? "" : string.Join(", ", ScenesManager.settings.customScenes[name]);

		[UIComponent("bgContainer")] ImageView bg;

		[UIAction("refresh-visuals")]
		public void Refresh(bool selected, bool highlighted) {
			var x = new UnityEngine.Color(0, 0, 0, 0.45f);

			if(selected || highlighted)
				x.a = selected ? 0.9f : 0.6f;

			bg.color = x;
		}
	}

	class CustomScenesSwitchUI {
		[UIComponent("customScenesList")] public CustomCellListTableData list = null;
		[UIValue("scenes")] List<object> scenes =>
			ScenesManager.settings.customScenes.Keys.Select(x => new CustomSceneUIEntry(x))
				.Prepend(new CustomSceneUIEntry(null))
				.Cast<object>()
				.ToList();

		void SwitchToCustomScene(TableView tableView, CustomSceneUIEntry row) {
			if(row._name == null) {
				ScenesManager.LoadGameScene(forceReload: true);
				return;
			}

			ScenesManager.SwitchToCustomScene(row._name);
		}

		public void Update(int setSelected = -1, bool reloadData = true) {
			if(list == null || list.TableView == null)
				return;

			if(reloadData) {
				list.Data = scenes;
				list.TableView.ReloadData();
			}

			if(setSelected > -1) {
				list.TableView.SelectCellWithIdx(setSelected);
				list.TableView.ScrollToCellWithIdx(setSelected, TableView.ScrollPositionType.Center, false);
			}
		}
	}
}
