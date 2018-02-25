/*
    This file is part of HomeGenie Project source code.

    HomeGenie is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HomeGenie is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with HomeGenie.  If not, see <http://www.gnu.org/licenses/>.  
*/

using HomeGenie.Data;
using System;
using System.Collections.Generic;
using HomeGenie.Service;

namespace HomeGenie.Automation.Scripting
{
    [Serializable]
    public class LocationHelper
    {
        private List<Data.Location> location = null;

        public LocationHelper(List<Location> location)
        {
            this.location = location;
        }

        public List<Location> GetLocations()
        {
            return location;
        }
    }
}
