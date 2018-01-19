// <copyright file="Store.cs" company="Bounz">
// This file is part of HomeGenie-BE Project source code.
//
// HomeGenie-BE is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// HomeGenie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with HomeGenie-BE.  If not, see http://www.gnu.org/licenses.
//
//  Project Homepage: https://github.com/Bounz/HomeGenie-BE
//
//  Forked from Homegenie by Generoso Martello gene@homegenie.it
// </copyright>

namespace HomeGenie
{
    using System;
    using HomeGenie.Data;
    using HomeGenie.Service;

    [Serializable]
    public class Store
    {
        public string Name;
        public string Description;
        public TsList<ModuleParameter> Data;

        public Store()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.Data = new TsList<ModuleParameter>();
        }

        public Store(string name, string description = "")
        {
            this.Name = name;
            this.Description = description;
            this.Data = new TsList<ModuleParameter>();
        }
    }
}
