﻿// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (c) 2007-2016 Pivotal Software, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//  The contents of this file are subject to the Mozilla Public License
//  Version 1.1 (the "License"); you may not use this file except in
//  compliance with the License. You may obtain a copy of the License
//  at http://www.mozilla.org/MPL/
//
//  Software distributed under the License is distributed on an "AS IS"
//  basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See
//  the License for the specific language governing rights and
//  limitations under the License.
//
//  The Original Code is RabbitMQ.
//
//  The Initial Developer of the Original Code is Pivotal Software, Inc.
//  Copyright (c) 2013-2016 Pivotal Software, Inc.  All rights reserved.
//---------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace RabbitMQ.Util
{
    internal class SynchronizedList<T> : IList<T>
    {
        private readonly IList<T> list;

        internal SynchronizedList()
            : this(new List<T>())
        {
        }

        internal SynchronizedList(IList<T> list)
        {
            this.list = list;
            SyncRoot = new object();
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return list.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return list.IsReadOnly; }
        }

        public T this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    return list[index];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    list[index] = value;
                }
            }
        }

        public object SyncRoot { get; private set; }

        public void Add(T item)
        {
            lock (SyncRoot)
            {
                list.Add(item);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                list.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                list.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                return list.Remove(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return list.GetEnumerator();
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return list.GetEnumerator();
            }
        }

        public int IndexOf(T item)
        {
            lock (SyncRoot)
            {
                return list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (SyncRoot)
            {
                list.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                list.RemoveAt(index);
            }
        }
    }
}
