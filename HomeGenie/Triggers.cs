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

namespace HomeGenie.Enums
{
    /// <summary>
    /// An enum containing the possible events that trigger a potential change in occupancy state
    /// </summary>
    public enum Trigger
    {
        DoorOpens,
        DoorCloses,
        SensorActivity,
        AlarmFullSet,
        AlarmPartSet,
        AlarmUnset,
        OccupancyTimerExpires,
        ChildOccupied
    }
}
