// <copyright file="StoreHelper.cs" company="Bounz">
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

namespace HomeGenie.Automation.Scripting
{
    using System;
    using HomeGenie.Data;
    using HomeGenie.Service;

    /// <summary>
    /// Store helper class.\n
    /// Class instance accessor: **Program.Store(<store_name>)**
    /// </summary>
    [Serializable]
    public class StoreHelper
    {
        private TsList<Store> storeList;
        private string storeName;

        public StoreHelper(TsList<Store> storageList, string storeName)
        {
            this.storeList = storageList;
            this.storeName = storeName;
        }

        /// <summary>
        /// Get the specified parameterName from the Store.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        public ModuleParameter Get(string parameterName)
        {
            var store = GetStore(storeName);
            ModuleParameter value = null;
            value = Service.Utility.ModuleParameterGet(store.Data, parameterName);

            // create parameter if does not exists
            if (value == null)
            {
                value = Service.Utility.ModuleParameterSet(store.Data, parameterName, string.Empty);
            }

            return value;
        }

        /// <summary>
        /// Gets the list of parameters defined in the Store.
        /// </summary>
        /// <value>The list.</value>
        public TsList<ModuleParameter> List
        {
            get
            {
                var store = GetStore(this.storeName);
                return store.Data;
            }
        }

        /// <summary>
        /// Remove the specified parameterName from the Store.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        public StoreHelper Remove(string parameterName)
        {
            var store = GetStore(storeName);
            store.Data.RemoveAll(d => d.Name == parameterName);
            return this;
        }

        /// <summary>
        /// Remove all parameters from this store.
        /// </summary>
        public void Reset()
        {
            storeList.RemoveAll(s => s.Name == storeName);
        }

        private Store GetStore(string storeName)
        {
            var store = storeList.Find(s => s.Name == storeName);

            // create store if does not exists
            if (store == null)
            {
                store = new Store(storeName);
                storeList.Add(store);
            }

            return store;
        }
    }
}
