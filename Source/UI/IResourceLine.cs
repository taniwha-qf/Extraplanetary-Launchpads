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

namespace ExtraplanetaryLaunchpads {

	public interface IResourceLine
	{
		string ResourceName { get; }
		string ResourceInfo { get; }
		double ResourceFraction { get; }
		double BuildAmount { get; }
		double AvailableAmount { get; }
	}

	public interface IResourceLineAdjust
	{
		double ResourceFraction { set; }
	}
}
