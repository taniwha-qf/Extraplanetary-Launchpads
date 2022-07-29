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
using System.Collections.Generic;
using UnityEngine;

using KodeUI;

namespace ExtraplanetaryLaunchpads {

	public class ResourceGroup : IResourceLine
	{
		RMResourceInfo resource;
#region IResourceLine
		public string ResourceName { get { return resourceName; } }
		public string ResourceInfo { get { return null; } }
		public double ResourceFraction
		{
			get {
				double maxAmount = resource.maxAmount;
				if (maxAmount <= 0) {
					return 0;
				}
				double amount = resource.amount;
				return amount / maxAmount;
			}
		}
		public double BuildAmount { get { return resource.amount; } }
		public double AvailableAmount { get { return resource.maxAmount; } }
#endregion
		public string resourceName { get; }
		public bool isOpen { get; set; }
		public ResourceModule.List modules { get; private set; }
		ResourceModule.Dict moduleDict;

		public void BuildModules (RMResourceManager manager)
		{
			resource = manager.masterSet.resources[resourceName];

			var liveSets = new HashSet<uint> ();
			for (int i = 0; i < manager.resourceSets.Count; i++) {
				var set = manager.resourceSets[i];
				if (!set.resources.ContainsKey (resourceName)) {
					continue;
				}
				liveSets.Add (set.id);
				if (moduleDict.ContainsKey (set.id)) {
					Debug.Log ($"[ResourceGroup] BuildModules updating {set.name} {set.id}");
					var mod = moduleDict[set.id];
					mod.set = set;
					mod.manager = manager;
				} else {
					Debug.Log ($"[ResourceGroup] BuildModules adding {set.name} {set.id}");
					var mod = new ResourceModule (set, resourceName, manager);
					moduleDict[set.id] = mod;
					modules.Add (mod);
				}
			}

			for (int i = modules.Count; i-- > 0; ) {
				if (!liveSets.Contains(modules[i].id)) {
					Debug.Log ($"[ResourceGroup] BuildModules removing {modules[i].name} {modules[i].id}");
					moduleDict.Remove (modules[i].id);
					modules.RemoveAt (i);
				}
			}
		}

		public ResourceGroup (string resourceName, RMResourceManager manager)
		{
			isOpen = false;
			this.resourceName = resourceName;
			modules = new ResourceModule.List ();
			moduleDict = new ResourceModule.Dict ();

			BuildModules (manager);
		}

		public class Dict : Dictionary<string, ResourceGroup> { }
		public class List : List<ResourceGroup>, UIKit.IListObject
		{
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<ELResourceGroupView> ()
						.Resource (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<ELResourceGroupView> ();
				view.Resource (this[index]);
			}
		}
	}
}
