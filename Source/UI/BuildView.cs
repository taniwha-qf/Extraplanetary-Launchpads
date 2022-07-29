/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;

namespace ExtraplanetaryLaunchpads {

	public class ELBuildView : Layout
	{
		public class ProgressResource : IResourceLine
		{
			ELBuildControl control;
			BuildResource built;
			BuildResource required;
			RMResourceInfo available;

			public string ResourceName { get { return built.name; } }
			public string ResourceInfo
			{
				get {
					if (control.paused) {
						return ELLocalization.Paused;
					}
					double fraction = ResourceFraction;
					string percent = (fraction * 100).ToString ("G4") + "%";
					double eta = CalculateETA ();
					if (eta > 0) {
						percent += " " + EL_Utils.TimeSpanString (eta);
					}
					return percent;
				}
			}
			public double BuildAmount { get { return built.amount; } }
			public double AvailableAmount
			{
				get {
					if (available == null) {
						return 0;
					}
					return available.amount;
				}
			}
			public double ResourceFraction
			{
				get {
					double required = this.required.amount;
					double built = this.built.amount;

					if (required < 0) {
						return 0;
					}
					if (built < 0) {
						return 1;
					} else if (built <= required) {
						return (required - built) / required;
					} else {
						return 0;
					}
				}
			}

			public ProgressResource (BuildResource built, BuildResource required, RMResourceInfo pad, ELBuildControl control)
			{
				this.built = built;
				this.required = required;
				this.available = pad;
				this.control = control;
			}

			public double CalculateETA ()
			{
				double frames;
				if (built.deltaAmount <= 0) {
					return 0;
				}
				if (control.state == ELBuildControl.State.Building) {
					frames = built.amount / built.deltaAmount;
				} else {
					double remaining = built.amount;
					remaining -= required.amount;
					frames = remaining / built.deltaAmount;
				}
				return frames * TimeWarp.fixedDeltaTime;
			}
		}

		UIButton pauseButton;
		UIButton finalizeButton;
		UIButton cancelButton;

		ScrollView craftView;
		Layout selectedCraft;
		ELResourceDisplay resourceList;
		UIText craftName;

		List<IResourceLine> progressResources;

		ELBuildControl control;

		public override void CreateUI()
		{
			if (progressResources == null) {
				progressResources = new List<IResourceLine> ();
			}

			base.CreateUI ();

			var leftMin = new Vector2 (0, 0);
			var leftMax = new Vector2 (0.5f, 1);
			var rightMin = new Vector2 (0.5f, 0);
			var rightMax = new Vector2 (1, 1);

			UIScrollbar scrollbar;
			Vertical ()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)

				.Add<LayoutPanel>()
					.Vertical()
					.Padding(8)
					.ControlChildSize(true, true)
					.ChildForceExpand(false, false)
					.Anchor(AnchorPresets.HorStretchTop)
					.FlexibleLayout(true,true)
					.PreferredHeight(300)
					.Add<Layout> (out selectedCraft)
						.Horizontal ()
						.ControlChildSize(true, true)
						.ChildForceExpand(false, false)
						.FlexibleLayout(true,false)
						.Add<UIText> ()
							.Text (ELLocalization.SelectedCraft)
							.Finish ()
						.Add<UIEmpty>()
							.MinSize(15,-1)
							.Finish()
						.Add<UIText> (out craftName)
							.Finish ()
						.Finish ()
					.Add<ScrollView> (out craftView)
						.Horizontal (false)
						.Vertical (true)
						.Horizontal()
						.ControlChildSize (true, true)
						.ChildForceExpand (false, true)
						.Add<UIScrollbar> (out scrollbar, "Scrollbar")
							.Direction(Scrollbar.Direction.BottomToTop)
							.PreferredWidth (15)
							.Finish ()
						.Finish ()
					.Finish ()
				.Add<LayoutAnchor> ()
					.DoPreferredHeight (true)
					.FlexibleLayout (true, false)
					.SizeDelta (0, 0)
					.Add<UIButton> (out pauseButton)
						.Text (PauseResumeText ())
						.OnClick (PauseResume)
						.Anchor (leftMin, leftMax)
						.SizeDelta (0, 0)
						.PreferredWidth(0)
						.Finish()
					.Add<UIButton> (out finalizeButton)
						.Text (ELLocalization.FinalizeBuild)
						.OnClick (FinalizeBuild)
						.Anchor (leftMin, leftMax)
						.SizeDelta (0, 0)
						.Finish()
					.Add<UIButton> (out cancelButton)
						.Text (CancelRestartText ())
						.OnClick (CancelRestart)
						.Anchor (rightMin, rightMax)
						.SizeDelta (0, 0)
						.Finish()
					.Finish()
				.Finish();

			craftView.VerticalScrollbar = scrollbar;
			craftView.Viewport.FlexibleLayout (true, true);
			craftView.Content
				.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Anchor (AnchorPresets.HorStretchTop)
				.PreferredSizeFitter(true, false)
				.WidthDelta(0)
				.Add<ELResourceDisplay> (out resourceList)
					.Finish ()
				.Finish ();
		}

		string PauseResumeText ()
		{
			if (control == null) {
				return ELLocalization.PauseBuild;
			}
			if (control.state == ELBuildControl.State.Building) {
				if (control.paused) {
					return ELLocalization.ResumeBuild;
				} else {
					return ELLocalization.PauseBuild;
				}
			} else {
				if (control.paused) {
					return ELLocalization.ResumeTeardown;
				} else {
					return ELLocalization.PauseTeardown;
				}
			}
		}

		void FinalizeBuild ()
		{
			control.BuildAndLaunchCraft ();
		}

		void PauseResume ()
		{
			if (control.paused) {
				control.ResumeBuild ();
			} else {
				control.PauseBuild ();
			}
		}

		void CancelRestart ()
		{
			if (control.state == ELBuildControl.State.Building) {
				control.CancelBuild ();
			} else {
				control.UnCancelBuild ();
			}
		}

		string CancelRestartText ()
		{
			if (control == null) {
				return ELLocalization.CancelBuild;
			}
			if (control.state == ELBuildControl.State.Building) {
				return ELLocalization.CancelBuild;
			} else {
				return ELLocalization.RestartBuild;
			}
		}

		public void UpdateControl (ELBuildControl control)
		{
			this.control = control;
			if (control != null) {
				bool enable = false;
				bool complete = false;

				switch (control.state) {
					case ELBuildControl.State.Building:
					case ELBuildControl.State.Canceling:
						enable = true;
						complete = false;
						break;
					case ELBuildControl.State.Complete:
						enable = true;
						complete = true;;
						break;
				}
				gameObject.SetActive (enable);
				if (enable) {
					pauseButton.Text (PauseResumeText ());
					cancelButton.Text (CancelRestartText ());
					pauseButton.gameObject.SetActive (!complete);
					finalizeButton.gameObject.SetActive (complete);
					craftName.Text (control.craftName);
					StartCoroutine (WaitAndRebuildResources ());
				}
			} else {
				gameObject.SetActive (false);
			}
		}

		static BuildResource FindResource (List<BuildResource> reslist, string name)
		{
			return reslist.Where(r => r.name == name).FirstOrDefault ();
		}

		void RebuildResources ()
		{
			progressResources.Clear ();
			if (control == null || control.builtStuff == null
				|| control.buildCost == null) {
				return;
			}
			foreach (var res in control.builtStuff.required) {
				var req = FindResource (control.buildCost.required, res.name);
				var available = control.padResources[res.name];
				var line = new ProgressResource (res, req, available, control);
				progressResources.Add (line);
			}
			resourceList.Resources (progressResources);
		}

		IEnumerator WaitAndRebuildResources ()
		{
			while (control.padResources == null) {
				yield return null;
			}
			RebuildResources ();
		}
	}
}
